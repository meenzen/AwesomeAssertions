#nullable enable
#pragma warning disable

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using AwesomeAssertions.Extensions;

namespace AwesomeAssertions.Builders;

/// <summary>A builder for creating dates.</summary>
/// <param name="value">
/// The <see cref="DateTime"/> value.
/// </param>
public readonly struct DateTimeBuilder(DateTime value)
    : IEquatable<DateTimeBuilder>
    , IEquatable<DateTime>
    , IEquatable<DateTimeOffset>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly DateTime Value = value;

    /// <summary>
    /// Gets a new <see cref="DateTimeBuilder"/> with the kind set to local.
    /// </summary>
    public DateTimeBuilder Local => new(new(Value.Ticks, DateTimeKind.Local));

    /// <summary>
    /// Gets a new <see cref="DateTimeBuilder"/> with the kind set to UTC.
    /// </summary>
    public DateTimeBuilder Utc => new (new(Value.Ticks, DateTimeKind.Utc));

    /// <summary>
    /// Gets a new <see cref="DateTimeBuilder"/> with the kind set to unspecified.
    /// </summary>
    public DateTimeBuilder Unspecified => new(new(Value.Ticks, DateTimeKind.Unspecified));

    /// <summary>Creates a new <see cref="DateTimeOffset"/> with the specified offset.</summary>
    /// <param name="hours">
    /// The hours of the offset.
    /// </param>
    /// <param name="minutes">
    /// The minutes of the offset.
    /// </param>
    [Pure]
    public DateTimeOffset WithOffset(int hours, int minutes = 0)
        => WithOffset(new TimeSpan(hours, minutes, 00));

    /// <summary>Creates a new <see cref="DateTimeOffset"/> with the specified offset.</summary>
    /// <param name="offset">
    /// The offset.
    /// </param>
    [Pure]
    public DateTimeOffset WithOffset(TimeSpan offset)
        => offset == TimeSpan.Zero
        ? new(new DateTime(Value.Ticks, DateTimeKind.Utc), TimeSpan.Zero)
        : new(new DateTime(Value.Ticks, DateTimeKind.Unspecified), offset);

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj) => obj switch
    {
        DateTimeBuilder d => Equals(d),
        DateTime d => Equals(d),
        DateTimeOffset d => Equals(d),
        _ => false,
    };

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateTimeBuilder other)
        => Value == other.Value;

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateTime other)
        => Value == other;

    /// <inheritdoc />
    [Pure]
    public bool Equals(DateTimeOffset other)
        => Value.WithOffset(TimeSpan.Zero) == other;

    /// <summary>
    /// Converts the <see cref="DateTimeBuilder"/> to a <see cref="DateTime"/>.
    /// </summary>
    /// <param name="builder">
    /// The builder to convert.
    /// </param>
    public static implicit operator DateTime(DateTimeBuilder builder)
        => builder.Value;

    /// <summary>
    /// Converts the <see cref="DateTimeBuilder"/> to a <see cref="DateTimeBuilder"/>.
    /// </summary>
    /// <param name="builder">
    /// The builder to convert.
    /// </param>
    /// <remarks>
    /// Converts with an offset of zero, which is equivalent to UTC.
    /// </remarks>
    public static implicit operator DateTimeOffset(DateTimeBuilder builder)
        => builder.WithOffset(TimeSpan.Zero);
}
