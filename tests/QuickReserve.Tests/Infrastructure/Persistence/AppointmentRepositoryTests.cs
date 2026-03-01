// -----------------------------------------------------------------------
// <copyright file="AppointmentRepositoryTests.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Tests.Infrastructure.Persistence;

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using QuickReserve.Domain.Entities;
using QuickReserve.Infrastructure.Persistence;
using QuickReserve.Infrastructure.Persistence.Repositories;

/// <summary>
/// Tests for <see cref="AppointmentRepository"/> using EF Core InMemory provider.
/// </summary>
public sealed class AppointmentRepositoryTests : IDisposable
{
    private readonly AppDbContext context;
    private readonly AppointmentRepository repository;

    public AppointmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        context = new AppDbContext(options);
        repository = new AppointmentRepository(context);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistAppointment()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        var result = await repository.AddAsync(appointment);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(appointment.Id);

        var persisted = await context.Appointments.FindAsync(appointment.Id);
        persisted.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnAppointment()
    {
        // Arrange
        var appointment = CreateTestAppointment();
        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(appointment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.PlaceId.Should().Be(appointment.PlaceId);
        result.Contact.Name.Should().Be(appointment.Contact.Name);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllAppointments()
    {
        // Arrange
        var appointment1 = CreateTestAppointment();
        var appointment2 = CreateTestAppointment();
        await context.Appointments.AddRangeAsync(appointment1, appointment2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnOrderedByCreatedAtDescending()
    {
        // Arrange
        var appointment1 = CreateTestAppointment();
        var appointment2 = CreateTestAppointment();
        await context.Appointments.AddAsync(appointment1);
        await context.SaveChangesAsync();

        // Small delay to ensure different CreatedAt
        await Task.Delay(10);
        await context.Appointments.AddAsync(appointment2);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].CreatedAt.Should().BeOnOrAfter(result[1].CreatedAt);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistContactAsOwnedEntity()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        await repository.AddAsync(appointment);

        // Assert - reload from context to verify owned entity persistence
        var persisted = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

        persisted.Should().NotBeNull();
        persisted!.Contact.Should().NotBeNull();
        persisted.Contact.Name.Should().Be("Juan Perez");
        persisted.Contact.Email.Value.Should().Be("juan@example.com");
        persisted.Contact.Whatsapp.Value.Should().Be("+5491155551234");
    }

    [Fact]
    public async Task AddAsync_WithVehicle_ShouldPersistVehicleAsOwnedEntity()
    {
        // Arrange
        var appointment = Appointment.Create(
            placeId: 1,
            appointmentAt: DateTime.UtcNow.AddDays(1),
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@example.com",
            contactWhatsapp: "+5491155551234",
            vehicleMake: "Toyota",
            vehicleModel: "Corolla",
            vehicleYear: 2023,
            vehicleLicensePlate: "AB123CD");

        // Act
        await repository.AddAsync(appointment);

        // Assert
        var persisted = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

        persisted.Should().NotBeNull();
        persisted!.Vehicle.Should().NotBeNull();
        persisted.Vehicle!.Make.Should().Be("Toyota");
        persisted.Vehicle.Model.Should().Be("Corolla");
        persisted.Vehicle.Year.Should().Be(2023);
        persisted.Vehicle.LicensePlate!.Value.Should().Be("AB123CD");
    }

    [Fact]
    public async Task AddAsync_WithoutVehicle_ShouldPersistNullVehicle()
    {
        // Arrange
        var appointment = CreateTestAppointment();

        // Act
        await repository.AddAsync(appointment);

        // Assert
        var persisted = await context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointment.Id);

        persisted.Should().NotBeNull();
        persisted!.Vehicle.Should().BeNull();
    }

    public void Dispose() => context.Dispose();

    private static Appointment CreateTestAppointment()
    {
        return Appointment.Create(
            placeId: 1,
            appointmentAt: DateTime.UtcNow.AddDays(1),
            serviceType: "Mantenimiento",
            contactName: "Juan Perez",
            contactEmail: "juan@example.com",
            contactWhatsapp: "+5491155551234");
    }
}
