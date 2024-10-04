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
    }
}
