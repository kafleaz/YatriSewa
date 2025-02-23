using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using YatriSewa.Models;

namespace YatriSewa.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ApplicationContext _context;

        public PaymentsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: Payments
        public async Task<IActionResult> Index()
        {
            var applicationContext = _context.Payment_Table.Include(p => p.Booking).Include(p => p.EsewaTransaction).Include(p => p.Passenger).Include(p => p.StripeTrans).Include(p => p.User);
            return View(await applicationContext.ToListAsync());
        }

        // GET: Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment_Table
                .Include(p => p.Booking)
                .Include(p => p.EsewaTransaction)
                .Include(p => p.Passenger)
                .Include(p => p.StripeTrans)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Payments/Create
        public IActionResult Create()
        {
            ViewData["BookingId"] = new SelectList(_context.Booking_Table, "BookingId", "BookingId");
            ViewData["TransactionId"] = new SelectList(_context.EsewaTransaction_Table, "TransactionId", "MerchantCode");
            ViewData["PassengerId"] = new SelectList(_context.Passenger_Table, "PassengerId", "Name");
            ViewData["StripeTransId"] = new SelectList(_context.StripeTrans_Table, "TransactionId", "ChargeId");
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PaymentId,BookingId,PaymentDate,AmountPaid,PaymentMethod,PassengerId,UserId,Status,TransactionId,StripeTransId,RefundTransactionId,RefundDate")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookingId"] = new SelectList(_context.Booking_Table, "BookingId", "BookingId", payment.BookingId);
            ViewData["TransactionId"] = new SelectList(_context.EsewaTransaction_Table, "TransactionId", "MerchantCode", payment.TransactionId);
            ViewData["PassengerId"] = new SelectList(_context.Passenger_Table, "PassengerId", "Name", payment.PassengerId);
            ViewData["StripeTransId"] = new SelectList(_context.StripeTrans_Table, "TransactionId", "ChargeId", payment.StripeTransId);
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", payment.UserId);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment_Table.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["BookingId"] = new SelectList(_context.Booking_Table, "BookingId", "BookingId", payment.BookingId);
            ViewData["TransactionId"] = new SelectList(_context.EsewaTransaction_Table, "TransactionId", "MerchantCode", payment.TransactionId);
            ViewData["PassengerId"] = new SelectList(_context.Passenger_Table, "PassengerId", "Name", payment.PassengerId);
            ViewData["StripeTransId"] = new SelectList(_context.StripeTrans_Table, "TransactionId", "ChargeId", payment.StripeTransId);
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", payment.UserId);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PaymentId,BookingId,PaymentDate,AmountPaid,PaymentMethod,PassengerId,UserId,Status,TransactionId,StripeTransId,RefundTransactionId,RefundDate")] Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(payment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.PaymentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BookingId"] = new SelectList(_context.Booking_Table, "BookingId", "BookingId", payment.BookingId);
            ViewData["TransactionId"] = new SelectList(_context.EsewaTransaction_Table, "TransactionId", "MerchantCode", payment.TransactionId);
            ViewData["PassengerId"] = new SelectList(_context.Passenger_Table, "PassengerId", "Name", payment.PassengerId);
            ViewData["StripeTransId"] = new SelectList(_context.StripeTrans_Table, "TransactionId", "ChargeId", payment.StripeTransId);
            ViewData["UserId"] = new SelectList(_context.User_Table, "UserId", "Name", payment.UserId);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payment_Table
                .Include(p => p.Booking)
                .Include(p => p.EsewaTransaction)
                .Include(p => p.Passenger)
                .Include(p => p.StripeTrans)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentId == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payment_Table.FindAsync(id);
            if (payment != null)
            {
                _context.Payment_Table.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payment_Table.Any(e => e.PaymentId == id);
        }
    }
}
