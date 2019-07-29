using System.Collections.Generic;
using Advobot.Modules;
using Advobot.Utilities;

namespace Advobot.CommandMarking.Responses
{
	public sealed class ModifyCommands : CommandResponses
	{
		private ModifyCommands() { }

		public static AdvobotResult CannotBeEdited(string command)
			=> Failure(Default.FormatInterpolated($"{command} cannot be edited."));
		public static AdvobotResult Unmodified(string command, bool value)
			=> Failure(Default.FormatInterpolated($"{command} is already {GetEnabled(value)}."));
		public static AdvobotResult InvalidCategory(string category)
			=> Failure(Default.FormatInterpolated($"{category} is not a valid category."));
		public static AdvobotResult Modified(string command, bool value)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(value)} {command}.")).WithTime(DefaultTime);
		public static AdvobotResult ModifiedMultiple(IEnumerable<string> commands, bool value)
			=> Success(Default.FormatInterpolated($"Successfully {GetEnabled(value)} the following: {commands}.")).WithTime(DefaultTime);
	}
}
