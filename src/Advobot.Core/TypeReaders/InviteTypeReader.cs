using System;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Attributes;
using Advobot.Utilities;
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
			{
				var invites = await context.Guild.GetInvitesAsync().CAF();
				var invite = invites.FirstOrDefault(x => x.Code.CaseInsEquals(input));
				if (invite != null)
				{
					return TypeReaderResult.FromSuccess(invite);
				}
			}

			{
				var invite = await context.Client.GetInviteAsync(input).CAF();
				//TODO: put the invite.GuildId == context.Guild.Id into parameter precon?
				if (invite is IInviteMetadata meta && invite.GuildId == context.Guild.Id)
				{
					return TypeReaderResult.FromSuccess(meta);
				}
			}

			return TypeReaderUtils.SingleValidResult(Array.Empty<IInviteMetadata>(), "invites", input);
		}
	}
}