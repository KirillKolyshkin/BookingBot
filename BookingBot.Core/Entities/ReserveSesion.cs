using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBot.Core.Entities
{
    public class ReserveSesion
    {
        public ReserveSesion(Guid id, Guid roomId, long userId, Guid timeSessionId)
        {
            Id = id;
            RoomId = roomId;
            UserId = userId;
            TimeSessionId = timeSessionId;
            //TimeSession = timeSession;
            Cancaled = false;
        }

        public bool Cancaled { get; set; }
        public Guid Id { get; set; }
        public Guid RoomId { get; private set; }
        public Guid TimeSessionId { get; private set; }
        public long UserId { get; private set; }
        //public override string ToString() => String.Format("From " + TimeSession.StartTime + " To " + TimeSession.EndTime);
    }
}
