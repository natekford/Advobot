using Advobot.Classes;
using Advobot.Classes.Results;
using Advobot.Utilities;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Commands.Responses
{
	public sealed class Invites : CommandResponses
	{
		private Invites() { }

		public static AdvobotResult DisplayInvites(IReadOnlyCollection<IInviteMetadata> invites)
		{
			var codeLength = invites.Max(x => x.Code.Length);
			var useLength = invites.Max(x => x.Uses.ToString().Length);

			string FormatInvite(IInviteMetadata i)
				=> $"{i.Code.PadRight(codeLength)} {i.Uses.ToString().PadRight(useLength)} {i.Inviter.Format()}";

			return Success(new EmbedWrapper
			{
				Title = "Invites",
				Description = BigBlock.FormatInterpolated($"{invites.Join("\n", FormatInvite)}"),
			});
		}
		public static AdvobotResult CreatedInvite(IInviteMetadata invite)
			=> Success(Default.FormatInterpolated($"Successfully created {invite}."));
		public static AdvobotResult DeletedInvite(IInviteMetadata invite)
			=> Success(Default.FormatInterpolated($"Successfully deleted {invite}."));
		public static AdvobotResult NoInviteMatches()
			=> Success($"Failed to find any invites matching the given conditions.").WithTime(DefaultTime);
		public static AdvobotResult DeletedMultipleInvites(IReadOnlyCollection<IInviteMetadata> invites)
			=> Success(Default.FormatInterpolated($"Successfully deleted {invites.Count} instant invites."));
	}
}