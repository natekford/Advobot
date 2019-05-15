using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Advobot.Classes;
using Advobot.Classes.Attributes;
using Advobot.Classes.Attributes.ParameterPreconditions.NumberValidation;
using Advobot.Classes.Attributes.ParameterPreconditions.StringValidation;
using Advobot.Classes.Attributes.Preconditions.Permissions;
using Advobot.Classes.Modules;
using Advobot.Classes.Results;
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
			public Task<RuntimeResult> Create([ValidateRuleCategory(ErrorOnCategoryExisting = true)] string name)
			{
				Settings.Rules.Categories.Add(name, new List<string>());
				return Responses.Rules.CreatedCategory(name);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> ModifyName([ValidateRuleCategory] string category, [ValidateRuleCategory(ErrorOnCategoryExisting = true)] string newName)
			{
				var temp = Settings.Rules.Categories[category];
				Settings.Rules.Categories.Remove(category);
				Settings.Rules.Categories.Add(newName, temp);
				return Responses.Rules.ModifiedCategoryName(category, newName);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Delete([ValidateRuleCategory] string category)
			{
				Settings.Rules.Categories.Remove(category);
				return Responses.Rules.DeletedCategory(category);
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
			public Task<RuntimeResult> Add([ValidateRuleCategory] string category, [ValidateRule] string rule)
			{
				if (Settings.Rules.Categories[category].CaseInsContains(rule))
				{
					return Responses.Rules.RuleAlreadyExists();
				}

				Settings.Rules.Categories[category].Add(rule);
				return Responses.Rules.AddedRule(category);
			}
			[ImplicitCommand, ImplicitAlias]
			public Task<RuntimeResult> Insert([ValidateRuleCategory] string category,
				[ValidatePositiveNumber] int position,
				[ValidateRule] string rule)
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
			public Task<RuntimeResult> Remove([ValidateRuleCategory] string category, [ValidatePositiveNumber] int position)
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
		[Summary("Prints out the rules with given formatting options. " +
			"`Format` uses the `" + nameof(RuleFormat) + "` enum. " +
			"`TitleFormat` and `RuleFormat` use the `" + nameof(MarkDownFormat) + "` enum. " +
			"`FormatOptions` use the `" + nameof(RuleFormatOption) + "` enum.")]
		[UserPermissionRequirement(PermissionRequirementAttribute.GenericPerms)]
		[EnabledByDefault(false)]
		public sealed class PrintOutRules : AdvobotModuleBase
		{
			[Command]
			public Task<RuntimeResult> Command([ValidateRuleCategory] string? category, [Optional, Remainder] RuleFormatter? args)
				=> AdvobotResult.FromReasonSegments(Context.GuildSettings.Rules.GetParts(args ?? new RuleFormatter(), category));
			[Command]
			public Task<RuntimeResult> Command([Optional, Remainder] RuleFormatter? args)
				=> Command(null, args);
		}
	}
}
