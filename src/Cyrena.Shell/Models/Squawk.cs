using System;

namespace Cyrena.Shell.Models
{
    /// <summary>
    /// Ensure that user who started the application is the one interacting with it
    /// </summary>
    public class Squawk
    {
        public const string Key = "squawk";
        public Squawk()
        {
            Value = Guid.NewGuid().ToString();
        }
        public string Value { get; set; }
    }
}
