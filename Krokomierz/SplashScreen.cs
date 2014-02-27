using Android.App;
using Android.OS;
using System.Threading;

namespace Krokomierz
{
    [Activity(Theme = "@style/Theme.Main", Label = "Krokomierz", NoHistory = true, MainLauncher = true, Icon = "@drawable/icon")]
    public class SplashScreen : Activity
    {
        static Thread thread;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            
            SetContentView(Resource.Layout.Splash);
            if (thread==null)
            {
                thread = new Thread(new ThreadStart(startMethod));
                thread.Start();
            }
        }

        private void startMethod()
        {
            Thread.Sleep(2000);
            StartActivity(typeof(MainActivity));
            this.Finish();
        }

    }
}