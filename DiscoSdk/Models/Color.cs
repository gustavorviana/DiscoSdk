namespace DiscoSdk.Models;

/// <summary>
/// Represents a color for Discord embeds.
/// Colors are represented as RGB integers (0xRRGGBB format).
/// </summary>
public readonly struct Color : IEquatable<Color>
{
	private readonly int _value;

	/// <summary>
	/// Initializes a new instance of the <see cref="Color"/> struct.
	/// </summary>
	/// <param name="value">The color value (0xRRGGBB format).</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when the value is outside the valid range (0-0xFFFFFF).</exception>
	public Color(int value)
	{
		if (value < 0 || value > 0xFFFFFF)
			throw new ArgumentOutOfRangeException(nameof(value), "Color value must be between 0 and 0xFFFFFF.");

		_value = value;
	}

	/// <summary>
	/// Gets the color value as an integer (0xRRGGBB format).
	/// </summary>
	public int Value => _value;

	/// <summary>
	/// Black color (0x000000).
	/// </summary>
	public static Color Black => new(0x000000);

	/// <summary>
	/// White color (0xFFFFFF).
	/// </summary>
	public static Color White => new(0xFFFFFF);

	/// <summary>
	/// Red color (0xFF0000).
	/// </summary>
	public static Color Red => new(0xFF0000);

	/// <summary>
	/// Green color (0x00FF00).
	/// </summary>
	public static Color Green => new(0x00FF00);

	/// <summary>
	/// Blue color (0x0000FF).
	/// </summary>
	public static Color Blue => new(0x0000FF);

	/// <summary>
	/// Yellow color (0xFFFF00).
	/// </summary>
	public static Color Yellow => new(0xFFFF00);

	/// <summary>
	/// Cyan color (0x00FFFF).
	/// </summary>
	public static Color Cyan => new(0x00FFFF);

	/// <summary>
	/// Magenta color (0xFF00FF).
	/// </summary>
	public static Color Magenta => new(0xFF00FF);

	/// <summary>
	/// Orange color (0xFFA500).
	/// </summary>
	public static Color Orange => new(0xFFA500);

	/// <summary>
	/// Purple color (0x800080).
	/// </summary>
	public static Color Purple => new(0x800080);

	/// <summary>
	/// Pink color (0xFFC0CB).
	/// </summary>
	public static Color Pink => new(0xFFC0CB);

	/// <summary>
	/// Gray color (0x808080).
	/// </summary>
	public static Color Gray => new(0x808080);

	/// <summary>
	/// Dark gray color (0x404040).
	/// </summary>
	public static Color DarkGray => new(0x404040);

	/// <summary>
	/// Light gray color (0xC0C0C0).
	/// </summary>
	public static Color LightGray => new(0xC0C0C0);

	/// <summary>
	/// Gold color (0xFFD700).
	/// </summary>
	public static Color Gold => new(0xFFD700);

	/// <summary>
	/// Silver color (0xC0C0C0).
	/// </summary>
	public static Color Silver => new(0xC0C0C0);

	/// <summary>
	/// Creates a color from RGB values.
	/// </summary>
	/// <param name="r">Red component (0-255).</param>
	/// <param name="g">Green component (0-255).</param>
	/// <param name="b">Blue component (0-255).</param>
	/// <returns>A new <see cref="Color"/> instance.</returns>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when any component is outside the valid range (0-255).</exception>
	public static Color FromRgb(int r, int g, int b)
	{
		if (r < 0 || r > 255)
			throw new ArgumentOutOfRangeException(nameof(r), "Red component must be between 0 and 255.");
		if (g < 0 || g > 255)
			throw new ArgumentOutOfRangeException(nameof(g), "Green component must be between 0 and 255.");
		if (b < 0 || b > 255)
			throw new ArgumentOutOfRangeException(nameof(b), "Blue component must be between 0 and 255.");

		return new Color((r << 16) | (g << 8) | b);
	}

	/// <summary>
	/// Creates a color from a hex string (e.g., "#FF0000" or "FF0000").
	/// </summary>
	/// <param name="hex">The hex color string.</param>
	/// <returns>A new <see cref="Color"/> instance.</returns>
	/// <exception cref="ArgumentException">Thrown when the hex string is invalid.</exception>
	public static Color FromHex(string hex)
	{
		if (string.IsNullOrWhiteSpace(hex))
			throw new ArgumentException("Hex string cannot be null or empty.", nameof(hex));

		hex = hex.TrimStart('#');

		if (hex.Length != 6)
			throw new ArgumentException("Hex string must be 6 characters long.", nameof(hex));

		if (!int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out int color))
			throw new ArgumentException("Invalid hex string format.", nameof(hex));

		return new Color(color);
	}

	/// <summary>
	/// Implicitly converts an integer to a Color.
	/// </summary>
	/// <param name="value">The color value.</param>
	public static implicit operator Color(int value) => new(value);

	/// <summary>
	/// Implicitly converts a Color to an integer.
	/// </summary>
	/// <param name="color">The color.</param>
	public static implicit operator int(Color color) => color._value;

	/// <inheritdoc />
	public bool Equals(Color other) => _value == other._value;

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is Color other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => _value.GetHashCode();

	/// <summary>
	/// Equality operator.
	/// </summary>
	public static bool operator ==(Color left, Color right) => left.Equals(right);

	/// <summary>
	/// Inequality operator.
	/// </summary>
	public static bool operator !=(Color left, Color right) => !left.Equals(right);
}

