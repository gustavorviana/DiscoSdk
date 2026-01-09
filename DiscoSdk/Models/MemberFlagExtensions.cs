using DiscoSdk.Models.Enums;
using System.Collections.Immutable;

namespace DiscoSdk.Models;

/// <summary>
/// Extension methods for <see cref="MemberFlag"/>.
/// </summary>
public static class MemberFlagExtensions
{
	/// <summary>
	/// Converts a raw integer value to a set of member flags.
	/// </summary>
	/// <param name="raw">The raw integer value.</param>
	/// <returns>An immutable set of member flags.</returns>
	public static ImmutableHashSet<MemberFlag> FromRaw(int raw)
	{
		var flags = ImmutableHashSet<MemberFlag>.Empty;

		foreach (MemberFlag flag in Enum.GetValues<MemberFlag>())
		{
			if ((raw & (int)flag) == (int)flag)
			{
				flags = flags.Add(flag);
			}
		}

		return flags;
	}

	/// <summary>
	/// Converts a collection of member flags to a raw integer value.
	/// </summary>
	/// <param name="flags">The collection of flags.</param>
	/// <returns>The raw integer value.</returns>
	public static int ToRaw(IEnumerable<MemberFlag> flags)
	{
		ArgumentNullException.ThrowIfNull(flags);

		int raw = 0;

		foreach (var flag in flags)
		{
			raw |= (int)flag;
		}

		return raw;
	}
}

