using DiscoSdk.Models.Enums;
using DiscoSdk.Models.Messages.Components;

namespace DiscoSdk.Models.Messages;

/// <summary>
/// Shared validation / fluent surface for User/Role/Channel/Mentionable select builders. Each
/// concrete builder adds the type-specific knobs (e.g. <see cref="ChannelSelectBuilder.WithChannelTypes"/>).
/// </summary>
public abstract class EntitySelectBuilderBase<TSelf, TComponent>
	where TSelf : EntitySelectBuilderBase<TSelf, TComponent>
	where TComponent : class
{
	protected readonly TComponent _component;
	protected EntitySelectBuilderBase(TComponent component) => _component = component;

	protected abstract void SetCustomId(string customId);
	protected abstract void SetPlaceholder(string? placeholder);
	protected abstract void SetMinValues(int? min);
	protected abstract void SetMaxValues(int? max);
	protected abstract void SetDisabled(bool? disabled);
	protected abstract void SetDefaultValues(SelectDefaultValue[]? values);
	protected abstract SelectDefaultValue[]? GetDefaultValues();

	public TSelf WithPlaceholder(string placeholder)
	{
		if (placeholder?.Length > 150)
			throw new ArgumentException("Placeholder cannot exceed 150 characters.", nameof(placeholder));
		SetPlaceholder(placeholder);
		return (TSelf)this;
	}

	public TSelf WithMinValues(int min)
	{
		if (min is < 0 or > 25)
			throw new ArgumentOutOfRangeException(nameof(min), "MinValues must be between 0 and 25.");
		SetMinValues(min);
		return (TSelf)this;
	}

	public TSelf WithMaxValues(int max)
	{
		if (max is < 1 or > 25)
			throw new ArgumentOutOfRangeException(nameof(max), "MaxValues must be between 1 and 25.");
		SetMaxValues(max);
		return (TSelf)this;
	}

	public TSelf WithDisabled(bool disabled = true)
	{
		SetDisabled(disabled);
		return (TSelf)this;
	}

	protected void AppendDefault(Snowflake id, string type)
	{
		var existing = GetDefaultValues() ?? [];
		SetDefaultValues([.. existing, new SelectDefaultValue { Id = id, Type = type }]);
	}

	IInteractionComponent BuildBase() => (IInteractionComponent)_component!;
}

