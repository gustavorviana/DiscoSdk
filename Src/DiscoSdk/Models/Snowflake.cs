using System.Globalization;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

/// <summary>
/// Represents a Discord Snowflake ID (64-bit unsigned integer).
/// All Discord IDs (users, channels, guilds, messages, roles, etc.) must use this type.
/// </summary>
/// <remarks>
/// <para>
/// Discord IDs are 64-bit unsigned integers (snowflakes) that uniquely identify resources in Discord.
/// This struct provides type safety and additional functionality for working with Discord IDs.
/// </para>
/// <para>
/// <strong>Important:</strong> All methods and properties that accept or return Discord IDs must use <see cref="Snowflake"/>,
/// not <see cref="ulong"/>, <see cref="string"/>, or any other type. This ensures type safety and consistency across the SDK.
/// </para>
/// <para>
/// The struct includes:
/// - Automatic JSON serialization/deserialization
/// - Conversion to/from <see cref="ulong"/> when needed
/// - Methods to extract creation timestamp and snowflake components
/// - Comparison and equality operators
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Creating a DiscordId from a string
/// var userId = DiscordId.Parse("123456789012345678");
/// 
/// // Creating from ulong
/// var channelId = (DiscordId)987654321098765432UL;
/// 
/// // Using in API calls
/// await channel.SendMessage()
///     .SetContent("Hello")
///     .SendAsync();
/// </code>
/// </example>
[JsonConverter(typeof(JsonConverters.SnowflakeConverter))]
public readonly struct Snowflake(ulong value) : IEquatable<Snowflake>, IComparable<Snowflake>
{
	private const long DiscordEpoch = 1420070400000L; // 2015-01-01T00:00:00Z

    public Snowflake(long value) : this((ulong)value)
    {

    }

    /// <summary>
    /// Gets a value indicating whether this Discord ID is empty (zero).
    /// </summary>
    public bool Empty => value == 0;

    public static Snowflake Parse(string value)
    {
        return new Snowflake(ulong.Parse(value, CultureInfo.InvariantCulture));
    }

    public static bool TryParse(string? value, out Snowflake snowflake)
    {
        if (string.IsNullOrEmpty(value))
        {
            snowflake = default;
            return false;
        }

        if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var v))
        {
            snowflake = new Snowflake(v);
            return true;
        }

        snowflake = default;
        return false;
    }

    public ulong Value => value;

    public DateTimeOffset CreatedAt
    {
        get
        {
            var timestamp = (long)(value >> 22);
            var milliseconds = timestamp + DiscordEpoch;
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
        }
    }

    public int WorkerId => (int)((value >> 17) & 0b1_1111);
    public int ProcessId => (int)((value >> 12) & 0b1_1111);
    public int Increment => (int)(value & 0b1111_1111_1111);

    public override string ToString()
    {
        return value.ToString(CultureInfo.InvariantCulture);
    }

    public bool Equals(Snowflake other)
    {
        return value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is Snowflake other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public int CompareTo(Snowflake other)
    {
        return value.CompareTo(other.Value);
    }

    public static implicit operator ulong(Snowflake snowflake)
    {
        return snowflake.Value;
    }

    public static explicit operator Snowflake(long value)
    {
        return new Snowflake(value);
    }

    public static implicit operator long(Snowflake snowflake)
    {
        return (long)snowflake.Value;
    }

    public static explicit operator Snowflake(ulong value)
    {
        return new Snowflake(value);
    }

    public static bool operator ==(Snowflake left, Snowflake right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Snowflake left, Snowflake right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Snowflake left, Snowflake right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(Snowflake left, Snowflake right)
    {
        return left.Value > right.Value;
    }
}