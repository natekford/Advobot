using Advobot.Modules;
using Advobot.Utilities;

namespace Advobot.CommandMarking.Responses
{
	public sealed class Rules : CommandResponses
	{
		private Rules() { }

		public static AdvobotResult CreatedCategory(string category)
			=> Success(Default.FormatInterpolated($"Successfully created the rule category {category}."));
		public static AdvobotResult ModifiedCategoryName(string category, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the name of the rule category {category} to {name}."));
		public static AdvobotResult DeletedCategory(string category)
			=> Success(Default.FormatInterpolated($"Successfully deleted the rule category {category}."));
		public static AdvobotResult AddedRule(string category)
			=> Success(Default.FormatInterpolated($"Successfully added a rule to the rule category {category}."));
		public static AdvobotResult InsertedRule(string category, int position)
			=> Success(Default.FormatInterpolated($"Successfully inserted a rule at position #{position} in the rule category {category}."));
		public static AdvobotResult RemovedRule(string category, int position)
			=> Success(Default.FormatInterpolated($"Successfully removed a rule at position #{position} in the rule category {category}."));
		public static AdvobotResult RuleAlreadyExists()
			=> Success("The supplied rule already exists.");
		public static AdvobotResult InvalidRuleInsert(int position)
			=> Failure(Default.FormatInterpolated($"{position} is an invalid position to insert at.")).WithTime(DefaultTime);
		public static AdvobotResult InvalidRuleRemove(int position)
			=> Failure(Default.FormatInterpolated($"{position} is an invalid position to remove at.")).WithTime(DefaultTime);
	}
}
