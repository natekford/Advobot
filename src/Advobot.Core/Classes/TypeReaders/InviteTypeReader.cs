using System;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to find an <see cref="IInvite"/> on a guild.
	/// </summary>
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
			IInvite invite = (await context.Guild.GetInvitesAsync().CAF()).FirstOrDefault(x => x.Code.CaseInsEquals(input));
			if (invite == null && await context.Client.GetInviteAsync(input).CAF() is IInvite inv && inv.GuildId == context.Guild.Id)
			{
				invite = inv;
			}
			return invite != null
				? TypeReaderResult.FromSuccess(invite)
				: TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
		}
	}
}