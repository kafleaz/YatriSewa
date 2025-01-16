﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using YatriSewa.Models;

#nullable disable

namespace YatriSewa.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    [Migration("20241214150020_EsewaTransid")]
    partial class EsewaTransid
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("YatriSewa.Models.Booking", b =>
                {
                    b.Property<int>("BookingId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BookingId"));

                    b.Property<DateTime>("BookingDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("BusId")
                        .HasColumnType("int");

                    b.Property<int?>("PassengerId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasMaxLength(20)
                        .HasColumnType("int");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int?>("TotalSeats")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("BookingId");

                    b.HasIndex("BusId");

                    b.HasIndex("PassengerId");

                    b.HasIndex("UserId");

                    b.ToTable("Booking_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Bus", b =>
                {
                    b.Property<int>("BusId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BusId"));

                    b.Property<string>("BusName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("BusNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(500)
                        .HasColumnType("nvarchar(500)");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<string>("ImagePath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int?>("RouteId")
                        .HasColumnType("int");

                    b.Property<int>("SeatCapacity")
                        .HasColumnType("int");

                    b.HasKey("BusId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("DriverId");

                    b.HasIndex("RouteId");

                    b.ToTable("Bus_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.BusCompany", b =>
                {
                    b.Property<int>("CompanyId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CompanyId"));

                    b.Property<string>("CompanyAddress")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("ContactInfo")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Reg_No")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("VAT_PAN")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("VAT_PAN_PhotoPath")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CompanyId");

                    b.ToTable("Company_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.BusDriver", b =>
                {
                    b.Property<int>("DriverId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DriverId"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<DateOnly>("DateOfBirth")
                        .HasColumnType("date");

                    b.Property<string>("DriverName")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<bool>("IsAssigned")
                        .HasColumnType("bit");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<string>("LicenseNumber")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("LicensePhotoPath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("DriverId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("UserId")
                        .IsUnique();

                    b.ToTable("Driver_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.DriverAssignment", b =>
                {
                    b.Property<int>("AssignmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AssignmentId"));

                    b.Property<int?>("BusId")
                        .HasColumnType("int");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("EndDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.HasKey("AssignmentId");

                    b.HasIndex("BusId");

                    b.HasIndex("DriverId");

                    b.ToTable("DriverAssign_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.EsewaTransaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionId"));

                    b.Property<string>("BookingId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("MerchantCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("PaidAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<string>("PaymentId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("ProductCharge")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<string>("ReferenceId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("ServiceCharge")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<decimal>("TaxAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<DateTime>("TransactionDate")
                        .HasColumnType("datetime2");

                    b.HasKey("TransactionId");

                    b.ToTable("EsewaTransaction_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Merchant", b =>
                {
                    b.Property<int>("MerchantId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MerchantId"));

                    b.Property<int?>("CompanyId")
                        .HasColumnType("int");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("MerchantCode")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal>("ProductCharge")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<decimal>("ServiceCharge")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<decimal>("TaxAmount")
                        .HasColumnType("decimal(10, 2)");

                    b.HasKey("MerchantId");

                    b.HasIndex("CompanyId");

                    b.HasIndex("DriverId");

                    b.ToTable("Merchant_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.OTP", b =>
                {
                    b.Property<int>("OtpId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("OtpId"));

                    b.Property<DateTime>("ExpiresAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsUsed")
                        .HasColumnType("bit");

                    b.Property<int>("OtpCode")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("OtpId");

                    b.HasIndex("UserId");

                    b.ToTable("Otp_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Passenger", b =>
                {
                    b.Property<int>("PassengerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PassengerId"));

                    b.Property<string>("BoardingPoint")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("DroppingPoint")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.HasKey("PassengerId");

                    b.ToTable("Passenger_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Payment", b =>
                {
                    b.Property<int>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PaymentId"));

                    b.Property<decimal>("AmountPaid")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("BookingId")
                        .HasColumnType("int");

                    b.Property<int?>("PassengerId")
                        .HasColumnType("int");

                    b.Property<DateTime>("PaymentDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("PaymentMethod")
                        .HasMaxLength(20)
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasMaxLength(20)
                        .HasColumnType("int");

                    b.Property<int?>("TransactionId")
                        .HasColumnType("int");

                    b.Property<int?>("UserId")
                        .HasColumnType("int");

                    b.HasKey("PaymentId");

                    b.HasIndex("BookingId");

                    b.HasIndex("PassengerId");

                    b.HasIndex("TransactionId");

                    b.HasIndex("UserId");

                    b.ToTable("Payment_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Route", b =>
                {
                    b.Property<int>("RouteID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RouteID"));

                    b.Property<int>("CompanyID")
                        .HasColumnType("int");

                    b.Property<string>("EndLocation")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("EstimatedTime")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StartLocation")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<string>("Stops")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("RouteID");

                    b.HasIndex("CompanyID");

                    b.ToTable("Route_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Schedule", b =>
                {
                    b.Property<int>("ScheduleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ScheduleId"));

                    b.Property<DateTime>("ArrivalTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("AvailableSeats")
                        .HasColumnType("int");

                    b.Property<int?>("BusCompanyId")
                        .HasColumnType("int");

                    b.Property<int?>("BusId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DepartureTime")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("RouteId")
                        .HasColumnType("int");

                    b.Property<string>("Status")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("ScheduleId");

                    b.HasIndex("BusCompanyId");

                    b.HasIndex("BusId");

                    b.HasIndex("DriverId");

                    b.HasIndex("RouteId");

                    b.ToTable("Schedule_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Seat", b =>
                {
                    b.Property<int>("SeatId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("SeatId"));

                    b.Property<int?>("BookingId")
                        .HasColumnType("int");

                    b.Property<int>("BusId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ReservedAt")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ReservedByUserId")
                        .HasColumnType("int");

                    b.Property<string>("SeatNumber")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("SeatId");

                    b.HasIndex("BookingId");

                    b.HasIndex("BusId");

                    b.HasIndex("ReservedByUserId");

                    b.ToTable("Seat_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Service", b =>
                {
                    b.Property<int>("ServiceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ServiceId"));

                    b.Property<bool>("AC")
                        .HasColumnType("bit");

                    b.Property<int>("BusId")
                        .HasColumnType("int");

                    b.Property<int?>("BusType")
                        .HasColumnType("int");

                    b.Property<string>("Essentials")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("essentials");

                    b.Property<string>("SafetyFeatures")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("safety_features");

                    b.Property<int?>("SeatType")
                        .HasColumnType("int");

                    b.Property<string>("Snacks")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasColumnName("snacks");

                    b.Property<bool>("Wifi")
                        .HasColumnType("bit");

                    b.HasKey("ServiceId");

                    b.HasIndex("BusId")
                        .IsUnique();

                    b.ToTable("Service_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Ticket", b =>
                {
                    b.Property<int>("TicketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TicketId"));

                    b.Property<int>("BookingId")
                        .HasColumnType("int");

                    b.Property<int?>("BusId")
                        .HasColumnType("int");

                    b.Property<string>("PNR")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.Property<int?>("PassengerId")
                        .HasColumnType("int");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(10, 2)");

                    b.Property<int>("SeatId")
                        .HasColumnType("int");

                    b.Property<string>("TicketNo")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("TicketId");

                    b.HasIndex("BookingId");

                    b.HasIndex("BusId");

                    b.HasIndex("PassengerId");

                    b.HasIndex("SeatId");

                    b.ToTable("Ticket_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<int>("Auth_Method")
                        .HasColumnType("int");

                    b.Property<int?>("CompanyID")
                        .HasColumnType("int");

                    b.Property<DateTime>("Created_At")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Google_Id")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("OTP")
                        .HasColumnType("int");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNo")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProfilePicPath")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime>("Updated_At")
                        .HasColumnType("datetime2");

                    b.HasKey("UserId");

                    b.HasIndex("CompanyID");

                    b.ToTable("User_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Booking", b =>
                {
                    b.HasOne("YatriSewa.Models.Bus", "Bus")
                        .WithMany()
                        .HasForeignKey("BusId");

                    b.HasOne("YatriSewa.Models.Passenger", "Passenger")
                        .WithMany("Bookings")
                        .HasForeignKey("PassengerId");

                    b.HasOne("YatriSewa.Models.User", "User")
                        .WithMany("Bookings")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bus");

                    b.Navigation("Passenger");

                    b.Navigation("User");
                });

            modelBuilder.Entity("YatriSewa.Models.Bus", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "BusCompany")
                        .WithMany("Buses")
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YatriSewa.Models.BusDriver", "BusDriver")
                        .WithMany("Buses")
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("YatriSewa.Models.Route", "Route")
                        .WithMany("Buses")
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("BusCompany");

                    b.Navigation("BusDriver");

                    b.Navigation("Route");
                });

            modelBuilder.Entity("YatriSewa.Models.BusDriver", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "BusCompany")
                        .WithMany()
                        .HasForeignKey("CompanyId");

                    b.HasOne("YatriSewa.Models.User", "User")
                        .WithOne("BusDriver")
                        .HasForeignKey("YatriSewa.Models.BusDriver", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BusCompany");

                    b.Navigation("User");
                });

            modelBuilder.Entity("YatriSewa.Models.DriverAssignment", b =>
                {
                    b.HasOne("YatriSewa.Models.Bus", "Bus")
                        .WithMany()
                        .HasForeignKey("BusId");

                    b.HasOne("YatriSewa.Models.BusDriver", "BusDriver")
                        .WithMany()
                        .HasForeignKey("DriverId");

                    b.Navigation("Bus");

                    b.Navigation("BusDriver");
                });

            modelBuilder.Entity("YatriSewa.Models.Merchant", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId");

                    b.HasOne("YatriSewa.Models.BusDriver", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverId");

                    b.Navigation("Company");

                    b.Navigation("Driver");
                });

            modelBuilder.Entity("YatriSewa.Models.OTP", b =>
                {
                    b.HasOne("YatriSewa.Models.User", "User_Table")
                        .WithMany("Otp_Table")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User_Table");
                });

            modelBuilder.Entity("YatriSewa.Models.Payment", b =>
                {
                    b.HasOne("YatriSewa.Models.Booking", "Booking")
                        .WithMany()
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YatriSewa.Models.Passenger", "Passenger")
                        .WithMany()
                        .HasForeignKey("PassengerId");

                    b.HasOne("YatriSewa.Models.EsewaTransaction", "EsewaTransaction")
                        .WithMany()
                        .HasForeignKey("TransactionId");

                    b.HasOne("YatriSewa.Models.User", null)
                        .WithMany("Payments")
                        .HasForeignKey("UserId");

                    b.Navigation("Booking");

                    b.Navigation("EsewaTransaction");

                    b.Navigation("Passenger");
                });

            modelBuilder.Entity("YatriSewa.Models.Route", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "BusCompany")
                        .WithMany()
                        .HasForeignKey("CompanyID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BusCompany");
                });

            modelBuilder.Entity("YatriSewa.Models.Schedule", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "BusCompany")
                        .WithMany()
                        .HasForeignKey("BusCompanyId");

                    b.HasOne("YatriSewa.Models.Bus", "Bus")
                        .WithMany("Schedules")
                        .HasForeignKey("BusId");

                    b.HasOne("YatriSewa.Models.BusDriver", "Driver")
                        .WithMany()
                        .HasForeignKey("DriverId");

                    b.HasOne("YatriSewa.Models.Route", "Route")
                        .WithMany()
                        .HasForeignKey("RouteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Bus");

                    b.Navigation("BusCompany");

                    b.Navigation("Driver");

                    b.Navigation("Route");
                });

            modelBuilder.Entity("YatriSewa.Models.Seat", b =>
                {
                    b.HasOne("YatriSewa.Models.Booking", "Booking")
                        .WithMany()
                        .HasForeignKey("BookingId");

                    b.HasOne("YatriSewa.Models.Bus", "Bus")
                        .WithMany("Seats")
                        .HasForeignKey("BusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YatriSewa.Models.User", "ReservedByUser")
                        .WithMany()
                        .HasForeignKey("ReservedByUserId");

                    b.Navigation("Booking");

                    b.Navigation("Bus");

                    b.Navigation("ReservedByUser");
                });

            modelBuilder.Entity("YatriSewa.Models.Service", b =>
                {
                    b.HasOne("YatriSewa.Models.Bus", "Bus")
                        .WithOne("Service")
                        .HasForeignKey("YatriSewa.Models.Service", "BusId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Bus");
                });

            modelBuilder.Entity("YatriSewa.Models.Ticket", b =>
                {
                    b.HasOne("YatriSewa.Models.Booking", "Booking")
                        .WithMany("Tickets")
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("YatriSewa.Models.Bus", null)
                        .WithMany("Tickets")
                        .HasForeignKey("BusId");

                    b.HasOne("YatriSewa.Models.Passenger", "Passenger")
                        .WithMany()
                        .HasForeignKey("PassengerId");

                    b.HasOne("YatriSewa.Models.Seat", "Seat")
                        .WithMany()
                        .HasForeignKey("SeatId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Booking");

                    b.Navigation("Passenger");

                    b.Navigation("Seat");
                });

            modelBuilder.Entity("YatriSewa.Models.User", b =>
                {
                    b.HasOne("YatriSewa.Models.BusCompany", "BusCompany")
                        .WithMany()
                        .HasForeignKey("CompanyID");

                    b.Navigation("BusCompany");
                });

            modelBuilder.Entity("YatriSewa.Models.Booking", b =>
                {
                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("YatriSewa.Models.Bus", b =>
                {
                    b.Navigation("Schedules");

                    b.Navigation("Seats");

                    b.Navigation("Service");

                    b.Navigation("Tickets");
                });

            modelBuilder.Entity("YatriSewa.Models.BusCompany", b =>
                {
                    b.Navigation("Buses");
                });

            modelBuilder.Entity("YatriSewa.Models.BusDriver", b =>
                {
                    b.Navigation("Buses");
                });

            modelBuilder.Entity("YatriSewa.Models.Passenger", b =>
                {
                    b.Navigation("Bookings");
                });

            modelBuilder.Entity("YatriSewa.Models.Route", b =>
                {
                    b.Navigation("Buses");
                });

            modelBuilder.Entity("YatriSewa.Models.User", b =>
                {
                    b.Navigation("Bookings");

                    b.Navigation("BusDriver");

                    b.Navigation("Otp_Table");

                    b.Navigation("Payments");
                });
#pragma warning restore 612, 618
        }
    }
}
