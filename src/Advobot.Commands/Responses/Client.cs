using Advobot.Classes.Results;
using Advobot.Utilities;

namespace Advobot.Commands.Responses
{
	public sealed class Client : CommandResponses
	{
		private Client() { }

		public static AdvobotResult ModifiedName(string old, string name)
			=> Success(Default.FormatInterpolated($"Successfully changed the name of {old} to {name}."));
		public static AdvobotResult EnqueuedIcon(int position)
			=> Success(Default.FormatInterpolated($"Successfully queued changing the bot icon at position {position}."));
		public static AdvobotResult RemovedIcon()
			=> Success("Successfully removed the bot icon.");
	}
}
