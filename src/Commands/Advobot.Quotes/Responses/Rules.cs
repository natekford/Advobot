using System.Collections.Generic;

using Advobot.Modules;
using Advobot.Quotes.ReadOnlyModels;
using Advobot.Utilities;

using static Advobot.Resources.Responses;

namespace Advobot.Quotes.Responses
{
	public sealed class Rules : AdvobotResult
	{
		private Rules() : base(null, "")
		{
		}

		public static AdvobotResult AddedRule(IReadOnlyRuleCategory category)
		{
			return Success(RulesAddedRule.Format(
				category.Category.ToString().WithBlock()
			));
		}

		public static AdvobotResult CreatedCategory(IReadOnlyRuleCategory category)
		{
			return Success(RulesCreatedCategory.Format(
				category.Category.ToString().WithBlock()
			));
		}

		public static AdvobotResult DeletedCategory(IReadOnlyRuleCategory category)
		{
			return Success(RulesDeletedCategory.Format(
				category.Category.ToString().WithBlock()
			));
		}

		public static AdvobotResult InvalidRuleInsert(int position)
		{
			return Success(RulesInvalidRuleInsert.Format(
				position.ToString().WithBlock()
			));
		}

		public static AdvobotResult InvalidRuleRemove(int position)
		{
			return Success(RulesInvalidRuleRemove.Format(
				position.ToString().WithBlock()
			));
		}

		public static AdvobotResult ModifiedCategoryValue(IReadOnlyRuleCategory category)
		{
			return Success(RulesModifiedCategoryValue.Format(
				category.Category.ToString().WithBlock(),
				category.Value.WithBlock()
			));
		}

		public static AdvobotResult ModifiedRuleValue(IReadOnlyRule rule)
		{
			return Success(RulesModifiedRuleValue.Format(
				$"{rule.Category}.{rule.Position}".WithBlock(),
				rule.Value.WithBlock()
			));
		}

		public static AdvobotResult RemovedRule(IReadOnlyRule rule)
		{
			return Success(RulesRemovedRule.Format(
				rule.Position.ToString().WithBlock(),
				rule.Category.ToString().WithBlock()
			));
		}

		public static AdvobotResult RuleAlreadyExists()
			=> Success(RulesRuleAlreadyExists);

		public static AdvobotResult SwappedRuleCategories(
			IReadOnlyRuleCategory categoryA,
			IReadOnlyList<IReadOnlyRule> rulesA,
			IReadOnlyRuleCategory categoryB,
			IReadOnlyList<IReadOnlyRule> rulesB)
		{
			return Success(RulesSwappedRuleCategories.Format(
				categoryA.Category.ToString().WithBlock(),
				rulesA.Count.ToString().WithBlock(),
				categoryB.Category.ToString().WithBlock(),
				rulesB.Count.ToString().WithBlock()
			));
		}

		public static AdvobotResult SwappedRules(IReadOnlyRule ruleA, IReadOnlyRule ruleB)
		{
			return Success(RulesSwappedRules.Format(
				ruleA.Position.ToString().WithBlock(),
				ruleB.Position.ToString().WithBlock(),
				ruleA.Category.ToString().WithBlock()
			));
		}
	}
}