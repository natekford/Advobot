using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IInvite"/> on a guild.
	/// </summary>
	[TypeReaderTargetType(typeof(IInvite))]
	public sealed class InviteTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any invites matching the input code.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var invites = await context.Guild.GetInvitesAsync().CAF();
			{
				var invite = invites.FirstOrDefault(x => x.Code.CaseInsEquals(input));
				if (invite != null)
				{
					return TypeReaderResult.FromSuccess(invite);
				}
			}

			{
				var invite = await context.Client.GetInviteAsync(input).CAF();
				if (invite != null && invite.GuildId == context.Guild.Id)
				{
					return TypeReaderResult.FromSuccess(invite);
				}
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Invite not found.");
		}
	}
}