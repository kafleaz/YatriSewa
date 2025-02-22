using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace YatriSewa.Models
{
    public class ApplicationContext : DbContext
    {
        private readonly IConfiguration _configuration;

        // Constructor injection of IConfiguration
        public ApplicationContext(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // OnConfiguring method to configure the DbContext with the connection string
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Use the connection string from the configuration
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("YatriSewa"));
            }
        }

        // DbSet properties for the tables in your database
        public DbSet<User> User_Table { get; set; }
        public DbSet<OTP> Otp_Table { get; set; }
        public DbSet<Bus> Bus_Table { get; set; }
        public DbSet<BusCompany> Company_Table { get; set; }
        public DbSet<Service> Service_Table { get; set; }
        public DbSet<BusDriver> Driver_Table { get; set; }
        public DbSet<Route> Route_Table { get; set; }
        public DbSet<DriverAssignment> DriverAssign_Table { get; set; }
        public DbSet<Schedule> Schedule_Table { get; set; }
        public DbSet<Seat> Seat_Table { get; set; }
        public DbSet<Booking> Booking_Table { get; set; }
        public DbSet<Ticket> Ticket_Table { get; set; }
        public DbSet<Payment> Payment_Table { get; set; }
        public DbSet<Passenger> Passenger_Table { get; set; }
        public DbSet<Merchant> Merchant_Table { get; set; }
        public DbSet<EsewaTransaction> EsewaTransaction_Table { get; set; }
        public DbSet<StripeTrans> StripeTrans_Table { get; set; }
        public DbSet<IoTDevice> IoTDevices { get; set; }
        public DbSet<IoTDeviceLocationLog> IoTDeviceLocationLogs { get; set; }
        public DbSet<PassengerLocationLog> PassengerLocationLogs { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Define the one-to-one relationship between User and BusDriver
            modelBuilder.Entity<User>()
                .HasOne(u => u.BusDriver)
                .WithOne(bd => bd.User)
                .HasForeignKey<BusDriver>(bd => bd.UserId);

            // Bus -> Route relationship with "Restrict" to prevent cascading deletes
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Route)
                .WithMany(r => r.Buses)
                .HasForeignKey(b => b.RouteId)
                .OnDelete(DeleteBehavior.Restrict);  // No cascading delete for routes

            // Bus -> BusCompany relationship with "Cascade" to allow company deletion to cascade
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.BusCompany)
                .WithMany(c => c.Buses)
                .HasForeignKey(b => b.CompanyId)
                .OnDelete(DeleteBehavior.Cascade);  // Allow cascading delete if needed, or change to Restrict if not

            // Bus -> BusDriver relationship with "Restrict" to prevent cascading deletes
            modelBuilder.Entity<Bus>()
                .HasOne(b => b.BusDriver)
                .WithMany(d => d.Buses)
                .HasForeignKey(b => b.DriverId)
                .OnDelete(DeleteBehavior.Restrict);  // No cascading delete for drivers

            modelBuilder.Entity<Bus>()
                .HasOne(b => b.Service)
                .WithOne(s => s.Bus)
                .HasForeignKey<Service>(s => s.BusId)
                .OnDelete(DeleteBehavior.Restrict);  // Prevent cascading deletes


            base.OnModelCreating(modelBuilder);
        }


    }
}
