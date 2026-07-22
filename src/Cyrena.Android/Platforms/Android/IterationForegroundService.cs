using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidPM = Android.Content.PM;
using AndroidR = Android.Resource;

namespace Cyrena.Platforms
{
    public static class MauiAppContainer
    {
        public static IServiceProvider? Provider { get; set; }
    }

    [Service(ForegroundServiceType = AndroidPM.ForegroundService.TypeDataSync)]
    public class IterationForegroundService : Service
    {
        private const int NotificationId = 1001;
        private const string ChannelId = "iteration_service_channel";

        public override IBinder? OnBind(Intent? intent) => null;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            CreateNotificationChannel();

            var builder = new NotificationCompat.Builder(this, ChannelId);
            builder.SetContentTitle("Cyréna");
            builder.SetContentText("Conversations loaded...");
            builder.SetSmallIcon(Microsoft.Maui.Resource.Drawable.notification);
            builder.SetOngoing(true);
            var launchIntent = PackageManager?.GetLaunchIntentForPackage(PackageName!)!;
            launchIntent.SetFlags(ActivityFlags.SingleTop);

            var pendingIntent = PendingIntent.GetActivity(
                this,
                0,
                launchIntent,
                PendingIntentFlags.Immutable
            );
            builder.SetContentIntent(pendingIntent);
            builder.SetAutoCancel(false);
            var notification = builder.Build();

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
                StartForeground(NotificationId, notification, AndroidPM.ForegroundService.TypeDataSync);
            else
                StartForeground(NotificationId, notification);

            return StartCommandResult.Sticky;
        }

        private void CreateNotificationChannel()
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(26)) return;

            var channel = new NotificationChannel(ChannelId, "Cyréna Services", NotificationImportance.Default);
            channel.Description = "Ensures loaded conversations do not miss iterations.";

            var manager = GetSystemService(NotificationService) as NotificationManager;
            manager?.CreateNotificationChannel(channel);
        }

        public override void OnDestroy()
        {
            StopForeground(StopForegroundFlags.Remove);
            base.OnDestroy();
        }
    }
}
