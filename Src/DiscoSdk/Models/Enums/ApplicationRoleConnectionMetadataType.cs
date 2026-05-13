namespace DiscoSdk.Models.Enums;

/// <summary>
/// The comparison operation used when evaluating an application role connection metadata record
/// against a user's connected-account value.
/// </summary>
public enum ApplicationRoleConnectionMetadataType
{
	/// <summary>The metadata value (integer) is less than or equal to the guild's configured value.</summary>
	IntegerLessThanOrEqual = 1,

	/// <summary>The metadata value (integer) is greater than or equal to the guild's configured value.</summary>
	IntegerGreaterThanOrEqual = 2,

	/// <summary>The metadata value (integer) is equal to the guild's configured value.</summary>
	IntegerEqual = 3,

	/// <summary>The metadata value (integer) is not equal to the guild's configured value.</summary>
	IntegerNotEqual = 4,

	/// <summary>The metadata value (ISO 8601 datetime) is less than or equal to the guild's configured value (days before current date).</summary>
	DatetimeLessThanOrEqual = 5,

	/// <summary>The metadata value (ISO 8601 datetime) is greater than or equal to the guild's configured value (days before current date).</summary>
	DatetimeGreaterThanOrEqual = 6,

	/// <summary>The metadata value (integer) is equal to the guild's configured value (1).</summary>
	BooleanEqual = 7,

	/// <summary>The metadata value (integer) is not equal to the guild's configured value (1).</summary>
	BooleanNotEqual = 8
}
