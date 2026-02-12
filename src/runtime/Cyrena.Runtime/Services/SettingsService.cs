using Newtonsoft.Json;
using Cyrena.Contracts;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;

namespace Cyrena.Runtime.Services
{
    internal class SettingsService : ISettingsService
    {
        private readonly string _dir;
        private readonly object _lock = new();

        public SettingsService(string dir)
        {
            _dir = dir;
            Directory.CreateDirectory(_dir);
        }

        public T? Read<T>(string key) where T : class
        {
            lock (_lock)
            {
                var path = GetPath(key);
                if (!File.Exists(path))
                    return null;

                try
                {
                    var encrypted = File.ReadAllBytes(path);
                    var json = Decrypt(encrypted);
                    return JsonConvert.DeserializeObject<T>(json);
                }
                catch
                {
                    return null;
                }
            }
        }

        public void Save<T>(string key, T value) where T : class
        {
            lock (_lock)
            {
                var path = GetPath(key);
                var json = JsonConvert.SerializeObject(value);
                var encrypted = Encrypt(json);
                File.WriteAllBytes(path, encrypted);
            }
        }

        private string GetPath(string key)
            => Path.Combine(_dir, $"{key}.settings");

        private byte[] Encrypt(string plaintext)
        {
            var data = Encoding.UTF8.GetBytes(plaintext);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return ProtectedData.Protect(
                    data,
                    null,
                    DataProtectionScope.CurrentUser);
            }
            else
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKey();
                aes.GenerateIV();

                using var encryptor = aes.CreateEncryptor();
                var cipher = encryptor.TransformFinalBlock(data, 0, data.Length);

                var result = new byte[aes.IV.Length + cipher.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);

                return result;
            }
        }

        private string Decrypt(byte[] encrypted)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var decrypted = ProtectedData.Unprotect(
                    encrypted,
                    null,
                    DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(decrypted);
            }
            else
            {
                using var aes = Aes.Create();
                aes.Key = DeriveKey();

                var iv = new byte[16];
                Buffer.BlockCopy(encrypted, 0, iv, 0, 16);

                var cipher = new byte[encrypted.Length - 16];
                Buffer.BlockCopy(encrypted, 16, cipher, 0, cipher.Length);

                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor();
                var decrypted = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
        }

        private byte[] DeriveKey()
        {
            var combined = $"{Environment.MachineName}:{Environment.UserName}";
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(combined));
        }
    }
}
