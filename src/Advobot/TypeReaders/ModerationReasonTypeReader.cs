using Advobot.Punishments;

using YACCS.TypeReaders;

namespace Advobot.TypeReaders;

/// <summary>
/// Attempts to create a moderation reason with a time from a string.
/// </summary>
[TypeReaderTargetTypes(typeof(ModerationReason))]
public sealed class ModerationReasonTypeReader()
	: TryParseTypeReader<ModerationReason>(TryParse)
{
	private static bool TryParse(string s, out ModerationReason result)
	{
		result = new ModerationReason(s);
		return true;
	}
}