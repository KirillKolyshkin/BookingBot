using BookingBot.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Booking.Infrastucture
{
    public class ReserveDataContext : DbContext
    {
        public ReserveDataContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=ec2-54-163-245-64.compute-1.amazonaws.com;Port=5432;Database=d268i2i2c09fr0;Username=qyebdryzgjyotz;Password=c0d305d1464a5f96503ece91b7d17f7260815ac6b9857ad2a0610fc985cf4c81;SslMode=Require;Trust Server Certificate=True");
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ReserveSesion> Sessions { get; set; }
    }
}


//Host ec2-54-163-245-64.compute-1.amazonaws.com
//Database
//d268i2i2c09fr0
//User
//qyebdryzgjyotz
//Port
//5432
//Password
//c0d305d1464a5f96503ece91b7d17f7260815ac6b9857ad2a0610fc985cf4c81
//URI
//postgres://qyebdryzgjyotz:c0d305d1464a5f96503ece91b7d17f7260815ac6b9857ad2a0610fc985cf4c81@ec2-54-163-245-64.compute-1.amazonaws.com:5432/d268i2i2c09fr0
//Heroku CLI
//heroku pg:psql postgresql-infinite-30434 --app itisbotbooking