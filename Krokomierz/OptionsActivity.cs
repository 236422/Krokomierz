using Android.App;
using Android.OS;
using Android.Widget;

namespace Krokomierz
{
    [Activity(Label = "Options Activity")]
    public class OptionsActivity : Activity, SeekBar.IOnSeekBarChangeListener
    {
        private EditText weightInput;
        private EditText stepSizeInput;
        private SeekBar sensivityInput;
        private TextView sensivityOutput;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Options);
            weightInput = FindViewById<EditText>(Resource.Id.weightInput);
            stepSizeInput = FindViewById<EditText>(Resource.Id.stepSizeInput);
            sensivityInput = FindViewById<SeekBar>(Resource.Id.sensivityInput);
            sensivityOutput = FindViewById<TextView>(Resource.Id.sensivityOutput);
            sensivityInput.SetOnSeekBarChangeListener(this);

            weightInput.Text = MainActivity.weight.ToString();
            stepSizeInput.Text = MainActivity.stepLength.ToString();
            sensivityInput.Progress = sensivityInput.Max - MainActivity.mLimit;
        }

        public void OnProgressChanged(SeekBar seekBar, int progress, bool fromUser)
        {
            sensivityOutput.Text = (seekBar.Progress * 100 / seekBar.Max) + "%";
        }

        public void OnStartTrackingTouch(SeekBar seekBar)
        {
            //nothing to do
        }

        public void OnStopTrackingTouch(SeekBar seekBar)
        {
            //nothing to do
        }

        protected override void OnPause()
        {
            base.OnPause();
            saveSettings();
        }

        private void saveSettings()
        {
            float.TryParse(weightInput.Text, out MainActivity.weight);
            float.TryParse(stepSizeInput.Text, out MainActivity.stepLength);
            MainActivity.mLimit = sensivityInput.Max - sensivityInput.Progress;
            RunOnUiThread(() =>Toast.MakeText(this, "Zapisano ustawienia", ToastLength.Short).Show());
        }

    }
}