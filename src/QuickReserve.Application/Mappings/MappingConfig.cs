// -----------------------------------------------------------------------
// <copyright file="MappingConfig.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.Mappings;

using Mapster;
using QuickReserve.Application.DTOs.Responses;
using QuickReserve.Domain.Entities;
using QuickReserve.Domain.Interfaces;

/// <summary>
/// Mapster configuration for entity-to-DTO mappings.
/// </summary>
public static class MappingConfig
{
    /// <summary>
    /// Configures all Mapster type mappings.
    /// </summary>
    public static void Configure()
    {
        // Appointment -> AppointmentResponse
        TypeAdapterConfig<Appointment, AppointmentResponse>
            .NewConfig()
            .Map(dest => dest.ServiceType, src => src.ServiceType.Value)
            .Map(dest => dest.Contact, src => src.Contact)
            .Map(dest => dest.Vehicle, src => src.Vehicle);

        // Contact -> ContactResponse
        TypeAdapterConfig<Contact, ContactResponse>
            .NewConfig()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.Whatsapp, src => src.Whatsapp.Value);

        // Vehicle -> VehicleResponse
        TypeAdapterConfig<Vehicle, VehicleResponse>
            .NewConfig()
            .Map(dest => dest.LicensePlate, src => src.LicensePlate != null ? src.LicensePlate.Value : null);

        // WorkshopInfo -> WorkshopResponse
        TypeAdapterConfig<WorkshopInfo, WorkshopResponse>
            .NewConfig();
    }
}
