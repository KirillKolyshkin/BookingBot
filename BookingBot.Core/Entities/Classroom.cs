using BookingBot.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Core.Entities
{
    public class Classroom
    {
        public Classroom(Guid id, int roomNumber)
        {
            Id = id;
            RoomNumber = roomNumber;
            GetBaseWorkingHours();
        }

        public Classroom(Guid id, int roomNumber, TimeSpan startTime, TimeSpan endTime)
        {
            if (endTime > startTime)
            {
                Id = id;
                RoomNumber = roomNumber;
                StartWork = startTime;
                EndWork = endTime;
            }
            else
            {

            }
        }

        private TimeSpan StartWork { get; set; }
        private TimeSpan EndWork { get; set; }

        public Guid Id { get; set;}
        public int RoomNumber { get; private set; }
        public RoomStatus Status { get; set; }

        private void GetBaseWorkingHours()
        {
            StartWork = new TimeSpan(8, 0, 0);
            EndWork = new TimeSpan(18, 0, 0);
        }

        public string GetWorkingHours() => String.Format("StartWork: " + StartWork.ToString() + "/n EndWork: " + EndWork.ToString());
        public void Reserve() => Status = RoomStatus.Reserved;
        public void UnReserve() => Status = RoomStatus.Open;
        public RoomStatus GetStatus(DateTime dateTime) => Status;
        public override string ToString() => String.Format("Room : " + RoomNumber + " Status: " + Status);
    }
}
