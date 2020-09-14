using System;

using Advobot.Modules;
using Advobot.Utilities;

using static Advobot.Resources.Responses;
using static Advobot.Utilities.FormattingUtils;

namespace Advobot.AutoMod.Responses
{
	public sealed class SpamPrevention : AdvobotResult
	{
		private SpamPrevention() : base(null, "")
		{
		}

		public static AdvobotResult AlreadyToggledPrevention(Enum type, bool enabled)
		{
			var format = enabled ? SpamPreventionAlreadyEnabled : SpamPreventionAlreadyDisabled;
			return Success(format.Format(
				type.ToString().WithBlock(),
				GetType(type)
			));
		}

		public static AdvobotResult CreatedPrevention(Enum type)
		{
			return Success(SpamPreventionCreated.Format(
				type.ToString().WithBlock(),
				GetType(type)
			));
		}

		public static AdvobotResult NoPrevention(Enum type)
		{
			return Success(SpamPreventionNotFound.Format(
				type.ToString().WithBlock(),
				GetType(type)
			));
		}

		public static AdvobotResult ToggledPrevention(Enum type, bool enabled)
		{
			var format = enabled ? SpamPreventionEnabled : SpamPreventionDisabled;
			return Success(format.Format(
				type.ToString().WithBlock(),
				GetType(type)
			));
		}

		private static MarkdownFormattedArg GetType(Enum type) => type switch
		{
			RaidType _ => VariableRaid.WithNoMarkdown(),
			SpamType _ => VariableSpam.WithNoMarkdown(),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}
}