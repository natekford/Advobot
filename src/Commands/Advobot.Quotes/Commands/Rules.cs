using Advobot.Attributes;
using Advobot.Localization;
using Advobot.Modules;
using Advobot.Preconditions.Permissions;
using Advobot.Quotes.Database;
using Advobot.Quotes.Formatting;
using Advobot.Quotes.Models;
using Advobot.Resources;

using Discord.Commands;

using static Advobot.Quotes.Responses.Rules;

namespace Advobot.Quotes.Commands;

[Category(nameof(Rules))]
public sealed class Rules : ModuleBase
{
	[LocalizedGroup(nameof(Groups.ModifyRuleCategories))]
	[LocalizedAlias(nameof(Aliases.ModifyRuleCategories))]
	[LocalizedSummary(nameof(Summaries.ModifyRuleCategories))]
	[Meta("29ce9d5e-59c0-4262-8922-e444a9fc0ec6")]
	[RequireGuildPermissions]
	public sealed class ModifyRuleCategories : RuleModuleBase
	{
		[LocalizedCommand(nameof(Groups.Create))]
		[LocalizedAlias(nameof(Aliases.Create))]
		public async Task<RuntimeResult> Create(
			[Remainder, ParameterPreconditions.Rule]
				string value)
		{
			var categories = await Db.GetCategoriesAsync(Context.Guild.Id).ConfigureAwait(false);

			var category = new RuleCategory
			{
				GuildId = Context.Guild.Id,
				Value = value,
				Category = categories.Count + 1,
			};
			await Db.UpsertRuleCategoryAsync(category).ConfigureAwait(false);
			return CreatedCategory(category);
		}

		[LocalizedCommand(nameof(Groups.Delete))]
		[LocalizedAlias(nameof(Aliases.Delete))]
		public async Task<RuntimeResult> Delete(RuleCategory category)
		{
			await Db.DeleteRuleCategoryAsync(category).ConfigureAwait(false);
			return DeletedCategory(category);
		}

		[LocalizedCommand(nameof(Groups.ModifyValue))]
		[LocalizedAlias(nameof(Aliases.ModifyValue))]
		public async Task<RuntimeResult> ModifyValue(
			RuleCategory category,
			[Remainder, ParameterPreconditions.Rule]
				string value)
		{
			var copy = category with
			{
				Value = value,
			};
			await Db.UpsertRuleCategoryAsync(copy).ConfigureAwait(false);
			return ModifiedCategoryValue(copy);
		}

		[LocalizedCommand(nameof(Groups.Swap))]
		[LocalizedAlias(nameof(Aliases.Swap))]
		public async Task<RuntimeResult> Swap(
			RuleCategory categoryA,
			RuleCategory categoryB)
		{
			var copyA = categoryA with
			{
				Category = categoryB.Category,
			};
			var rulesA = await Db.GetRulesAsync(categoryA).ConfigureAwait(false);
			var copyRulesA = rulesA.Select(x => x with
			{
				Category = copyA.Category,
			});

			var copyB = categoryB with
			{
				Category = categoryA.Category,
			};
			var rulesB = await Db.GetRulesAsync(categoryB).ConfigureAwait(false);
			var copyRulesB = rulesB.Select(x => x with
			{
				Category = copyB.Category,
			});

			await Db.UpsertRuleCategoryAsync(copyA).ConfigureAwait(false);
			await Db.UpsertRuleCategoryAsync(copyB).ConfigureAwait(false);
			await Db.UpsertRulesAsync(copyRulesA.Concat(copyRulesB)).ConfigureAwait(false);

			// Example:
			// 1.1, 1.2, 1.3, 1.4 | 2.1, 2.2
			// 1.1, 1.2, [1.3, 1.4] | 2.1, 2.2, 2.3, 2.4
			// 1.3 and 1.4 aren't upserted; only 2 rules from 2.x overwrite 1.1 and 1.2
			// so they need to be deleted manually
			if (rulesA.Count != rulesB.Count)
			{
				var (longer, shorter) = rulesA.Count > rulesB.Count
					? (rulesA, rulesB)
					: (rulesB, rulesA);
				var needsDeleting = longer.Skip(shorter.Count);
				await Db.DeleteRulesAsync(needsDeleting).ConfigureAwait(false);
			}

			return SwappedRuleCategories(categoryA, rulesA, categoryB, rulesB);
		}
	}

