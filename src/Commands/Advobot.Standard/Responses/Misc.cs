using Advobot.Classes;
using Advobot.Embeds;
using Advobot.Formatting;
using Advobot.Modules;
using Advobot.Services.HelpEntries;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord.Commands;

using System.Reflection;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.Standard.Responses;

public sealed class Misc : AdvobotResult
{
	private static readonly Type _Commands = typeof(Commands.Misc.Commands);
	private static readonly Type _Help = typeof(Commands.Misc.Help);

	private Misc() : base(null, "")
	{
	}

	public static AdvobotResult CategoryCommands(
		IEnumerable<IModuleHelpEntry> entries,
		string category)
	{
		var title = MiscTitleCategoryCommands.Format(
			category.WithTitleCase()
		);
		var description = entries
			.Join(x => x.Name)
			.WithBigBlock()
			.Value;
		return Success(new EmbedWrapper
		{
			Title = title,
			Description = description,
		});
	}

	public static AdvobotResult GeneralCommandInfo(
		IEnumerable<string> categories,
		string prefix)
	{
		var description = MiscGeneralCommandInfo.Format(
			GetPrefixedCommand(prefix, _Commands, VariableCategoryParameter),
			categories.Join().WithBigBlock()
		);
		return Success(new EmbedWrapper
		{
			Title = MiscTitleCategories,
			Description = description,
		});
	}

	public static AdvobotResult GeneralHelp(string prefix)
	{
		var description = MiscGeneralHelp.Format(
			GetPrefixedCommand(prefix, _Commands),
			GetPrefixedCommand(prefix, _Help, VariableCategoryParameter)
		);
		var syntaxFieldValue = MiscBasicSyntax.Format(
			(VariableRequiredLeft + VariableRequiredRight).WithBlock(),
			(VariableOptionalLeft + VariableOptionalRight).WithBlock(),
			VariableOr.WithBlock()
		);
		return Success(new EmbedWrapper
		{
			Title = MiscTitleGeneralHelp,
			Description = description,
			Footer = new() { Text = MiscFooterHelp },
			Fields = new()
			{
				new()
				{
					Name = MiscTitleBasicSyntax,
					Value = syntaxFieldValue,
					IsInline = true,
				},
				new()
				{
					Name = MiscTitleMentionSyntax,
					Value = MiscMentionSyntax,
					IsInline = true,
				},
				new()
				{
					Name = MiscTitleLinks,
					Value = MiscLinks.Format(
						Constants.REPO.WithNoMarkdown(),
						Constants.INVITE.WithNoMarkdown()
					),
					IsInline = false,
				},
			},
		});
	}

	public static AdvobotResult Help(IModuleHelpEntry module)
	{
		var info = new InformationMatrix();
		var top = info.CreateCollection();
		top.Add(MiscTitleAliases, module.Aliases.Join());
		top.Add(MiscTitleBasePermissions, FormatPreconditions(module.Preconditions));
		var description = info.CreateCollection();
		description.Add(MiscTitleDescription, module.Summary);
		var meta = info.CreateCollection();
		meta.Add(MiscTitleEnabledByDefault, module.EnabledByDefault);
		meta.Add(MiscTitleAbleToBeToggled, module.AbleToBeToggled);

		if (module.Submodules.Count != 0)
		{
			var submodules = "\n" + module.Submodules
				.Select((x, i) => $"{i + 1}. {x.Name}")
				.Join("\n")
				.WithBigBlock()
				.Value;
			info.CreateCollection().Add(MiscTitleSubmodules, submodules);
		}

		if (module.Commands.Count != 0)
		{
			var commands = "\n" + module.Commands.Select((x, i) =>
			{
				var parameters = x.Parameters.Join(FormatParameter);
				var name = string.IsNullOrWhiteSpace(x.Name) ? "" : x.Name + " ";
				return $"{i + 1}. {name}({parameters})";
			}).Join("\n").WithBigBlock().Value;
			info.CreateCollection().Add(MiscTitleCommands, commands);
		}

		return Success(CreateHelpEmbed(module.Aliases[0], info.ToString()));
	}

	public static AdvobotResult Help(
		IModuleHelpEntry module,
		int position)
	{
		if (module.Commands.Count < position)
		{
			return Failure(MiscInvalidHelpEntryNumber.Format(
				position.ToString().WithBlock(),
				module.Name.WithBlock()
			));
		}
		var command = module.Commands[position - 1];

		var info = new InformationMatrix();
		var top = info.CreateCollection();
		top.Add(MiscTitleAliases, command.Aliases.Join(x => x.WithBlock().Value));
		top.Add(MiscTitleBasePermissions, FormatPreconditions(command.Preconditions));
		var description = info.CreateCollection();
		description.Add(MiscTitleDescription, command.Summary);

		var embed = CreateHelpEmbed(command.Aliases[0], info.ToString());
		foreach (var parameter in command.Parameters)
		{
			var paramInfo = new InformationMatrix();
			var paramTop = paramInfo.CreateCollection();
			paramTop.Add(MiscTitleBasePermissions, FormatPreconditions(parameter.Preconditions));
			paramTop.Add(MiscTitleDescription, parameter.Summary);
			paramTop.Add(MiscTitleNamedArguments, parameter.NamedArguments.Join());

			embed.TryAddField(FormatParameter(parameter), paramInfo.ToString(), true, out _);
		}
		return Success(embed);
	}

	public static AdvobotResult MakeAnEmbed(CustomEmbed embed)
		=> Success(embed.BuildWrapper());

	public static AdvobotResult Remind(TimeSpan time)
	{
		return Success(MiscRemind.Format(
			time.ToString("00:00:00").WithBlock()
		));
	}

	private static EmbedWrapper CreateHelpEmbed(string name, string entry)
	{
		return new()
		{
			Title = name,
			Description = entry,
			Footer = new() { Text = MiscFooterHelp, },
		};
	}

	private static string FormatParameter(IParameterHelpEntry p)
	{
		var left = p.IsOptional ? VariableOptionalLeft : VariableRequiredLeft;
		var right = p.IsOptional ? VariableOptionalRight : VariableRequiredRight;
		return left + p.Name + right;
	}

	private static string FormatPreconditions(IEnumerable<IPrecondition> preconditions)
	{
		if (!preconditions.Any())
		{
			return VariableNotApplicable;
		}
		if (preconditions.Any(x => x.Group == null))
		{
			return preconditions.Join(x => x.Summary, VariableAnd);
		}

		var groups = preconditions
			.GroupBy(x => x.Group)
			.Select(g => g.Join(x => x.Summary, VariableOr))
			.ToArray();
		if (groups.Length == 1)
		{
			return groups[0];
		}
		return groups.Join(g => $"({g})", VariableAnd);
	}

	private static string FormatPreconditions(IEnumerable<IParameterPrecondition> preconditions)
	{
		if (!preconditions.Any())
		{
			return VariableNotApplicable;
		}
		return preconditions.Join(x => x.Summary, VariableAnd);
	}

	private static MarkdownFormattedArg GetPrefixedCommand(
		string prefix,
		Type command,
		string args = "")
	{
		var attr = command.GetCustomAttribute<GroupAttribute>();
		if (attr == null)
		{
			throw new ArgumentException("Group is null.", nameof(command));
		}

		return $"{prefix}{attr.Prefix} {args}".WithBlock();
	}
}