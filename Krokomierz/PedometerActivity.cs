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
using Android.Hardware;
using Android.Text;
using System.Threading;

namespace Krokomierz
{
    [Activity(Label = "Pedometer Activity")]
    public class PedometerActivity : Activity, ISensorEventListener
    {
        int steps = 0;
        float calories = 0;
        float speed = 0.0f;
        float distance = 0.0f;
        float stepLength = 50.0f;
        float weight = 80.0f;
        float MWF = 0.73f;
        DateTime? dt;
        bool threadFlag = true;

        private SensorManager sensorManager;
        private TextView stepsTextView;
        private TextView caloriesTextView;
        private TextView speedTextView;
        private TextView distanceTextView;
        private TextView timeTextView;
        private static readonly object _syncLock = new object();
        private Thread stopper;
        private bool run = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Pedometer);

            if (savedInstanceState != null)
            {
                steps = savedInstanceState.GetInt("steps", 0);
                calories = savedInstanceState.GetFloat("calories", 0.0f);
                speed = savedInstanceState.GetFloat("speed", 0.0f);
                distance = savedInstanceState.GetFloat("distance", 0.0f);
                if (savedInstanceState.GetLong("time", 0) == 0)
                    dt = null;
                else
                    dt = DateTime.FromBinary(savedInstanceState.GetLong("time", 0));
                run = savedInstanceState.GetBoolean("run", false);
            }

            sensorManager = (SensorManager)GetSystemService(Context.SensorService);
            stepsTextView = FindViewById<TextView>(Resource.Id.pedometer);
            caloriesTextView = FindViewById<TextView>(Resource.Id.calories);
            speedTextView = FindViewById<TextView>(Resource.Id.speed);
            distanceTextView = FindViewById<TextView>(Resource.Id.distance);
            timeTextView = FindViewById<TextView>(Resource.Id.time);

            refreshTextViews();

            int h = 480;
            mYOffset = h * 0.5f;
            mScale[0] = -(h * 0.5f * (1.0f / (SensorManager.StandardGravity * 2)));
            mScale[1] = -(h * 0.5f * (1.0f / (SensorManager.MagneticFieldEarthMax)));

            stopper = new Thread(timeSet);
            if (run)
                stopper.Start();

            //TODO
            //wstrzymac czas po Stop
        }

        private void refreshTextViews()
        {
            if (dt != null)
            {
                TimeSpan tim = DateTime.Now - (DateTime)dt;
                timeTextView.Text = tim.ToString(@"h\:mm\:ss");
                stepsTextView.Text = steps.ToString();
                caloriesTextView.Text = ((int)calories).ToString();
                speedTextView.Text = (distance / 100000 / tim.TotalHours).ToString("0.00");
                distanceTextView.Text = (distance / 100000).ToString();
            }
        }

        public override bool OnMenuOpened(int featureId, IMenu menu)
        {
            menu.Clear();
            MenuInflater inflater = MenuInflater; // -->onCreateMenu (Menu) 
            inflater.Inflate(Resource.Menu.ActionMenu, menu);  // /

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
                dt = null;
                steps = 0;
                calories = 0;
                speed = 0.0f;
                distance = 0.0f;
                stepsTextView.Text = steps.ToString();
                caloriesTextView.Text = calories.ToString();
                speedTextView.Text = speed.ToString("0.00");
                distanceTextView.Text = distance.ToString();
                timeTextView.Text = new TimeSpan(0).ToString(@"h\:mm\:ss");
            }
            else
            {
                if (stopper.ThreadState == ThreadState.Unstarted)
                {
                    if (dt == null)
                        dt = DateTime.Now;

                    threadFlag = true;
                    stopper.Start();
                    run = true;
                }
                else
                {
                    threadFlag = false;
                    run = false;
                    stopper = new Thread(timeSet);
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
            if (dt != null)
                outState.PutLong("time", ((DateTime)dt).ToBinary());
            outState.PutBoolean("run", run);
            base.OnSaveInstanceState(outState);
        }

        private void timeSet()
        {
            sensorManager.RegisterListener(this, sensorManager.GetDefaultSensor(SensorType.All), SensorDelay.Normal);
            while (threadFlag)
            {
                RunOnUiThread(delegate
                {
                    if (dt == null)
                        dt = DateTime.Now;
                    TimeSpan tim = DateTime.Now - (DateTime)dt;
                    timeTextView.Text = tim.ToString(@"h\:mm\:ss");
                    if (tim.Ticks != 0)
                        speedTextView.Text = (distance / 100000 / tim.TotalHours).ToString("0.00");
                });
                Thread.Sleep(1000);
            }
            sensorManager.UnregisterListener(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            threadFlag = false;
        }

        public void OnAccuracyChanged(Sensor sensor, SensorStatus accuracy)
        {
            //nothing to do
        }

        private float mLimit = 13; // 1.97  2.96  4.44  6.66  10.00  15.00  22.50  33.75  50.62
        private float[] mLastValues = new float[3 * 2];
        private float[] mScale = new float[2];
        private float mYOffset;

        private float[] mLastDirections = new float[3 * 2];
        private float[][] mLastExtremes = { new float[3 * 2], new float[3 * 2] };
        private float[] mLastDiff = new float[3 * 2];
        private int mLastMatch = -1;

        public void OnSensorChanged(SensorEvent e)
        {
            lock (_syncLock)
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

                            if (diff > mLimit)
                            {
                                bool isAlmostAsLargeAsPrevious = diff > (mLastDiff[k] * 2 / 3);
                                bool isPreviousLargeEnough = mLastDiff[k] > (diff / 3);
                                bool isNotContra = (mLastMatch != 1 - extType);

                                if (isAlmostAsLargeAsPrevious && isPreviousLargeEnough && isNotContra)
                                {
                                    steps++;
                                    distance += stepLength;
                                    calories += weight * MWF * stepLength / 100000;
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
                stepsTextView.Text = steps.ToString();
                distanceTextView.Text = (distance / 100000).ToString();
                caloriesTextView.Text = ((int)calories).ToString();

            }
        }
    }
}