using StripePaymentMethod = Stripe.PaymentMethod;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System;
using System.Threading.Tasks;
using YatriSewa.Models;

namespace YatriSewa.Controllers
{
    public class PaymentRefund : Controller
    {
        private readonly ApplicationContext _context;

        public PaymentRefund(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessStripeRefund([FromBody] RefundRequestModel request)
        {
            Console.WriteLine($"BookingId: {request?.BookingId}, TicketId: {request?.TicketId}");

            if (request == null || request.BookingId <= 0 || request.TicketId <= 0)
            {
                return Json(new { success = false, message = "Invalid Booking ID or Ticket ID received." });
            }

            var payment = await _context.Payment_Table
                .Include(p => p.StripeTrans)
                .FirstOrDefaultAsync(p => p.BookingId == request.BookingId);

            if (payment == null || payment.StripeTrans == null)
            {
                return Json(new { success = false, message = "Payment record not found." });
            }

            if (string.IsNullOrEmpty(payment.StripeTrans.StripeTransactionId))
            {
                return Json(new { success = false, message = "No valid charge ID found for refund." });
            }

            var ticket = await _context.Ticket_Table
            .Include(t => t.Seat)
            .Include(t => t.Booking)
                .ThenInclude(b => b.Bus)
                    .ThenInclude(bus => bus.Schedules)  // Include Schedules explicitly
            .FirstOrDefaultAsync(t => t.TicketId == request.TicketId && t.BookingId == request.BookingId);

            if (ticket == null || ticket.Seat.Status == SeatStatus.Available)
            {
                return Json(new { success = false, message = "Ticket not found or already cancelled." });
            }

            // Calculate time difference from departure
            DateTime departureTime = ticket.Booking.Bus.Schedules.FirstOrDefault()?.DepartureTime ?? DateTime.MinValue;
            TimeSpan timeDifference = departureTime - DateTime.UtcNow;
            Console.WriteLine($"DepartureTime: {departureTime}, CurrentTime: {DateTime.UtcNow}");

            decimal refundPercentage;
            if (timeDifference.TotalHours > 48)
            {
                refundPercentage = 0.9m;  // 90% refund
            }
            else if (timeDifference.TotalHours > 24)
            {
                refundPercentage = 0.75m; // 75% refund
            }
            else if (timeDifference.TotalHours > 12)
            {
                refundPercentage = 0.5m;  // 50% refund
            }
            else
            {
                return Json(new { success = false, message = "Cancellation not allowed less than 12 hours before departure." });
            }

            try
            {
                StripeConfiguration.ApiKey = "sk_test_51PNIqUB3UNeeoGIf1BwZwOzVjkkGUopjyJgVaTeP57NJhJNNqZNjFLfzQgK1W5kMinCZnaTN8iugH3qLXlxEgcgm00VBFEoCPp"; // Replace with your actual Stripe secret key

                decimal refundAmount = ticket.Price * refundPercentage;
                var refundOptions = new RefundCreateOptions
                {
                    Charge = payment.StripeTrans.StripeTransactionId,
                    Amount = (long)(refundAmount * 100) // Convert to cents
                };

                var refundService = new RefundService();
                Refund refund = await refundService.CreateAsync(refundOptions);

                if (refund.Status == "succeeded")
                {
                    // ✅ Update seat status to Available
                    ticket.Seat.Status = SeatStatus.Available;
                    _context.Seat_Table.Update(ticket.Seat);

                    // ✅ Delete the specific ticket
                    _context.Ticket_Table.Remove(ticket);

                    // ✅ Adjust payment amount
                    payment.AmountPaid -= refundAmount;

                    // ✅ Check if all tickets under the same booking are cancelled
                    var remainingTickets = await _context.Ticket_Table
                        .Where(t => t.BookingId == request.BookingId)
                        .ToListAsync();

                    if (!remainingTickets.Any())
                    {
                        // If no tickets left, update the booking status to Cancelled
                        var booking = ticket.Booking;
                        booking.Status = BookingStatus.Cancelled;
                        _context.Booking_Table.Update(booking);

                        // Update payment status to fully refunded if all tickets are deleted
                        payment.Status = PaymentStatus.Refunded;
                    }

                    payment.RefundDate = DateTime.UtcNow;
                    payment.RefundTransactionId = refund.Id;
                    _context.Payment_Table.Update(payment);

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = $"Ticket cancelled successfully. Refund amount: Rs.{refundAmount}" });
                }
                else
                {
                    return Json(new { success = false, message = "Stripe refund failed." });
                }
            }
            catch (StripeException ex)
            {
                return Json(new { success = false, message = "Stripe refund error: " + ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Unexpected error: " + ex.Message });
            }
        }

    }

}

