using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Attemps to find an <see cref="IInvite"/> on a guild.
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
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			IInvite invite = (await context.Guild.GetInvitesAsync().CAF()).FirstOrDefault(x => x.Code.CaseInsEquals(input));
			if (invite == null)
			{
				//Test if vanity url
				var testInv = await context.Client.GetInviteAsync(input).CAF();
				if (testInv.GuildId == context.Guild.Id)
				{
					invite = testInv;
				}
			}
			return invite != null
				? TypeReaderResult.FromSuccess(invite)
				: TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
		}
	}
}