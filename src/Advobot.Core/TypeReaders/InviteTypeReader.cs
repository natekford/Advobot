using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IInviteMetadata"/> on a guild.
	/// </summary>
	[TypeReaderTargetType(typeof(IInviteMetadata))]
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
				if (invite is IInviteMetadata meta && invite.GuildId == context.Guild.Id)
				{
					return TypeReaderResult.FromSuccess(meta);
				}
				else if (invite != null)
				{
					return TypeReaderResult.FromError(CommandError.Exception, "Found an invite, but it was invalid for what I needed.");
				}
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Invite not found.");
		}
	}
}