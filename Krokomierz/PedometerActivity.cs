using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Hardware;

namespace Krokomierz
{
    [Activity(Label = "Pedometer Activity", NoHistory=true)]
    public class PedometerActivity : Activity, ISensorEventListener, Chronometer.IOnChronometerTickListener
    {
        private int steps = 0;
        private float calories = 0;
        private float speed = 0.0f;
        private float distance = 0.0f;
        private float MWF = 0.73f;
        private Chronometer chrono;
        private Chronometer tmp;
        private SensorManager sensorManager;
        private TextView stepsTextView;
        private TextView caloriesTextView;
        private TextView speedTextView;
        private TextView distanceTextView;
        private TextView timeTextView;
        private bool run;
        private static readonly object syncLock = new object();
        private PowerManager powerManager;
        private PowerManager.WakeLock wakeLock;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            powerManager = (PowerManager)GetSystemService(Context.PowerService);
            wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "MyWakeLock");
            wakeLock.Acquire();

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Pedometer);

            chrono = FindViewById<Chronometer>(Resource.Id.chronometer1);
            sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            stepsTextView = FindViewById<TextView>(Resource.Id.pedometer);
            caloriesTextView = FindViewById<TextView>(Resource.Id.calories);
            speedTextView = FindViewById<TextView>(Resource.Id.speed);
            distanceTextView = FindViewById<TextView>(Resource.Id.distance);
            timeTextView = FindViewById<TextView>(Resource.Id.time);

            if (savedInstanceState != null)
            {
                steps = savedInstanceState.GetInt("steps", 0);
                calories = savedInstanceState.GetFloat("calories", 0.0f);
                speed = savedInstanceState.GetFloat("speed", 0.0f);
                distance = savedInstanceState.GetFloat("distance", 0.0f);
                chrono.Base = savedInstanceState.GetLong("time", SystemClock.ElapsedRealtime());
                run = savedInstanceState.GetBoolean("run", false);
            }

            chrono.Format = "00:0%s";
            chrono.OnChronometerTickListener = this;

            int h = 480;
            mYOffset = h * 0.5f;
            mScale[0] = -(h * 0.5f * (1.0f / (SensorManager.StandardGravity * 2)));
            mScale[1] = -(h * 0.5f * (1.0f / (SensorManager.MagneticFieldEarthMax)));

            if (run)
            {
                refreshTextViews();
                sensorsListenerRegister();
                chrono.Start();
            }
        }

        private void sensorsListenerRegister()
        {
            sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.All), SensorDelay.Normal);
        }

        private void refreshTextViews()
        {
            TimeSpan tim = TimeSpan.FromMilliseconds(SystemClock.ElapsedRealtime() - chrono.Base);
            timeTextView.Text = chrono.Text;
            stepsTextView.Text = steps.ToString();
            caloriesTextView.Text = ((int)calories).ToString();
            speedTextView.Text = (distance / 100000 / tim.TotalHours).ToString("0.00");
            distanceTextView.Text = (distance / 100000).ToString();
        }

        public override bool OnMenuOpened(int featureId, IMenu menu)
        {
            menu.Clear();
            MenuInflater.Inflate(Resource.Menu.ActionMenu, menu);

            if (run)
                menu.FindItem(Resource.Id.startstop).SetTitle("Stop");
            else
                menu.FindItem(Resource.Id.startstop).SetTitle("Start");
            return base.OnMenuOpened(featureId, menu);
        }
    

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.reset)
            {
                steps = 0;
                calories = 0;
                speed = 0.0f;
                distance = 0.0f;
                tmp = null;
                chrono.Base = SystemClock.ElapsedRealtime();
                refreshTextViews();
            }
            else
            {
                if (!run)
                {
                    sensorsListenerRegister();
                    if (tmp == null)
                        chrono.Base = SystemClock.ElapsedRealtime();
                    else
                        chrono.Base += SystemClock.ElapsedRealtime() - tmp.Base;
                    chrono.Start();
                    run = true;
                }
                else
                {
                    sensorManager.UnregisterListener(this);
                    run = false;
                    chrono.Stop();
                    tmp = new Chronometer(this);
                }
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            outState.PutInt("steps", steps);
            outState.PutFloat("calories", calories);
            outState.PutFloat("speed", speed);
            outState.PutFloat("distance", distance);
            outState.PutLong("time", chrono.Base);
            outState.PutBoolean("run", run);
            base.OnSaveInstanceState(outState);
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            //nothing to do
        }

        private float[] mLastValues = new float[3 * 2];
        private float[] mScale = new float[2];
        private float mYOffset;

        private float[] mLastDirections = new float[3 * 2];
        private float[][] mLastExtremes = { new float[3 * 2], new float[3 * 2] };
        private float[] mLastDiff = new float[3 * 2];
        private int mLastMatch = -1;

        public void OnSensorChanged(SensorEvent e)
        {
            lock (syncLock)
            {
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

                            if (diff > MainActivity.mLimit)
                            {
                                bool isAlmostAsLargeAsPrevious = diff > (mLastDiff[k] * 2 / 3);
                                bool isPreviousLargeEnough = mLastDiff[k] > (diff / 3);
                                bool isNotContra = (mLastMatch != 1 - extType);

                                if (isAlmostAsLargeAsPrevious && isPreviousLargeEnough && isNotContra)
                                {
                                    steps++;
                                    distance += MainActivity.stepLength;
                                    calories += MainActivity.weight * MWF * MainActivity.stepLength / 100000;
                                    mLastMatch = extType;
                                }
                                else
                                    mLastMatch = -1;
                            }
                            mLastDiff[k] = diff;
                        }
                        mLastDirections[k] = direction;
                        mLastValues[k] = V;
                    }
                }
                stepsTextView.Text = steps.ToString();
                distanceTextView.Text = (distance / 100000).ToString();
                caloriesTextView.Text = ((int)calories).ToString();
            }
        }

        public void OnChronometerTick(Chronometer chronometer)
        {
            TimeSpan tim = TimeSpan.FromMilliseconds(SystemClock.ElapsedRealtime() - chrono.Base);
            timeTextView.Text = chrono.Text;
            if (tim.Ticks != 0)
                speedTextView.Text = (distance / 100000 / tim.TotalHours).ToString("0.00");
        }
    }
}