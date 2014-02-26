using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading;

namespace Krokomierz
{
    [Activity(Theme = "@style/Theme.Splash", Label = "Krokomierz", MainLauncher = true, NoHistory = true, Icon = "@drawable/icon")]
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