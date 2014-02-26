using Android.App;
using Android.OS;
using System.Threading;

namespace Krokomierz
{
    [Activity(Theme = "@style/Theme.Splash", Label = "Krokomierz", MainLauncher = true, NoHistory = true, 
        Icon = "@drawable/icon", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class SplashScreen : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Thread.Sleep(1000);
            StartActivity(typeof(MainActivity));
        }
    }
}