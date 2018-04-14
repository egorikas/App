﻿namespace DotNetRu.Droid
{
    using System.Threading.Tasks;

    using Android.App;
    using Android.Content;
    using Android.Runtime;
    using Android.Util;

    using DotNetRu.DataStore.Audit.Services;
    using DotNetRu.Droid.Helpers;

    // TODO change to another class, Firebase specific

    [Preserve]
    [BroadcastReceiver(Permission = "com.google.android.c2dm.permission.SEND")]
    [IntentFilter(new[] { "com.google.android.c2dm.intent.RECEIVE" },
        Categories = new[] { "${applicationId}" })]
    public class DotNetRuPushReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Log.Debug("Push", "AuditUpdate. Push Notification receviced!");

            Task updateAudit = UpdateService.UpdateAudit().ContinueWith(
                t => this.SendNotification(
                    context,
                    "New information",
                    "New DotNetRu information is available"));
        }

        private void SendNotification(Context context, string title, string body)
        {
            // Set up an intent so that tapping the notifications returns to this app:
            Intent intent = new Intent(context, typeof(MainActivity));

            // Create a PendingIntent; we're only using one PendingIntent (ID = 0):
            const int PendingIntentID = 0;
            PendingIntent pendingIntent =
                PendingIntent.GetActivity(context, PendingIntentID, intent, PendingIntentFlags.OneShot);

            Notification notif = new Notification.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_launcher)
                .SetContentTitle(title)
                .SetContentText(body)
                .SetContentIntent(pendingIntent)
                .Build();

            NotificationManager notificationManager =
                (NotificationManager)context.GetSystemService(Context.NotificationService);
            notificationManager.Notify(Settings.GetUniqueNotificationID(), notif);
        }
    }
}