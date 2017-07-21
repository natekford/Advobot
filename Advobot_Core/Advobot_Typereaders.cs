using Advobot.Actions;
using Advobot.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Collections.Specialized;
using System.Reflection;

namespace Advobot
{
	namespace TypeReaders
	{
		public abstract class MyTypeReader : TypeReader
		{
			public bool TryParseMyCommandContext(ICommandContext context, out MyCommandContext myContext)
			{
				return (myContext = context as MyCommandContext) != null;
			}
		}

		public class IInviteTypeReader : MyTypeReader
		{
			public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
				{
					return TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided.");
				}

				var invites = await myContext.Guild.GetInvitesAsync();
				var invite = invites.FirstOrDefault(x => x.Code.CaseInsEquals(input));
				return invite != null ? TypeReaderResult.FromSuccess(invite) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
			}
		}

		public class IBanTypeReader : MyTypeReader
		{
			public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
				{
					return TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided.");
				}

				IBan ban = null;
				var bans = await myContext.Guild.GetBansAsync();
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

		public class IEmoteTypeReader : MyTypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided."));
				}

				IEmote emote = null;
				if (Emote.TryParse(input, out Emote tempEmote))
				{
					emote = tempEmote;
				}
				else if (ulong.TryParse(input, out ulong emoteID))
				{
					emote = myContext.Guild.Emotes.FirstOrDefault(x => x.Id == emoteID);
				}

				if (emote == null)
				{
					var emotes = myContext.Guild.Emotes.Where(x => x.Name.CaseInsEquals(input));
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

		public class ColorTypeReader : MyTypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (!TryParseMyCommandContext(context, out MyCommandContext myContext))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.Exception, "Invalid context provided."));
				}

				Color? color = null;
				//By name
				if (Constants.COLORS.TryGetValue(input, out Color temp))
				{
					color = temp;
				}
				//By hex
				else if (uint.TryParse(input.TrimStart(new[] { '&', 'h', '#', '0', 'x' }), System.Globalization.NumberStyles.HexNumber, null, out uint hex))
				{
					color = new Color(hex);
				}
				//By RGB
				else if (input.Contains('/'))
				{
					var colorRGB = input.Split('/');
					if (colorRGB.Length == 3)
					{
						const byte MAX_VAL = 255;
						if (byte.TryParse(colorRGB[0], out byte r) && byte.TryParse(colorRGB[1], out byte g) && byte.TryParse(colorRGB[2], out byte b))
						{
							color = new Color(Math.Min(r, MAX_VAL), Math.Min(g, MAX_VAL), Math.Min(b, MAX_VAL));
						}
					}
				}

				return color != null ? Task.FromResult(TypeReaderResult.FromSuccess(color)) : Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching color."));
			}
		}

		public class BypassUserLimitTypeReader : MyTypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
			}
		}

		public class BoolTypeReader : MyTypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (bool.TryParse(input, out bool output))
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(output));
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Failed to parse a bool."));
				}
			}
		}
	}

}
