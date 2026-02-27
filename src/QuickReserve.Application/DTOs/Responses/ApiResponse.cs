// -----------------------------------------------------------------------
// <copyright file="ApiResponse.cs" company="QuickReserve">
//     Copyright (c) QuickReserve. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace QuickReserve.Application.DTOs.Responses;

/// <summary>
/// Generic API response wrapper for consistent response format.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
public sealed record ApiResponse<T>
{
    public bool Success { get; init; }

    public T? Data { get; init; }

    public IReadOnlyList<string>? Errors { get; init; }

    /// <summary>
    /// Creates a successful response with data.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <returns>A successful <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Ok(T data) => new()
    {
        Success = true,
        Data = data,
        Errors = null,
    };

    /// <summary>
    /// Creates a failed response with error messages.
    /// </summary>
    /// <param name="errors">The error messages.</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Fail(IEnumerable<string> errors) => new()
    {
        Success = false,
        Data = default,
        Errors = errors.ToList(),
    };

    /// <summary>
    /// Creates a failed response with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="ApiResponse{T}"/>.</returns>
    public static ApiResponse<T> Fail(string error) => Fail([error]);
}
