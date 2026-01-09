namespace DiscoSdk.Models;

/// <summary>
/// Represents a slowmode duration in seconds.
/// </summary>
public readonly struct Slowmode
{
	/// <summary>
	/// Gets the slowmode duration in seconds.
	/// </summary>
	public int Seconds { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="Slowmode"/> struct.
	/// </summary>
	/// <param name="seconds">The slowmode duration in seconds (0-21600).</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown when seconds is out of valid range.</exception>
	public Slowmode(int seconds)
	{
		if (seconds < 0 || seconds > 21600)
			throw new ArgumentOutOfRangeException(nameof(seconds), "Slowmode must be between 0 and 21600 seconds.");

		Seconds = seconds;
	}

	/// <summary>
	/// Creates a slowmode from seconds.
	/// </summary>
	/// <param name="seconds">The slowmode duration in seconds.</param>
	/// <returns>A new <see cref="Slowmode"/> instance.</returns>
	public static Slowmode FromSeconds(int seconds) => new(seconds);

	/// <summary>
	/// Implicitly converts an integer to a Slowmode.
	/// </summary>
	/// <param name="seconds">The slowmode duration in seconds.</param>
	public static implicit operator Slowmode(int seconds) => new(seconds);

	/// <summary>
	/// Implicitly converts a Slowmode to an integer.
	/// </summary>
	/// <param name="slowmode">The slowmode instance.</param>
	public static implicit operator int(Slowmode slowmode) => slowmode.Seconds;
}

