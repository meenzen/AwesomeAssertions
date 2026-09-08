#nullable enable
#pragma warning disable

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace AwesomeAssertions.Builders;

/// <summary>A builder for creating dates.</summary>
/// <param name="year">
/// The year of the date.
/// </param>
/// <param name="month">
/// The month of the date.
/// </param>
/// <param name="day">
/// The day of the month of the date.
/// </param>
[StructLayout(LayoutKind.Auto)]
public readonly struct DateBuilder(int year, int month, int day)
    : IEquatable<DateBuilder>
#if NET6_0_OR_GREATER
    , IEquatable<DateOnly>
#endif
    , IEquatable<DateTime>
    , IEquatable<DateTimeOffset>
{
    /// <inheritdoc cref="DateTime.Year" />
    public int Year { get; } = year;

    /// <inheritdoc cref="DateTime.Month" />
    public int Month { get; } = month;

    /// <inheritdoc cref="DateTime.Day" />
    public int Day { get; } = day;

    /// <summary>Ats a time component.</summary>
    /// <param name="hours">
    /// The hours to add.
    /// </param>
    /// <param name="minutes">
    /// The minutes to add.
    /// </param>
    /// <param name="seconds">
    /// The seconds to add.
    /// </param>
    /// <returns>
    /// A <see cref="DateTimeBuilder"/> with the specified time component.
    /// </returns>
    [Pure]
    public DateTimeBuilder At(int hours, int minutes, int seconds = 0)
        => new(new DateTime(Year, Month, Day, hours, minutes, seconds, DateTimeKind.Utc));

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj) => obj switch
    {
        DateBuilder d => Equals(d),
        DateTime d => Equals(d),
        DateTimeOffset d => Equals(d),
#if NET6_0_OR_GREATER
        DateOnly d => Equals(d),
#endif
        _ => false,
    };

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateBuilder other)
        => Year == other.Year
        && Month == other.Month
        && Day == other.Day;

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateTime other)
        => Year == other.Year
        && Month == other.Month
        && Day == other.Day;

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateTimeOffset other)
        => Year == other.Year
        && Month == other.Month
        && Day == other.Day;

#if NET6_0_OR_GREATER
    /// <inheritdoc />
    [Pure]
    public bool Equals(DateOnly other)
        => Year == other.Year
        && Month == other.Month
        && Day == other.Day;
#endif

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode() => (Year * 10000) + (Month * 100) + Day;

    /// <summary>
    /// Implicitly converts a <see cref="DateBuilder"/> to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="builder">
    /// The builder to convert.
    /// </param>
    /// <remarks>
    /// The time is set to 00:00:00 and the kind set to UTC.
    /// </remarks>
    public static implicit operator DateTime(DateBuilder builder)
        => new(builder.Year, builder.Month, builder.Day, 00, 00, 00, DateTimeKind.Utc);

    /// <summary>
    /// Implicitly converts a <see cref="DateBuilder"/> to a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="builder">
    /// The builder to convert.
    /// </param>
    /// <remarks>
    /// The time is set to 00:00:00 and the offset is set to 0:00.
    /// </remarks>
    public static implicit operator DateTimeOffset(DateBuilder builder)
        => new(builder.Year, builder.Month, builder.Day, 00, 00, 00, TimeSpan.Zero);

#if NET6_0_OR_GREATER
    /// <summary>
    /// Implicitly converts a <see cref="DateBuilder"/> to a <see cref="DateOnly"/>.
    /// </summary>
    /// <param name="builder">
    /// The builder to convert.
    /// </param>
    public static implicit operator DateOnly(DateBuilder builder)
        => new(builder.Year, builder.Month, builder.Day);
#endif
}