/// <summary>Fluent builder for <see cref="UserSelectComponent"/> (type 5).</summary>
public sealed class UserSelectBuilder : EntitySelectBuilderBase<UserSelectBuilder, UserSelectComponent>, IInteractionComponentBuilder
{
	public UserSelectBuilder(string customId) : base(new UserSelectComponent())
	{
		if (string.IsNullOrWhiteSpace(customId) || customId.Length > 100)
			throw new ArgumentException("Custom ID is required and must be ≤ 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>Pre-selects a user.</summary>
	public UserSelectBuilder WithDefaultUser(Snowflake userId) { AppendDefault(userId, "user"); return this; }

	public UserSelectComponent Build() => _component;
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();

	protected override void SetCustomId(string customId) => _component.CustomId = customId;
	protected override void SetPlaceholder(string? placeholder) => _component.Placeholder = placeholder;
	protected override void SetMinValues(int? min) => _component.MinValues = min;
	protected override void SetMaxValues(int? max) => _component.MaxValues = max;
	protected override void SetDisabled(bool? disabled) => _component.Disabled = disabled;
	protected override void SetDefaultValues(SelectDefaultValue[]? values) => _component.DefaultValues = values;
	protected override SelectDefaultValue[]? GetDefaultValues() => _component.DefaultValues;
}

/// <summary>Fluent builder for <see cref="RoleSelectComponent"/> (type 6).</summary>
public sealed class RoleSelectBuilder : EntitySelectBuilderBase<RoleSelectBuilder, RoleSelectComponent>, IInteractionComponentBuilder
{
	public RoleSelectBuilder(string customId) : base(new RoleSelectComponent())
	{
		if (string.IsNullOrWhiteSpace(customId) || customId.Length > 100)
			throw new ArgumentException("Custom ID is required and must be ≤ 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>Pre-selects a role.</summary>
	public RoleSelectBuilder WithDefaultRole(Snowflake roleId) { AppendDefault(roleId, "role"); return this; }

	public RoleSelectComponent Build() => _component;
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();

	protected override void SetCustomId(string customId) => _component.CustomId = customId;
	protected override void SetPlaceholder(string? placeholder) => _component.Placeholder = placeholder;
	protected override void SetMinValues(int? min) => _component.MinValues = min;
	protected override void SetMaxValues(int? max) => _component.MaxValues = max;
	protected override void SetDisabled(bool? disabled) => _component.Disabled = disabled;
	protected override void SetDefaultValues(SelectDefaultValue[]? values) => _component.DefaultValues = values;
	protected override SelectDefaultValue[]? GetDefaultValues() => _component.DefaultValues;
}

/// <summary>Fluent builder for <see cref="ChannelSelectComponent"/> (type 8).</summary>
public sealed class ChannelSelectBuilder : EntitySelectBuilderBase<ChannelSelectBuilder, ChannelSelectComponent>, IInteractionComponentBuilder
{
	public ChannelSelectBuilder(string customId) : base(new ChannelSelectComponent())
	{
		if (string.IsNullOrWhiteSpace(customId) || customId.Length > 100)
			throw new ArgumentException("Custom ID is required and must be ≤ 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	/// <summary>Restricts the picker to the given channel types.</summary>
	public ChannelSelectBuilder WithChannelTypes(params ChannelType[] types)
	{
		ArgumentNullException.ThrowIfNull(types);
		_component.ChannelTypes = types;
		return this;
	}

	/// <summary>Pre-selects a channel.</summary>
	public ChannelSelectBuilder WithDefaultChannel(Snowflake channelId) { AppendDefault(channelId, "channel"); return this; }

	public ChannelSelectComponent Build() => _component;
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();

	protected override void SetCustomId(string customId) => _component.CustomId = customId;
	protected override void SetPlaceholder(string? placeholder) => _component.Placeholder = placeholder;
	protected override void SetMinValues(int? min) => _component.MinValues = min;
	protected override void SetMaxValues(int? max) => _component.MaxValues = max;
	protected override void SetDisabled(bool? disabled) => _component.Disabled = disabled;
	protected override void SetDefaultValues(SelectDefaultValue[]? values) => _component.DefaultValues = values;
	protected override SelectDefaultValue[]? GetDefaultValues() => _component.DefaultValues;
}

/// <summary>Fluent builder for <see cref="MentionableSelectComponent"/> (type 7).</summary>
public sealed class MentionableSelectBuilder : EntitySelectBuilderBase<MentionableSelectBuilder, MentionableSelectComponent>, IInteractionComponentBuilder
{
	public MentionableSelectBuilder(string customId) : base(new MentionableSelectComponent())
	{
		if (string.IsNullOrWhiteSpace(customId) || customId.Length > 100)
			throw new ArgumentException("Custom ID is required and must be ≤ 100 characters.", nameof(customId));
		_component.CustomId = customId;
	}

	public MentionableSelectBuilder WithDefaultUser(Snowflake userId) { AppendDefault(userId, "user"); return this; }
	public MentionableSelectBuilder WithDefaultRole(Snowflake roleId) { AppendDefault(roleId, "role"); return this; }

	public MentionableSelectComponent Build() => _component;
	IInteractionComponent IInteractionComponentBuilder.Build() => Build();

	protected override void SetCustomId(string customId) => _component.CustomId = customId;
	protected override void SetPlaceholder(string? placeholder) => _component.Placeholder = placeholder;
	protected override void SetMinValues(int? min) => _component.MinValues = min;
	protected override void SetMaxValues(int? max) => _component.MaxValues = max;
	protected override void SetDisabled(bool? disabled) => _component.Disabled = disabled;
	protected override void SetDefaultValues(SelectDefaultValue[]? values) => _component.DefaultValues = values;
	protected override SelectDefaultValue[]? GetDefaultValues() => _component.DefaultValues;
}
