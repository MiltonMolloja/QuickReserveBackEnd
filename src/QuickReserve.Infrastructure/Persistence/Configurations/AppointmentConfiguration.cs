// -----------------------------------------------------------------------
// <copyright file="AppointmentConfiguration.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Infrastructure.Persistence.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.ValueObjects;

/// <summary>
/// EF Core configuration for the <see cref="Appointment"/> aggregate root.
/// Maps value objects and owned entities to flat table columns.
/// </summary>
public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    /// <inheritdoc/>
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.PlaceId)
            .IsRequired();

        builder.Property(a => a.AppointmentAt)
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        // ServiceType Value Object conversion
        builder.Property(a => a.ServiceType)
            .HasConversion(
                v => v.Value,
                v => ServiceType.Create(v))
            .HasMaxLength(100)
            .IsRequired();

        // Contact as Owned Entity
        builder.OwnsOne(a => a.Contact, contact =>
        {
            contact.Property(c => c.Name)
                .HasColumnName("ContactName")
                .HasMaxLength(200)
                .IsRequired();

            contact.Property(c => c.Email)
                .HasConversion(
                    v => v.Value,
                    v => Email.Create(v))
                .HasColumnName("ContactEmail")
                .HasMaxLength(254)
                .IsRequired();

            contact.Property(c => c.Phone)
                .HasConversion(
                    v => v.Value,
                    v => Phone.Create(v))
                .HasColumnName("ContactPhone")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Vehicle as Owned Entity (optional)
        builder.OwnsOne(a => a.Vehicle, vehicle =>
        {
            vehicle.Property(v => v.Make)
                .HasColumnName("VehicleMake")
                .HasMaxLength(100);

            vehicle.Property(v => v.Model)
                .HasColumnName("VehicleModel")
                .HasMaxLength(100);

            vehicle.Property(v => v.Year)
                .HasColumnName("VehicleYear");

            vehicle.Property(v => v.LicensePlate)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? LicensePlate.Create(v) : null)
                .HasColumnName("VehicleLicensePlate")
                .HasMaxLength(10);
        });

        builder.Navigation(a => a.Contact).IsRequired();
        builder.Navigation(a => a.Vehicle);
    }
}
