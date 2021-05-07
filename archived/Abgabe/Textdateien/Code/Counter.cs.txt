using System;
using System.Threading;

namespace GehirnJogging.Code
{
    /* COUNTER */
    public class Counter
    {
        private DateTime startime;
        private DateTime endtime;

        private Action onIncrement = () => { };
        public Action OnIncrement { get => onIncrement; set => onIncrement = value; }
        private Timer timer;

        private int interval = 10;

        public TimeSpan GetTimeSpan() => endtime.Subtract(startime);
        private void Increment(Object stateInfo)
        {
            endtime = DateTime.Now;
            onIncrement.Invoke();
        }

        public void Reset() => startime = endtime = DateTime.Now;
        public void Stop()
        {
            endtime = DateTime.Now;
            timer.Dispose();
            onIncrement.Invoke();
        }

        public void Start()
        {
            timer = new Timer(this.Increment, new AutoResetEvent(false), 0, interval);
            Reset();
        }

        public string GetFormatedCount()
        {
            TimeSpan ts = GetTimeSpan();
            return Format(ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds);
        }


        public static string GetFormatedCount(int count)
        {         
            int milli = count % 1000;
            int sec = (count / 1000) % 60;
            int min = (count / 1000) / 60 % 60;
            int hours = (count / 1000) / 3600 % 24;

            return Format(hours, min, sec, milli);
        }

        private static string Format(int hours, int min, int sec, int milli)
        {
            string formated = "";

            if (hours != 0) formated += hours + ":" + (min < 10 ? "0" : "");
            if (min != 0 || hours != 0) formated += min + ":" + (sec < 10 ? "0" : "");
            if (sec != 0 || min != 0 || hours != 0) formated += sec + ":" + (milli < 100 ? "0":"") + (milli < 10 ? "0" : "");
            formated += milli;
            return formated;
        }

    }

}
