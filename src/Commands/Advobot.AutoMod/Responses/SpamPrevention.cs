using System;

using Advobot.Modules;
using Advobot.Utilities;

using static Advobot.AutoMod.Resources.Responses;
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
			var toggled = enabled ? SpamPreventionVariableEnabled : SpamPreventionVariableDisabled;
			return Success(SpamPreventionAlreadyToggled.Format(
				type.ToString().WithBlock(),
				GetType(type),
				toggled.WithBlock()
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
			var toggled = enabled ? SpamPreventionVariableEnabled : SpamPreventionVariableDisabled;
			return Success(SpamPreventionToggled.Format(
				toggled.WithBlock(),
				type.ToString().WithBlock(),
				GetType(type)
			));
		}

		private static MarkdownFormattedArg GetType(Enum type) => type switch
		{
			RaidType _ => SpamPreventionVariableRaid.WithNoMarkdown(),
			SpamType _ => SpamPreventionVariableSpam.WithNoMarkdown(),
			_ => throw new ArgumentOutOfRangeException(nameof(type)),
		};
	}
}