using Booking.Infrastucture;
using BookingBot.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingBot.Infrastructure
{
    public class Repository
    {
        public Repository(ReserveDataContext context)
        {
            this.context = context;
        }

        public Repository()
        {
            context = new ReserveDataContext();
        }

        private ReserveDataContext context;
        int reservesessionCount = 0;
        private Guid GetId() => Guid.NewGuid();
        private bool ContaimRoom(Classroom room) => context.Classrooms.Contains(room);

        private bool EnableToReserve(TimeSession timeSession, Classroom classroom)
        {
            return !context
                .Sessions
                .Where(s => s.RoomId == classroom.Id && !s.Cancaled)
                .Where(s =>
                s.TimeSession.StartTime < timeSession.StartTime &&
                s.TimeSession.EndTime > timeSession.StartTime ||
                s.TimeSession.StartTime < timeSession.EndTime &&
                s.TimeSession.EndTime > timeSession.EndTime)
                .Any();
        }

        public void Reserve(long userId, Classroom room, TimeSession timeSession)
        {
            if (EnableToReserve(timeSession, room))
            {
                if (!ContaimRoom(room))
                    context.Classrooms.Add(room);
                context.Sessions.Add(new ReserveSesion(GetId(), room.Id, userId, timeSession));
            }
            else
            {

            }
        }

        public void Cancel(ReserveSesion reserveSesion) => reserveSesion.Cancaled = true;

        public ReserveSesion FindNearest(User user, Classroom classroom, TimeSpan neededTime)
        {
            DateTime endOfPrevious = DateTime.Now;
            DateTime startOfNext;
            foreach (var session in context.Sessions.Where(s => s.TimeSession.StartTime < DateTime.Now && !s.Cancaled))
            {
                startOfNext = session.TimeSession.StartTime;
                TimeSpan interval = startOfNext - endOfPrevious;
                if (neededTime <= interval)
                    return new ReserveSesion(GetId(), classroom.Id, user.Id, new TimeSession(endOfPrevious, neededTime));
                endOfPrevious = session.TimeSession.EndTime;
            }
            return new ReserveSesion(GetId(), classroom.Id, user.Id, new TimeSession(endOfPrevious, neededTime));
        }

        private Classroom GetRumByNum(int roomNum) => context.Classrooms.Where(c => c.RoomNumber == roomNum).ToList().First();
        public User GetUserById(long userId) => context.Users.Where(u => u.Id == userId).ToList().First();
        public bool ContainUser(long userId) => context.Users.Where(u => u.Id == userId).Any();
        public void AddUser(long userId, string userName) => context.Users.Add(new User(userId, userName));
        public void AddReservation(long userId, DateTime startTime, int roomNum)
        {
            var timeSession = new TimeSession(startTime, new TimeSpan(0, 45, 0));
            var room = GetRumByNum(roomNum);
            context.Sessions.Add(new ReserveSesion(GetId(), room.Id, userId, timeSession));
        }
        
        public List<ReserveSesion> ResereveSessionsOfUser(long userId)
        {
            var user = GetUserById(userId);
            return context
                .Sessions
                .Where(s => s.UserId == user.Id)
                .ToList();
        }

        public List<ReserveSesion> ReserveSessionInDay(DateTime date, int roomNum)
        {
            var room = GetRumByNum(roomNum);

            return context
                .Sessions
                .Where(s =>
                s.TimeSession.StartTime.Date == date.Date
                && s.TimeSession.StartTime >= DateTime.Now
                && s.RoomId == room.Id)
                .ToList();
        }
    }
}
