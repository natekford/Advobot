using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.TypeReaders;
using Advobot.Enums;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands
{
	public sealed class Rules : ModuleBase
	{
		[Group(nameof(ModifyRuleCategories)), ModuleInitialismAlias(typeof(ModifyRuleCategories))]
		[Summary("Modifies the rule categories which hold rules.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyRuleCategories : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Add([ValidateRuleCategory] string name)
			{
				if (Settings.Rules.Categories.Keys.CaseInsContains(name))
				{
					return ReplyErrorAsync($"The category `{name}` already exists.");
				}

				Settings.Rules.Categories.Add(name, new List<string>());
				return ReplyTimedAsync($"Successfully created the category `{name}` at `{Settings.Rules.Categories.Count + 1}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task ChangeName([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRuleCategory] string newName)
			{
				var value = Settings.Rules.Categories[category];
				Settings.Rules.Categories.Remove(category);
				Settings.Rules.Categories.Add(newName, value);
				return ReplyTimedAsync($"Successfully changed the category `{category}` to `{newName}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Remove([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category)
			{
				Settings.Rules.Categories.Remove(category);
				return ReplyTimedAsync($"Successfully removed the category `{category}`.");
			}
		}

		[Group(nameof(ModifyRules)), ModuleInitialismAlias(typeof(ModifyRules))]
		[Summary("Modifies the rules which are saved in the bot settings.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		public sealed class ModifyRules : AdvobotSettingsModuleBase<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.GuildSettings;

			[ImplicitCommand, ImplicitAlias]
			public Task Add([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRule] string rule)
			{
				if (Settings.Rules.Categories[category].CaseInsContains(rule))
				{
					return ReplyErrorAsync($"The supplied rule already exists.");
				}

				Settings.Rules.Categories[category].Add(rule);
				return ReplyTimedAsync($"Successfully added a rule in `{category}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Insert(
				[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
				[ValidatePositiveNumber] int position,
				[ValidateRule] string rule)
			{
				var count = Settings.Rules.Categories[category].Count;
				var index = Math.Min(position, count) - 1;
				Settings.Rules.Categories[category].Insert(index, rule);
				return ReplyTimedAsync($"Successfully inserted a rule at `{index + 1}` in `{category}`.");
			}
			[ImplicitCommand, ImplicitAlias]
			public Task Remove(
				[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
				[ValidatePositiveNumber] int position)
			{
				var index = position - 1;
				if (Settings.Rules.Categories[category].Count > index)
				{
					return ReplyErrorAsync($"{position} is an invalid position to remove at.");
				}
				Settings.Rules.Categories[category].RemoveAt(index);
				return ReplyTimedAsync($"Successfully removed the rule at `{position}` in `{category}`.");
			}
		}

		[Group(nameof(PrintOutRules)), ModuleInitialismAlias(typeof(PrintOutRules))]
		[Summary("Prints out the rules with given formatting options. " +
			"`Format` uses the `" + nameof(RuleFormat) + "` enum. " +
			"`TitleFormat` and `RuleFormat` use the `" + nameof(MarkDownFormat) + "` enum. " +
			"`FormatOptions` use the `" + nameof(RuleFormatOption) + "` enum.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		public sealed class PrintOutRules : AdvobotModuleBase
		{
			[Command]
			public Task Command([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [Optional, Remainder] RuleFormatter args)
				=> CommandRunner(args, category);
			[Command]
			public Task Command([Optional, Remainder] RuleFormatter args)
			{
				if (!Context.GuildSettings.Rules.Categories.Any())
				{
					return ReplyErrorAsync("This guild has no rules set up.");
				}
				return CommandRunner(args, null);
			}

			private async Task CommandRunner(RuleFormatter formatter, string? category)
			{
				foreach (var part in Context.GuildSettings.Rules.GetParts(formatter ?? new RuleFormatter(), category))
				{
					await ReplyAsync(part).CAF();
				}
			}
		}
	}
}
