using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Hardware;
using System.Text;
using System.Threading;

namespace Krokomierz
{
    [Activity(Label = "Krokomierz", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : TabActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            CreateTab(typeof(PedometerActivity), "pedometer", "Krokomierz", Resource.Drawable.ic_tab_pedometer);
            CreateTab(typeof(OptionsActivity), "options", "Opcje", Resource.Drawable.ic_tab_options);
        }

        private void CreateTab(Type activityType, string tag, string label, int drawableId)
        {
            var intent = new Intent(this, activityType);
            intent.AddFlags(ActivityFlags.NewTask);

            var spec = TabHost.NewTabSpec(tag);
            var drawableIcon = Resources.GetDrawable(drawableId);
            spec.SetIndicator(label, drawableIcon);
            spec.SetContent(intent);

            TabHost.AddTab(spec);
        }
    }
}
