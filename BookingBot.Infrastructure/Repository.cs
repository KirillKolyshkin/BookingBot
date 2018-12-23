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
        private Guid GetId() => Guid.NewGuid();

        //public void SaveChanges() => context.SaveChanges();

        public bool EnableToReserve(TimeSession timeSession, int roomNumb)
        {
            var classroom = GetRumByNum(roomNumb);
            var context = new ReserveDataContext();
            return !context
                .Sessions
                .Where(s => s.RoomId == classroom.Id && !s.Cancaled)
                .Where(s =>
                    GetTimeSessionById(s.TimeSessionId).StartTime < timeSession.StartTime &&
                    GetTimeSessionById(s.TimeSessionId).EndTime > timeSession.StartTime ||
                    GetTimeSessionById(s.TimeSessionId).StartTime < timeSession.EndTime &&
                    GetTimeSessionById(s.TimeSessionId).EndTime > timeSession.EndTime
                )
                .Any();
        }

        public void Cancel(ReserveSesion reserveSesion) => reserveSesion.Cancaled = true;

        public ReserveSesion FindNearest(long userId, int roomNum, TimeSpan neededTime)
        {
            var context = new ReserveDataContext();
            var user = GetUserById(userId);
            var classroom = GetRumByNum(roomNum);
            DateTime endOfPrevious = DateTime.Now;
            DateTime startOfNext;
            var tSesId = GetId();
            foreach (var session in context.Sessions.Where(s => 
            GetTimeSessionById(s.TimeSessionId).EndTime > DateTime.Now
            && !s.Cancaled).OrderBy(s => GetTimeSessionById(s.TimeSessionId).StartTime))
            {
                var timeSession = GetTimeSessionById(session.TimeSessionId);
                Console.Write(timeSession.StartTime + "   " + timeSession.EndTime);
                if (endOfPrevious > timeSession.StartTime)
                {
                    endOfPrevious = timeSession.EndTime;
                    Console.Write("kek");
                    continue;
                }
                startOfNext = timeSession.StartTime;
                TimeSpan interval = startOfNext - endOfPrevious;
                if (neededTime <= interval)
                {
                    Console.WriteLine("ok");
                    Console.WriteLine("new Date: " + endOfPrevious);
                    var newContext = new ReserveDataContext();
                    newContext.TimeSessions.Add(new TimeSession(endOfPrevious, neededTime, tSesId));
                    newContext.SaveChanges();
                    Console.WriteLine("ok");
                    return new ReserveSesion(GetId(), classroom.Id, user.Id, tSesId);
                }
                endOfPrevious = timeSession.EndTime;
            }
            context.TimeSessions.Add(new TimeSession(endOfPrevious, neededTime, tSesId));
            context.SaveChanges();
            return new ReserveSesion(GetId(), classroom.Id, user.Id, tSesId);
        }

        //context = new ReserveDataContext();

        public Classroom GetRumByNum(int roomNum) => context.Classrooms.Single(c => c.RoomNumber == roomNum);
        public Classroom GetRumById(Guid id) => context.Classrooms.Single(c => c.Id == id);
        //public ReserveSesion GetReserveSesionById(Guid id) => context.Sessions.First(s => s.Id == id);
        public TimeSession GetTimeSessionById(Guid id)
        {
            var context = new ReserveDataContext();
            return context.TimeSessions.Single(t => t.Id == id);
        }
        public User GetUserById(long userId) => context.Users.First(u => u.Id == userId);
        public bool ContainUser(string userName) => context.Users.Where(u => u.FirstName.Equals(userName)).Any();
        public bool ContainUser(long userId) => context.Users.Where(u => u.Id == userId).Any();
        public void AddUser(long userId, string userName)
        {
            var context = new ReserveDataContext();
            context.Users.Add(new User(userId, userName));
            context.SaveChanges();
        }
        public bool ContainRoom(int roomNum)
        {
            var context = new ReserveDataContext();
            return context.Classrooms.Where(r => r.RoomNumber == roomNum).Any();
        }
        public void AddClassroom(int roomNum)
        {
            var context = new ReserveDataContext();
            context.Classrooms.Add(new Classroom(GetId(), roomNum));
            context.SaveChanges();
        }
        public void AddReservation(long userId, DateTime startTime, int roomNum)
        {
            var context = new ReserveDataContext();
            var tSesId = GetId();
            context.TimeSessions.Add(new TimeSession(startTime, new TimeSpan(0, 45, 0), tSesId));
            var room = GetRumByNum(roomNum);
            context.Sessions.Add(new ReserveSesion(GetId(), room.Id, userId, tSesId));
            context.SaveChanges();
        }

        public List<ReserveSesion> ResereveSessionsOfUser(long userId)
        {
            var context = new ReserveDataContext();
            var user = GetUserById(userId);
            return context
                .Sessions
                .Where(s => s.UserId == user.Id)
                .ToList();
        }

        public List<ReserveSesion> ReserveSessionInDay(DateTime date, int roomNum)
        {
            var context = new ReserveDataContext();
            var room = GetRumByNum(roomNum);
            Console.WriteLine("aaaa" + room.RoomNumber);
            return context
                .Sessions
                .Where(s => (GetTimeSessionById(s.TimeSessionId).StartTime.Date == date.Date
                && s.RoomId == room.Id))
                .Where(s => GetTimeSessionById(s.TimeSessionId).EndTime >= DateTime.Now)
                .ToList();
        }

        public string ReservationToStr( ReserveSesion rs)
        {
            var timeSession = GetTimeSessionById(rs.TimeSessionId);
            var room = GetRumById(rs.RoomId);
            var user = GetUserById(rs.UserId);
            var str = $"room: {room.RoomNumber}     from {timeSession.StartTime} to {timeSession.EndTime}";
            return str;
        }
    }
}