	[LocalizedGroup(nameof(Groups.ModifyRules))]
	[LocalizedAlias(nameof(Aliases.ModifyRules))]
	[LocalizedSummary(nameof(Summaries.ModifyRules))]
	[Meta("2808540d-9dd7-4c4a-bd87-b6bd83c37cd5")]
	[RequireGuildPermissions]
	public sealed class ModifyRules : RuleModuleBase
	{
		[LocalizedCommand(nameof(Groups.Create))]
		[LocalizedAlias(nameof(Aliases.Create))]
		public async Task<RuntimeResult> Create(
			RuleCategory category,
			[Remainder, ParameterPreconditions.Rule]
				string value)
		{
			var rules = await Db.GetRulesAsync(category).ConfigureAwait(false);

			var rule = new Models.Rule
			{
				GuildId = Context.Guild.Id,
				Category = category.Category,
				Value = value,
				Position = rules.Count + 1,
			};
			await Db.UpsertRuleAsync(rule).ConfigureAwait(false);
			return AddedRule(category);
		}

		[LocalizedCommand(nameof(Groups.Delete))]
		[LocalizedAlias(nameof(Aliases.Delete))]
		public async Task<RuntimeResult> Delete(Models.Rule rule)
		{
			await Db.DeleteRuleAsync(rule).ConfigureAwait(false);
			return RemovedRule(rule);
		}

		[LocalizedCommand(nameof(Groups.ModifyValue))]
		[LocalizedAlias(nameof(Aliases.ModifyValue))]
		public async Task<RuntimeResult> ModifyValue(
			Models.Rule rule,
			[Remainder, ParameterPreconditions.Rule]
				string value)
		{
			var copy = rule with
			{
				Value = value,
			};
			await Db.UpsertRuleAsync(copy).ConfigureAwait(false);
			return ModifiedRuleValue(copy);
		}

		[LocalizedCommand(nameof(Groups.Swap))]
		[LocalizedAlias(nameof(Aliases.Swap))]
		public async Task<RuntimeResult> Swap(Models.Rule ruleA, Models.Rule ruleB)
		{
			var copyA = ruleA with
			{
				Position = ruleB.Position,
			};
			var copyB = ruleB with
			{
				Position = ruleA.Position,
			};

			await Db.UpsertRulesAsync([copyA, copyB]).ConfigureAwait(false);
			return SwappedRules(ruleA, ruleB);
		}
	}

	[LocalizedGroup(nameof(Groups.PrintOutRules))]
	[LocalizedAlias(nameof(Aliases.PrintOutRules))]
	[LocalizedSummary(nameof(Summaries.PrintOutRules))]
	[Meta("9ae48ca4-68a3-468f-8a6c-2cffd4483deb")]
	[RequireGuildPermissions]
	public sealed class PrintOutRules : RuleModuleBase
	{
		[Command]
		public async Task<RuntimeResult> Command(RuleFormatter? args = null)
		{
			args ??= new();

			var dict = await Db.GetRuleDictionaryAsync(Context.Guild.Id).ConfigureAwait(false);
			return AdvobotResult.Success(args.Format(dict));
		}

		[Command]
		public async Task<RuntimeResult> Command(
			RuleCategory category,
			RuleFormatter? args = null)
		{
			args ??= new();

			var rules = await Db.GetRulesAsync(category).ConfigureAwait(false);
			return AdvobotResult.Success(args.Format(category, rules));
		}
	}
}