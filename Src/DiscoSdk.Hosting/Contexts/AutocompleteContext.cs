using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using InteractionOptionModel = DiscoSdk.Models.InteractionOption;

namespace DiscoSdk.Hosting.Contexts;

internal class AutocompleteContext : InteractionContextWrapper, IAutocompleteContext
{
	private const int MaxAutocompleteChoices = 25;

	public AutocompleteContext(DiscordClient client, InteractionWrapper interaction)
		: base(client, interaction)
	{
        var options = interaction.Data?.Options;
        var focused = FindFocusedOption(options);
        CommandName = interaction.Data?.Name ?? string.Empty;
        ExtractSubcommandInfo(options, out var subcommandGroup, out var subcommand);
        SubcommandGroup = subcommandGroup;
        Subcommand = subcommand;

        if (focused is null)
        {
            if (options is { Length: > 0 })
            {
                focused = options[0];
            }
            else
            {
                throw new InvalidOperationException("Autocomplete interaction has no focused option or options.");
            }
        }

        FocusedOption = new AutocompleteFocusedOption(focused.Name, focused.Type, focused.Value);
        Options = CollectOtherOptions(options, focused);
	}

	public string CommandName { get; }
	public string? Subcommand { get; }
	public string? SubcommandGroup { get; }
	public IAutocompleteFocusedOption FocusedOption { get; }
	public IReadOnlyCollection<IAutocompleteOptionValue> Options { get; }

	public IRestAction ReplyWithChoices(IEnumerable<SlashCommandOptionChoice> choices)
	{
		var list = choices?.Take(MaxAutocompleteChoices + 1).ToList() ?? [];
		if (list.Count > MaxAutocompleteChoices)
			throw new ArgumentOutOfRangeException(nameof(choices), $"Autocomplete allows at most {MaxAutocompleteChoices} choices.");

		return RestAction.Create(async cancellationToken =>
		{
			await Client.InteractionClient.RespondWithAutocompleteAsync(
				Interaction.Handle,
                list,
				cancellationToken);
		});
	}

	private static InteractionOptionModel? FindFocusedOption(InteractionOptionModel[]? options)
	{
		if (options is null or { Length: 0 })
			return null;

		foreach (var opt in options)
		{
			if (opt.Focused == true)
				return opt;
			var found = FindFocusedOption(opt.Options);
			if (found is not null)
				return found;
		}
		return null;
	}

	private static void ExtractSubcommandInfo(
		InteractionOptionModel[]? options,
		out string? subcommandGroup,
		out string? subcommand)
	{
		subcommandGroup = null;
		subcommand = null;

		if (options is null or { Length: 0 })
			return;

		var first = options[0];

		if (first.Type == SlashCommandOptionType.SubCommandGroup)
		{
			subcommandGroup = first.Name;
			if (first.Options is { Length: > 0 } nested && nested[0].Type == SlashCommandOptionType.SubCommand)
				subcommand = nested[0].Name;
		}
		else if (first.Type == SlashCommandOptionType.SubCommand)
		{
			subcommand = first.Name;
		}
	}

	private static IReadOnlyCollection<IAutocompleteOptionValue> CollectOtherOptions(
		InteractionOptionModel[]? options,
		InteractionOptionModel? excludeFocused)
	{
		var list = new List<IAutocompleteOptionValue>();
		CollectOtherOptionsCore(options, excludeFocused, list);
		return list;
	}

	private static void CollectOtherOptionsCore(
		InteractionOptionModel[]? options,
		InteractionOptionModel? excludeFocused,
		List<IAutocompleteOptionValue> list)
	{
		if (options is null)
			return;

		foreach (var opt in options)
		{
			if (ReferenceEquals(opt, excludeFocused))
				continue;

			if (opt.Type == SlashCommandOptionType.SubCommand ||
				opt.Type == SlashCommandOptionType.SubCommandGroup)
			{
				CollectOtherOptionsCore(opt.Options, excludeFocused, list);
				continue;
			}

			if (opt.Value is not null)
				list.Add(new AutocompleteOptionValue(opt.Name, opt.Value));
		}
	}
}
