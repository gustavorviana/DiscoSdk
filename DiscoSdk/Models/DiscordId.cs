using System.Globalization;
using System.Text.Json.Serialization;

namespace DiscoSdk.Models;

[JsonConverter(typeof(JsonConverters.DiscordIdConverter))]
public readonly struct DiscordId(ulong value) : IEquatable<DiscordId>, IComparable<DiscordId>
{
    private const long DiscordEpoch = 1420070400000L; // 2015-01-01T00:00:00Z

    public bool Empty => value == 0;

    public static DiscordId Parse(string value)
    {
        return new DiscordId(ulong.Parse(value, CultureInfo.InvariantCulture));
    }

    public static bool TryParse(string value, out DiscordId snowflake)
    {
        if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var v))
        {
            snowflake = new DiscordId(v);
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

    public bool Equals(DiscordId other)
    {
        return value == other.Value;
    }

    public override bool Equals(object? obj)
    {
        return obj is DiscordId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return value.GetHashCode();
    }

    public int CompareTo(DiscordId other)
    {
        return value.CompareTo(other.Value);
    }

    public static implicit operator ulong(DiscordId snowflake)
    {
        return snowflake.Value;
    }

    public static explicit operator DiscordId(ulong value)
    {
        return new DiscordId(value);
    }

    public static bool operator ==(DiscordId left, DiscordId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(DiscordId left, DiscordId right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(DiscordId left, DiscordId right)
    {
        return left.Value < right.Value;
    }

    public static bool operator >(DiscordId left, DiscordId right)
    {
        return left.Value > right.Value;
    }
}
