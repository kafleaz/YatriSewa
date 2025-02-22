using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using YatriSewa.Migrations;
using YatriSewa.Models;
using YatriSewa.Services;
using YatriSewa.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 7066, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

// Add services to the container
builder.Services.AddControllersWithViews();

// Configure ApplicationContext with the connection string
builder.Services.AddDbContext<ApplicationContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("YatriSewa"));
});

builder.Services.AddTransient<IOperatorService, OperatorService>();
builder.Services.AddTransient<IDriverService, DriverService>();

// Register email service
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddTransient<ISMSService, SMSService>();
builder.Services.AddHostedService<SeatReservationCleanupService>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<RealTimeBusLocationService>();
builder.Services.AddScoped<FirebaseToDatabaseService>();
builder.Services.AddHttpClient<QrCodeService>();


// Configure and register the SMTP client
builder.Services.AddSingleton(new SmtpClient("smtp.gmail.com")
{
    Port = 587,
    Credentials = new NetworkCredential("otp.yatri.service@gmail.com", "@#$12345"), // Replace with your credentials
    EnableSsl = true,
});

builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Login/SignIn"; // Define the login path
        options.AccessDeniedPath = "/Home/AccessDenied"; // Path for access denied errors
    });

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout
    options.Cookie.HttpOnly = true; // Make session cookie HTTP-only
    options.Cookie.IsEssential = true; // Mark the session cookie as essential
});

// Add distributed memory cache for session storage
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.MapHub<BusLocationHub>("/busLocationHub");
app.MapHub<PassengerLocationHub>("/passengerLocationHub");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session handling
app.UseSession(); // This is necessary for session management

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();
