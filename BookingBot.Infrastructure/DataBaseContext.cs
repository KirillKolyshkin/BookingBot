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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(u => u.FirstName);
            
            modelBuilder.Entity<TimeSession>()
                .HasIndex(ts => new { ts.StartTime, ts.EndTime });

            modelBuilder.Entity<ReserveSesion>()
                .HasIndex(rs => rs.TimeSessionId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ReserveSesion> Sessions { get; set; }
        public DbSet<TimeSession> TimeSessions { get; set; }
    }
}