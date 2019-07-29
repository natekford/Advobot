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
	/// Attempts to find an <see cref="IBan"/> on a guild.
	/// </summary>
	[TypeReaderTargetType(typeof(IBan))]
	public sealed class BanTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any bans matching the input. Input is tested as a user id, username and discriminator, and finally solely the username.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var bans = await context.Guild.GetBansAsync().CAF();
			if (MentionUtils.TryParseUser(input, out var id) || ulong.TryParse(input, out id))
			{
				var ban = bans.FirstOrDefault(x => x.User.Id == id);
				if (ban != null)
				{
					return TypeReaderResult.FromSuccess(ban);
				}
			}

			var parts = input.Split(new[] { '#' }, 2);
			if (parts.Length == 2 && ushort.TryParse(parts[1], out var d))
			{
				var ban = bans.FirstOrDefault(x => x.User.DiscriminatorValue == d && x.User.Username.CaseInsEquals(parts[0]));
				if (ban != null)
				{
					return TypeReaderResult.FromSuccess(ban);
				}
			}

			var matchingBans = bans.Where(x => x.User.Username.CaseInsEquals(input)).ToArray();
			if (matchingBans.Length == 1)
			{
				return TypeReaderResult.FromSuccess(matchingBans[0]);
			}
			if (matchingBans.Length > 1)
			{
				return TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many bans found with the same username.");
			}
			return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Ban not found.");
		}
	}
}