
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Content.PM;
using Android;

namespace DivineVerITies.Helpers
{
    public class Utility
    {
        public const int MY_PERMISSIONS_REQUEST_WRITE_EXTERNAL_STORAGE = 122;
        public const int MY_PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE = 123;
        public const int MY_PERMISSIONS_REQUEST_READ_CONTACTS = 124;
        public const int MY_PERMISSIONS_REQUEST_USE_FINGERPRINT = 125;

        public static bool CheckPermission(Context context, string permission)
        {
            var currentAPIVersion = Build.VERSION.SdkInt;
            if (currentAPIVersion >= BuildVersionCodes.M)
            {
                if (ContextCompat.CheckSelfPermission(context, permission) != Permission.Granted)
                {
                    if (ActivityCompat.ShouldShowRequestPermissionRationale((Activity)context, permission))
                    {
                        Android.Support.V7.App.AlertDialog.Builder alertBuilder = new Android.Support.V7.App.AlertDialog.Builder(context);
                        alertBuilder.SetCancelable(true);
                        alertBuilder.SetTitle("Permission Required!");
                        if (permission == Manifest.Permission.ReadExternalStorage
                            || permission == Manifest.Permission.WriteExternalStorage)
                        {
                            alertBuilder.SetMessage("External storage access is required");
                        }
                        else if (permission == Manifest.Permission.ReadContacts)
                        {
                            alertBuilder.SetMessage("Contacts access is required");
                        }
                        else if (permission == Manifest.Permission.UseFingerprint)
                        {
                            alertBuilder.SetMessage("Fingerprint access is required");
                        }

                        alertBuilder.SetPositiveButton("Allow", (sender, args) =>
                        {
                            if (permission == Manifest.Permission.ReadExternalStorage)
                            {
                                ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.ReadExternalStorage }, MY_PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE);
                            }
                            else if (permission == Manifest.Permission.ReadContacts)
                            {
                                ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.ReadContacts }, MY_PERMISSIONS_REQUEST_READ_CONTACTS);
                            }
                            else if (permission == Manifest.Permission.UseFingerprint)
                            {
                                ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.UseFingerprint }, MY_PERMISSIONS_REQUEST_USE_FINGERPRINT);
                            }
                            else if (permission == Manifest.Permission.WriteExternalStorage)
                            {
                                ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.WriteExternalStorage }, MY_PERMISSIONS_REQUEST_WRITE_EXTERNAL_STORAGE);
                            }

                        });
                        alertBuilder.Create()
                        .Show();

                    }
                    else
                    {
                        if (permission == Manifest.Permission.ReadExternalStorage)
                        {
                            ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.ReadExternalStorage }, MY_PERMISSIONS_REQUEST_READ_EXTERNAL_STORAGE);
                        }
                        else if (permission == Manifest.Permission.ReadContacts)
                        {
                            ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.ReadContacts }, MY_PERMISSIONS_REQUEST_READ_CONTACTS);
                        }
                        else if (permission == Manifest.Permission.UseFingerprint)
                        {
                            ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.UseFingerprint }, MY_PERMISSIONS_REQUEST_USE_FINGERPRINT);
                        }
                        else if (permission == Manifest.Permission.WriteExternalStorage)
                        {
                            ActivityCompat.RequestPermissions((Activity)context, new string[] { Manifest.Permission.WriteExternalStorage }, MY_PERMISSIONS_REQUEST_WRITE_EXTERNAL_STORAGE);
                        }
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
    }
}