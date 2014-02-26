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
    [Activity(Label = "Krokomierz", Icon = "@drawable/icon")]
    public class MainActivity : TabActivity, IDialogInterfaceOnClickListener, IDialogInterfaceOnCancelListener
    {
        public static float stepLength;
        public static float weight;
        public static int mLimit;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            retrieveset();

            SensorManager sensorManager = (SensorManager)GetSystemService(SensorService);
            Sensor accelerometerSensor = sensorManager.GetDefaultSensor(SensorType.Accelerometer);
            
            if (accelerometerSensor != null)
            {
                CreateTab(typeof(PedometerActivity), "pedometer", "Krokomierz", Resource.Drawable.ic_tab_pedometer);
                CreateTab(typeof(OptionsActivity), "options", "Opcje", Resource.Drawable.ic_tab_options);
            }
            else
            {
                string mess = "Twoje urz¹dzenie nie jest wspierane - nie posiada akcelerometru";
                AlertDialog ad = new AlertDialog.Builder(this).SetMessage(mess).SetPositiveButton("Zamknij", this).SetOnCancelListener(this).Show();
            }
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

        protected override void OnDestroy()
        {
            base.OnDestroy();
            saveset();
        }

        protected void saveset()
        {
            var prefs = Application.Context.GetSharedPreferences("Krokomierz", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutFloat("stepLength", stepLength);
            prefEditor.PutFloat("weight", weight);
            prefEditor.PutInt("mLimit", mLimit);
            prefEditor.Apply();
            prefEditor.Commit();
        }

        protected void retrieveset()
        {
            var prefs = Application.Context.GetSharedPreferences("Krokomierz", FileCreationMode.Private);
            stepLength = prefs.GetFloat("stepLength", 50.0f);
            weight = prefs.GetFloat("weight", 80.0f);
            mLimit = prefs.GetInt("mLimit", 13);
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            this.Finish();
        }

        public void OnCancel(IDialogInterface dialog)
        {
            this.Finish();
        }
    }
}
