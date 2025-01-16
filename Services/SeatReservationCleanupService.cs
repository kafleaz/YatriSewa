using YatriSewa.Models;

namespace YatriSewa.Services
{
    public class SeatReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SeatReservationCleanupService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                    // Find expired reservations
                    var expiredSeats = context.Seat_Table
                        .Where(s => s.Status == SeatStatus.Reserved &&
                                    s.ReservedAt != null &&
                                    s.ReservedAt <= DateTime.UtcNow.AddMinutes(-20))
                        .ToList();

                    foreach (var seat in expiredSeats)
                    {
                        seat.Status = SeatStatus.Available;
                        seat.ReservedAt = null;
                        seat.ReservedByUserId = null; // Clear user reservation info
                    }

                    await context.SaveChangesAsync();
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Run every minute
            }
        }
    }

}
