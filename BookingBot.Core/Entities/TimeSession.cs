using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Core.Entities
{
    public class TimeSession
    {
        public TimeSession(DateTime startTime, DateTime endTime)
        {
            if (startTime < endTime)
            {
                StartTime = startTime;
                EndTime = endTime;
                Interval = EndTime - StartTime;
            }
            else
                throw new Exception("Input dates was incorect");

        }

        public TimeSession(DateTime startTime, TimeSpan interval, Guid id)
        {
            Id = id;
            StartTime = startTime;
            Interval = interval;
            EndTime = StartTime + Interval;
        }

        public Guid Id { get; set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public TimeSpan Interval { get; private set; }
    }
}
