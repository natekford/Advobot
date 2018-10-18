using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Rules
{
	[Category(typeof(ModifyRuleCategories)), Group(nameof(ModifyRuleCategories)), TopLevelShortAlias(typeof(ModifyRuleCategories))]
	[Summary("Modifies the rule categories which hold rules.")]
	[UserPermissionRequirement(GuildPermission.Administrator)]
	[DefaultEnabled(false)]
	//[SaveGuildSettings]
	public sealed class ModifyRuleCategories : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([ValidateRuleCategory] string name)
		{
			if (Context.GuildSettings.Rules.Categories.Keys.CaseInsContains(name))
			{
				await ReplyErrorAsync(new Error($"The category `{name}` already exists.")).CAF();
				return;
			}

			var pos = Context.GuildSettings.Rules.Categories.Count + 1;
			Context.GuildSettings.Rules.Categories.Add(name, new List<string>());
			await ReplyTimedAsync($"Successfully created the category `{name}` at `{pos}`.").CAF();
		}
		[Command(nameof(ChangeName)), ShortAlias(nameof(ChangeName))]
		public async Task ChangeName([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRuleCategory] string newName)
		{
			var oldVal = Context.GuildSettings.Rules.Categories[category];
			Context.GuildSettings.Rules.Categories.Remove(category);
			Context.GuildSettings.Rules.Categories.Add(category, oldVal);
			await ReplyTimedAsync($"Successfully changed the category `{category}` to `{newName}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category)
		{
			Context.GuildSettings.Rules.Categories.Remove(category);
			await ReplyTimedAsync($"Successfully removed the category `{category}`.").CAF();
		}
	}

#warning redo how index is parsed
	[Category(typeof(ModifyRules)), Group(nameof(ModifyRules)), TopLevelShortAlias(typeof(ModifyRules))]
	[Summary("Modifies the rules which are saved in the bot settings.")]
	[UserPermissionRequirement(GuildPermission.Administrator)]
	[DefaultEnabled(false)]
	//[SaveGuildSettings]
	public sealed class ModifyRules : AdvobotModuleBase
	{
		[Command(nameof(Add)), ShortAlias(nameof(Add))]
		public async Task Add([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRule] string rule)
		{
			if (Context.GuildSettings.Rules.Categories[category].CaseInsContains(rule))
			{
				await ReplyErrorAsync(new Error($"The supplied rule already exists.")).CAF();
				return;
			}

			Context.GuildSettings.Rules.Categories[category].Add(rule);
			await ReplyTimedAsync($"Successfully added a rule in `{category}`.").CAF();
		}
		[Command(nameof(Insert)), ShortAlias(nameof(Insert))]
		public async Task Insert(
			[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
			[ValidatePositiveNumber] int index,
			[ValidateRule] string rule)
		{
			var count = Context.GuildSettings.Rules.Categories[category].Count;
			Context.GuildSettings.Rules.Categories[category].Insert(Math.Min(index, count - 1), rule);
			await ReplyTimedAsync($"Successfully removed the rule at index `{index}` in `{category}`.").CAF();
		}
		[Command(nameof(Remove)), ShortAlias(nameof(Remove))]
		public async Task Remove(
			[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
			[ValidatePositiveNumber] int index)
		{
			if (Context.GuildSettings.Rules.Categories[category].Count >= index)
			{
				await ReplyErrorAsync(new Error($"{index} is an invalid position to remove at.")).CAF();
				return;
			}
			Context.GuildSettings.Rules.Categories[category].RemoveAt(Math.Min(index, int.MaxValue));
			await ReplyTimedAsync($"Successfully removed the rule at index `{index}` in `{category}`.").CAF();
		}
	}

	[Category(typeof(PrintOutRules)), Group(nameof(PrintOutRules)), TopLevelShortAlias(typeof(PrintOutRules))]
	[Summary("Prints out the rules with given formatting options. " +
		"`Format` uses the `" + nameof(RuleFormat) + "` enum. " +
		"`TitleFormat` and `RuleFormat` use the `" + nameof(MarkDownFormat) + "` enum. " +
		"`FormatOptions` use the `" + nameof(RuleFormatOption) + "` enum.")]
	[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
	[DefaultEnabled(false)]
	public sealed class PrintOutRules : AdvobotModuleBase
	{
		[Command]
		public Task Command([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [Optional, Remainder] RuleFormatter args)
			=> CommandRunner(args, category);
		[Command]
		public async Task Command([Optional, Remainder] RuleFormatter args)
		{
			if (Context.GuildSettings.Rules.Categories.Count == 0)
			{
				await ReplyErrorAsync(new Error("This guild has no rules set up.")).CAF();
				return;
			}

			await CommandRunner(args, null).CAF();
		}

		private async Task CommandRunner(RuleFormatter formatter, string category)
		{
			foreach (var part in Context.GuildSettings.Rules.GetParts(formatter ?? new RuleFormatter(), category))
			{
				await ReplyAsync(part).CAF();
			}
		}
	}
}
