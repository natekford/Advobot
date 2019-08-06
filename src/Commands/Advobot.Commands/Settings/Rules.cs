using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Commands.Localization;
using Advobot.Commands.Resources;
using Advobot.Formatting.Rules;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Commands.Settings
{
	public sealed class Rules : ModuleBase
	{
		[Group(nameof(ModifyRuleCategories)), ModuleInitialismAlias(typeof(ModifyRuleCategories))]
		[LocalizedSummary(nameof(Summaries.ModifyRuleCategories))]
		[CommandMeta("29ce9d5e-59c0-4262-8922-e444a9fc0ec6")]
		[RequireGuildPermissions]
		public sealed class ModifyRuleCategories : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Create(
				[RuleCategory(Status = ExistenceStatus.MustNotExist)] string name)
			{
				Settings.Rules.Categories.Add(name, new List<string>());
				return Responses.Rules.CreatedCategory(name);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> ModifyName(
				[RuleCategory] string category,
				[RuleCategory(Status = ExistenceStatus.MustNotExist)] string newName)
			{
				var temp = Settings.Rules.Categories[category];
				Settings.Rules.Categories.Remove(category);
				Settings.Rules.Categories.Add(newName, temp);
				return Responses.Rules.ModifiedCategoryName(category, newName);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Delete([RuleCategory] string category)
			{
				Settings.Rules.Categories.Remove(category);
				return Responses.Rules.DeletedCategory(category);
			}
		}

		[Group(nameof(ModifyRules)), ModuleInitialismAlias(typeof(ModifyRules))]
		[LocalizedSummary(nameof(Summaries.ModifyRules))]
		[CommandMeta("2808540d-9dd7-4c4a-bd87-b6bd83c37cd5")]
		[RequireGuildPermissions]
		public sealed class ModifyRules : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Add(
				[RuleCategory] string category,
				[Rule] string rule)
			{
				if (Settings.Rules.Categories[category].CaseInsContains(rule))
				{
					return Responses.Rules.RuleAlreadyExists();
				}

				Settings.Rules.Categories[category].Add(rule);
				return Responses.Rules.AddedRule(category);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Insert(
				[RuleCategory] string category,
				[Positive] int position,
				[Rule] string rule)
			{
				var index = position - 1;
				if (Settings.Rules.Categories[category].Count > index)
				{
					return Responses.Rules.InvalidRuleInsert(position);
				}

				Settings.Rules.Categories[category].Insert(index, rule);
				return Responses.Rules.InsertedRule(category, position);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Remove(
				[RuleCategory] string category,
				[Positive] int position)
			{
				var index = position - 1;
				if (Settings.Rules.Categories[category].Count > index)
				{
					return Responses.Rules.InvalidRuleRemove(position);
				}

				Settings.Rules.Categories[category].RemoveAt(index);
				return Responses.Rules.RemovedRule(category, position);
			}
		}

		[Group(nameof(PrintOutRules)), ModuleInitialismAlias(typeof(PrintOutRules))]
		[LocalizedSummary(nameof(Summaries.PrintOutRules))]
		[CommandMeta("9ae48ca4-68a3-468f-8a6c-2cffd4483deb")]
		[RequireGenericGuildPermissions]
		public sealed class PrintOutRules : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([Optional] RuleFormatter? args)
				=> Command(null, args);
			[Command]
			public Task<RuntimeResult> Command(
				[RuleCategory] string? category,
				[Optional] RuleFormatter? args)
			{
				args ??= new RuleFormatter();
				var segments = Context.Settings.Rules.GetParts(args, category);
				return AdvobotResult.FromReasonSegments(segments);
			}
		}
	}
}
