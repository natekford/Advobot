using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Advobot.Attributes;
using Advobot.Attributes.ParameterPreconditions;
using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Attributes.ParameterPreconditions.Strings;
using Advobot.Attributes.Preconditions.Permissions;
using Advobot.Formatting.Rules;
using Advobot.Modules;
using Advobot.Services.GuildSettings;
using Advobot.Settings.Localization;
using Advobot.Settings.Resources;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

namespace Advobot.Settings.Commands
{
	[Category(nameof(Rules))]
	public sealed class Rules : ModuleBase
	{
		[LocalizedGroup(nameof(Groups.ModifyRuleCategories))]
		[LocalizedAlias(nameof(Aliases.ModifyRuleCategories))]
		[LocalizedSummary(nameof(Summaries.ModifyRuleCategories))]
		[Meta("29ce9d5e-59c0-4262-8922-e444a9fc0ec6")]
		[RequireGuildPermissions]
		public sealed class ModifyRuleCategories : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[LocalizedCommand(nameof(Groups.Create))]
			[LocalizedAlias(nameof(Aliases.Create))]
			public Task<RuntimeResult> Create(
				[RuleCategory(Status = ExistenceStatus.MustNotExist)] string name)
			{
				Settings.Rules.Categories.Add(name, new List<string>());
				return Responses.Rules.CreatedCategory(name);
			}

			[LocalizedCommand(nameof(Groups.Delete))]
			[LocalizedAlias(nameof(Aliases.Delete))]
			public Task<RuntimeResult> Delete([RuleCategory] string category)
			{
				Settings.Rules.Categories.Remove(category);
				return Responses.Rules.DeletedCategory(category);
			}

			[LocalizedCommand(nameof(Groups.ModifyName))]
			[LocalizedAlias(nameof(Aliases.ModifyName))]
			public Task<RuntimeResult> ModifyName(
				[RuleCategory] string category,
				[RuleCategory(Status = ExistenceStatus.MustNotExist)] string newName)
			{
				var temp = Settings.Rules.Categories[category];
				Settings.Rules.Categories.Remove(category);
				Settings.Rules.Categories.Add(newName, temp);
				return Responses.Rules.ModifiedCategoryName(category, newName);
			}
		}

		[LocalizedGroup(nameof(Groups.ModifyRules))]
		[LocalizedAlias(nameof(Aliases.ModifyRules))]
		[LocalizedSummary(nameof(Summaries.ModifyRules))]
		[Meta("2808540d-9dd7-4c4a-bd87-b6bd83c37cd5")]
		[RequireGuildPermissions]
		public sealed class ModifyRules : SettingsModule<IGuildSettings>
		{
			protected override IGuildSettings Settings => Context.Settings;

			[LocalizedCommand(nameof(Groups.Add))]
			[LocalizedAlias(nameof(Aliases.Add))]
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

			[LocalizedCommand(nameof(Groups.Insert))]
			[LocalizedAlias(nameof(Aliases.Insert))]
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

			[LocalizedCommand(nameof(Groups.Remove))]
			[LocalizedAlias(nameof(Aliases.Remove))]
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

		[LocalizedGroup(nameof(Groups.PrintOutRules))]
		[LocalizedAlias(nameof(Aliases.PrintOutRules))]
		[LocalizedSummary(nameof(Summaries.PrintOutRules))]
		[Meta("9ae48ca4-68a3-468f-8a6c-2cffd4483deb")]
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