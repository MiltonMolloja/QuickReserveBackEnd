// -----------------------------------------------------------------------
// <copyright file="AppointmentSeeder.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.Persistence;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuickReserve.Domain.Entities;

/// <summary>
/// Seeds the in-memory database with sample appointments on application startup.
/// Generates 4 random appointments per workshop per day for the next 5 business days.
/// </summary>
public static class AppointmentSeeder
{
    private static readonly int[] WorkshopIds = [2, 3, 5, 7];

    private static readonly int[] AvailableHours = [9, 10, 11, 12, 13, 14, 15, 16, 17];

    private static readonly string[] ServiceTypes =
    [
        "Mantenimiento",
        "Reparacion",
        "Revision Tecnica",
        "Diagnostico",
    ];

    private static readonly string[] FirstNames =
    [
        "Juan", "María", "Carlos", "Ana", "Pedro",
        "Laura", "Diego", "Sofía", "Martín", "Valentina",
        "Lucas", "Camila", "Nicolás", "Florencia", "Matías",
        "Julieta", "Tomás", "Agustina", "Federico", "Rocío",
    ];

    private static readonly string[] LastNames =
    [
        "García", "Rodríguez", "Martínez", "López", "González",
        "Pérez", "Sánchez", "Ramírez", "Torres", "Flores",
        "Díaz", "Morales", "Álvarez", "Romero", "Acosta",
        "Fernández", "Gómez", "Ruiz", "Herrera", "Medina",
    ];

    private static readonly string[] VehicleMakes =
    [
        "Toyota", "Ford", "Volkswagen", "Chevrolet", "Fiat",
        "Renault", "Peugeot", "Honda", "Hyundai", "Nissan",
    ];

    private static readonly string[][] VehicleModels =
    [
        ["Corolla", "Hilux", "Etios", "Yaris"],
        ["Ranger", "Focus", "EcoSport", "Ka"],
        ["Gol", "Amarok", "Polo", "T-Cross"],
        ["Cruze", "Onix", "Tracker", "S10"],
        ["Cronos", "Argo", "Strada", "Toro"],
        ["Kwid", "Sandero", "Duster", "Logan"],
        ["208", "308", "2008", "Partner"],
        ["Civic", "HR-V", "Fit", "City"],
        ["Tucson", "Creta", "HB20", "Santa Fe"],
        ["Kicks", "Frontier", "Sentra", "Versa"],
    ];

    /// <summary>
    /// Seeds sample appointments into the database.
    /// </summary>
    /// <param name="serviceProvider">The application service provider.</param>
    /// <returns>A task representing the async operation.</returns>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();

        if (context.Appointments.Any())
        {
            logger.LogInformation("Database already seeded, skipping");
            return;
        }

        var random = new Random(42); // Fixed seed for reproducible data
        var appointments = GenerateAppointments(random);

        await context.Appointments.AddRangeAsync(appointments);
        await context.SaveChangesAsync();

        logger.LogInformation("Seeded {Count} appointments for {Workshops} workshops", appointments.Count, WorkshopIds.Length);
    }

    private static List<Appointment> GenerateAppointments(Random random)
    {
        var appointments = new List<Appointment>();
        var businessDays = GetNextBusinessDays(5);

        foreach (var workshopId in WorkshopIds)
        {
            foreach (var day in businessDays)
            {
                // Pick 4 random unique hours for this workshop on this day
                var selectedHours = AvailableHours
                    .OrderBy(_ => random.Next())
                    .Take(4)
                    .OrderBy(h => h)
                    .ToArray();

                foreach (var hour in selectedHours)
                {
                    var appointmentAt = day.AddHours(hour);
                    var appointment = CreateRandomAppointment(random, workshopId, appointmentAt);
                    appointments.Add(appointment);
                }
            }
        }

        return appointments;
    }

    private static List<DateTime> GetNextBusinessDays(int count)
    {
        var days = new List<DateTime>();

        // Argentina is UTC-3. We want hours like 09:00 ART = 12:00 UTC.
        // Get tomorrow's date in Argentina time, then work in UTC.
        var todayArgentina = DateTime.UtcNow.AddHours(-3).Date;
        var current = todayArgentina.AddDays(1);

        while (days.Count < count)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                // Store as UTC midnight + 3h offset (Argentina midnight = 03:00 UTC)
                days.Add(current.AddHours(3));
            }

            current = current.AddDays(1);
        }

        return days;
    }

    private static Appointment CreateRandomAppointment(Random random, int workshopId, DateTime appointmentAt)
    {
        var firstName = FirstNames[random.Next(FirstNames.Length)];
        var lastName = LastNames[random.Next(LastNames.Length)];
        var fullName = $"{firstName} {lastName}";
        var email = $"{firstName.ToLowerInvariant()}.{lastName.ToLowerInvariant()}@email.com";
        var whatsapp = $"+54911{random.Next(10000000, 99999999)}";

        var serviceType = ServiceTypes[random.Next(ServiceTypes.Length)];

        var makeIndex = random.Next(VehicleMakes.Length);
        var make = VehicleMakes[makeIndex];
        var model = VehicleModels[makeIndex][random.Next(VehicleModels[makeIndex].Length)];
        var year = random.Next(2018, 2027);

        // Generate Argentine license plate (new format: AA000AA)
        var plateLetters1 = $"{(char)random.Next('A', 'Z' + 1)}{(char)random.Next('A', 'Z' + 1)}";
        var plateNumbers = random.Next(100, 999).ToString();
        var plateLetters2 = $"{(char)random.Next('A', 'Z' + 1)}{(char)random.Next('A', 'Z' + 1)}";
        var licensePlate = $"{plateLetters1}{plateNumbers}{plateLetters2}";

        return Appointment.Create(
            placeId: workshopId,
            appointmentAt: appointmentAt,
            serviceType: serviceType,
            contactName: fullName,
            contactEmail: email,
            contactWhatsapp: whatsapp,
            vehicleMake: make,
            vehicleModel: model,
            vehicleYear: year,
            vehicleLicensePlate: licensePlate);
    }
}
