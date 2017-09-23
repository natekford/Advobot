using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
	/// <summary>
	/// Attemps to find an <see cref="IInvite"/> on a guild.
	/// </summary>
	public class InviteTypeReader : TypeReader
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
			var invite = (await context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code.CaseInsEquals(input));
			return invite != null ? TypeReaderResult.FromSuccess(invite) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
		}
	}

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

			return ban != null ? TypeReaderResult.FromSuccess(ban) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching ban.");
		}
	}

	/// <summary>
	/// Attempts to find an <see cref="Emote"/> on a guild.
	/// </summary>
	public class EmoteTypeReader : TypeReader
	{
		/// <summary>
		/// Checks for any emotes matching the input. Input is tested as an emote Id, then emote name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			IEmote emote = null;
			if (Emote.TryParse(input, out Emote tempEmote))
			{
				emote = tempEmote;
			}
			else if (ulong.TryParse(input, out ulong emoteID))
			{
				emote = context.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
			}

			if (emote == null)
			{
				var emotes = context.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input));
				if (emotes.Count() == 1)
				{
					emote = emotes.First();
				}
				else if (emotes.Count() > 1)
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.MultipleMatches, "Too many emotes have the provided name."));
				}
			}

			return emote != null ? Task.FromResult(TypeReaderResult.FromSuccess(emote)) : Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching emote."));
		}
	}

	/// <summary>
	/// Attemps to create a <see cref="Color"/>.
	/// </summary>
	public class ColorTypeReader : TypeReader
	{
		/// <summary>
		/// Input is tested as a color name, then hex, then RBG separated by back slashes.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
		{
			Color? color = null;
			//By name
			if (Colors.COLORS.TryGetValue(input, out Color temp))
			{
				color = temp;
			}
			//By hex (trimming characters that are sometimes at the beginning of hex numbers)
			else if (uint.TryParse(input.TrimStart(new[] { '&', 'h', '#', '0', 'x' }), System.Globalization.NumberStyles.HexNumber, null, out uint hex))
			{
				color = new Color(hex);
			}
			//By RGB
			else if (input.Contains('/'))
			{
				const byte MAX_VAL = 255;
				var colorRGB = input.Split('/');
				if (colorRGB.Length == 3 && byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
				{
					color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
				}
			}

			return color != null ? Task.FromResult(TypeReaderResult.FromSuccess(color)) : Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching color."));
		}
	}
}
