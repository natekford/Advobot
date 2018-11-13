﻿using System;
using System.Collections.Generic;
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
		//[SaveGuildSettings]
		public sealed class ModifyRuleCategories : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task Add([ValidateRuleCategory] string name)
			{
				if (Context.GuildSettings.Rules.Categories.Keys.CaseInsContains(name))
				{
					await ReplyErrorAsync($"The category `{name}` already exists.").CAF();
					return;
				}

				var pos = Context.GuildSettings.Rules.Categories.Count + 1;
				Context.GuildSettings.Rules.Categories.Add(name, new List<string>());
				await ReplyTimedAsync($"Successfully created the category `{name}` at `{pos}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task ChangeName([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRuleCategory] string newName)
			{
				var oldVal = Context.GuildSettings.Rules.Categories[category];
				Context.GuildSettings.Rules.Categories.Remove(category);
				Context.GuildSettings.Rules.Categories.Add(category, oldVal);
				await ReplyTimedAsync($"Successfully changed the category `{category}` to `{newName}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category)
			{
				Context.GuildSettings.Rules.Categories.Remove(category);
				await ReplyTimedAsync($"Successfully removed the category `{category}`.").CAF();
			}
		}

		[Group(nameof(ModifyRules)), ModuleInitialismAlias(typeof(ModifyRules))]
		[Summary("Modifies the rules which are saved in the bot settings.")]
		[UserPermissionRequirement(GuildPermission.Administrator)]
		[EnabledByDefault(false)]
		//[SaveGuildSettings]
		public sealed class ModifyRules : AdvobotModuleBase
		{
			[ImplicitCommand, ImplicitAlias]
			public async Task Add([OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category, [ValidateRule] string rule)
			{
				if (Context.GuildSettings.Rules.Categories[category].CaseInsContains(rule))
				{
					await ReplyErrorAsync($"The supplied rule already exists.").CAF();
					return;
				}

				Context.GuildSettings.Rules.Categories[category].Add(rule);
				await ReplyTimedAsync($"Successfully added a rule in `{category}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Insert(
				[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
				[ValidatePositiveNumber] int position,
				[ValidateRule] string rule)
			{
				--position;
				var count = Context.GuildSettings.Rules.Categories[category].Count;
				Context.GuildSettings.Rules.Categories[category].Insert(Math.Min(position, count - 1), rule);
				await ReplyTimedAsync($"Successfully removed the rule at `{position}` in `{category}`.").CAF();
			}
			[ImplicitCommand, ImplicitAlias]
			public async Task Remove(
				[OverrideTypeReader(typeof(RuleCategoryTypeReader))] string category,
				[ValidatePositiveNumber] int position)
			{
				--position;
				if (Context.GuildSettings.Rules.Categories[category].Count >= position)
				{
					await ReplyErrorAsync($"{position} is an invalid position to remove at.").CAF();
					return;
				}
				Context.GuildSettings.Rules.Categories[category].RemoveAt(Math.Min(position, int.MaxValue));
				await ReplyTimedAsync($"Successfully removed the rule at `{position}` in `{category}`.").CAF();
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
			public async Task Command([Optional, Remainder] RuleFormatter args)
			{
				if (Context.GuildSettings.Rules.Categories.Count == 0)
				{
					await ReplyErrorAsync("This guild has no rules set up.").CAF();
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
}
