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
    public class Activity1 : Activity, ISensorEventListener
    {
        //int count = 1;
        int kroki = 0;
        private SensorManager _sensorManager; 
        private TextView _sensorTextView; 
        private static readonly object _syncLock = new object();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            //Button button = FindViewById<Button>(Resource.Id.MyButton);

            //button.Click += delegate { button.Text = string.Format("{0} clicks!", count++); };

            _sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            _sensorTextView = FindViewById<TextView>(Resource.Id.accelerometer_text);

            int h = 480;
            mYOffset = h * 0.5f;
            mScale[0] = - (h * 0.5f * (1.0f / (SensorManager.StandardGravity * 2)));
            mScale[1] = - (h * 0.5f * (1.0f / (SensorManager.MagneticFieldEarthMax)));
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            // We don't want to do anything here.
        }

        private float mLimit = 10;
        private float[] mLastValues = new float[3*2];
        private float[] mScale = new float[2];
        private float mYOffset;

        private float[] mLastDirections = new float[3*2];
        private float[][] mLastExtremes = { new float[3*2], new float[3*2] };
        private float[] mLastDiff = new float[3*2];
        private int mLastMatch = -1;



        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
            {
                StringBuilder text = new StringBuilder();
                
                if (e.Sensor.Type == SensorType.Orientation)
                {
                    //nothing
                }
                else
                {
                    if (e.Sensor.Type == SensorType.Accelerometer)
                    {
                        float vSum = 0;
                        for (int i = 0; i < 3; i++)
                        {
                            float v = mYOffset + e.Values[i] * mScale[1];
                            vSum += v;
                        }
                        int k = 0;
                        float V = vSum / 3;

                        float direction = (V > mLastValues[k] ? 1 : (V < mLastValues[k] ? -1 : 0));
                        if (direction == -mLastDirections[k])
                        {
                            // Direction changed
                            int extType = (direction > 0 ? 0 : 1); // minumum or maximum?
                            mLastExtremes[extType][k] = mLastValues[k];
                            float diff = Math.Abs(mLastExtremes[extType][k] - mLastExtremes[1 - extType][k]);

                            if (diff > mLimit)
                            {
                                bool isAlmostAsLargeAsPrevious = diff > (mLastDiff[k] * 2 / 3);
                                bool isPreviousLargeEnough = mLastDiff[k] > (diff / 3);
                                bool isNotContra = (mLastMatch != 1 - extType);

                                if (isAlmostAsLargeAsPrevious && isPreviousLargeEnough && isNotContra)
                                {
                                    kroki++;
                                    mLastMatch = extType;
                                }
                                else
                                {
                                    mLastMatch = -1;
                                }
                            }
                            mLastDiff[k] = diff;
                        }
                        mLastDirections[k] = direction;
                        mLastValues[k] = V;
                    }
                }
                text.Append("\n Kroki: " + kroki);
                _sensorTextView.Text = text.ToString();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            _sensorManager.RegisterListener(this, _sensorManager.GetDefaultSensor(SensorType.All), SensorDelay.Normal);
        }

        protected override void OnPause()
        {
            base.OnPause();
            _sensorManager.UnregisterListener(this);
        }

    }
}

