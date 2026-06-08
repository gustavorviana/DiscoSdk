using DiscoSdk.Contexts.Interactions;
using DiscoSdk.Hosting.Contexts.Models;
using DiscoSdk.Hosting.Rest.Actions;
using DiscoSdk.Hosting.Wrappers;
using DiscoSdk.Models.Commands;
using DiscoSdk.Models.Enums;
using DiscoSdk.Rest.Actions;
using InteractionOptionModel = DiscoSdk.Models.InteractionOption;

namespace DiscoSdk.Hosting.Contexts;

internal class AutoCompleteContext : InteractionContextWrapper, IAutoCompleteContext
{
	private const int MaxAutoCompleteChoices = 25;

	public AutoCompleteContext(DiscordClient client, InteractionWrapper interaction)
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
                throw new InvalidOperationException("AutoComplete interaction has no focused option or options.");
            }
        }

        FocusedOption = new AutoCompleteFocusedOption(focused.Name, focused.Type, focused.Value);
        Options = CollectOtherOptions(options, focused);
	}

	public string CommandName { get; }
	public string? Subcommand { get; }
	public string? SubcommandGroup { get; }
	public IAutoCompleteFocusedOption FocusedOption { get; }
	public IReadOnlyCollection<IAutoCompleteOptionValue> Options { get; }

	public IRestAction ReplyWithChoices(IEnumerable<SlashCommandOptionChoice> choices)
	{
		var list = choices?.Take(MaxAutoCompleteChoices + 1).ToList() ?? [];
		if (list.Count > MaxAutoCompleteChoices)
			throw new ArgumentOutOfRangeException(nameof(choices), $"AutoComplete allows at most {MaxAutoCompleteChoices} choices.");

		return RestAction.Create(async cancellationToken =>
		{
			await Client.InteractionClient.RespondWithAutoCompleteAsync(
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

	private static IReadOnlyCollection<IAutoCompleteOptionValue> CollectOtherOptions(
		InteractionOptionModel[]? options,
		InteractionOptionModel? excludeFocused)
	{
		var list = new List<IAutoCompleteOptionValue>();
		CollectOtherOptionsCore(options, excludeFocused, list);
		return list;
	}

	private static void CollectOtherOptionsCore(
		InteractionOptionModel[]? options,
		InteractionOptionModel? excludeFocused,
		List<IAutoCompleteOptionValue> list)
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
				list.Add(new AutoCompleteOptionValue(opt.Name, opt.Value));
		}
	}
}
