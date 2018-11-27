using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkshopTimer
{
    public class TimeSection
    {
        private int secondsInEvent;
        private string eventName;
        private int timeLeft;
        private int originalTime;

        public string EventName { get => eventName; set => eventName = value; }
        public int SecondsInEvent { get => secondsInEvent; set => secondsInEvent = value; }
        public int TimeLeft { get => timeLeft; set => timeLeft = value; }
        public int OriginalTime { get => originalTime; set => originalTime = value; }

        public TimeSection(string _eventName, int _seconds)
        {
            eventName = _eventName;
            secondsInEvent = _seconds;
            timeLeft = secondsInEvent;
            originalTime = secondsInEvent;
        }
    }
}
