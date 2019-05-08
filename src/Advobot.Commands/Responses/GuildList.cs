using Advobot.Classes.Results;
using Advobot.Interfaces;
using Advobot.Utilities;
using Discord;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Commands.Responses
{
	public sealed class GuildList : CommandResponses
	{
		private const int _GLength = 25;
		private const int _ULength = 35;
		private const int _MLength = 14;

		private static readonly string _GHeader = "Guild Name".PadRight(_GLength);
		private static readonly string _UHeader = "Url".PadRight(_ULength);
		private static readonly string _MHeader = "Member Count".PadRight(_MLength);
		private static readonly string _EHeader = "Global Emotes";
		private static readonly string _Header = _GHeader + _UHeader + _MHeader + _EHeader;

		private GuildList() { }

		public static AdvobotResult CreatedListing(IInviteMetadata invite, IReadOnlyCollection<string> keywords)
			=> Success(Default.FormatInterpolated($"Successfully created a listed invite from {invite} with the keywords {keywords}."));
		public static AdvobotResult DeletedListing()
			=> Success("Successfully deleted the listed invite.");
		public static AdvobotResult NoInviteToBump()
			=> Failure("Failed to bump the listed invite; there is no listed invite.").WithTime(DefaultTime);
		public static AdvobotResult LastBumpTooRecent()
			=> Failure("Failed to bump the listed invite; the last bump was too recent.").WithTime(DefaultTime);
		public static AdvobotResult Bumped()
			=> Success("Successfully bumped the listed invite.");
		public static AdvobotResult NoInviteMatch()
			=> Failure("Failed to find an invite with the supplied options.").WithTime(DefaultTime);
		public static AdvobotResult InviteMatches(IReadOnlyCollection<IListedInvite> invites)
		{
			var formatted = invites.Join("\n", x =>
			{
				var n = x.GuildName.PadRight(_GLength).Substring(0, _GLength);
				var u = x.Url.PadRight(_ULength);
				var m = x.GuildMemberCount.ToString().PadRight(_MLength);
				var e = x.HasGlobalEmotes ? "Yes" : "";
				return $"{n}{u}{m}{e}";
			});
			var str = $"{_Header}\n{formatted}";
			return Success(BigBlock.FormatInterpolated($"{str}"));
		}
		public static AdvobotResult TooManyMatches()
			=> Failure("Failed to find a suitable invite; too many were found.").WithTime(DefaultTime);
	}
}
