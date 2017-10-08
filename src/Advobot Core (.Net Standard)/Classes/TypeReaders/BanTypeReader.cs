using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attemps to find an <see cref="IBan"/> on a guild.
	/// </summary>
	public class BanTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any bans matching the input. Input is tested as a user Id, username and discriminator, and finally solely the username.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			IBan ban = null;
			var bans = await context.Guild.GetBansAsync();
			if (MentionUtils.TryParseUser(input, out ulong userID))
			{
				ban = bans.FirstOrDefault(x => x.User.Id == userID);
			}
			else if (ulong.TryParse(input, out userID))
			{
				ban = bans.FirstOrDefault(x => x.User.Id == userID);
			}
			else if (input.Contains('#'))
			{
				var usernameAndDiscriminator = input.Split('#');
				if (usernameAndDiscriminator.Length == 2 && ushort.TryParse(usernameAndDiscriminator[1], out ushort discriminator))
				{
					ban = bans.FirstOrDefault(x => x.User.DiscriminatorValue == discriminator && x.User.Username.CaseInsEquals(usernameAndDiscriminator[0]));
				}
			}

			if (ban == null)
			{
				var matchingUsernames = bans.Where(x => x.User.Username.CaseInsEquals(input));
				if (matchingUsernames.Count() == 1)
				{
					ban = matchingUsernames.FirstOrDefault();
				}
				else if (matchingUsernames.Count() > 1)
				{
					return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many bans found with the same username.");
				}
			}

			return ban != null
				? TypeReaderResult.FromSuccess(ban)
				: TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching ban.");
		}
	}
}