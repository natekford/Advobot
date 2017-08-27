using Advobot.Actions;
using Advobot.Interfaces;
using Advobot.NonSavedClasses;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot
{
	namespace TypeReaders
	{
		public class InviteTypeReader : TypeReader
		{
			public override async Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				var invite = (await context.Guild.GetInvitesAsync()).FirstOrDefault(x => x.Code.CaseInsEquals(input));
				return invite != null ? TypeReaderResult.FromSuccess(invite) : TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a matching invite.");
			}
		}

		public class BanTypeReader : TypeReader
		{
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

		public class EmoteTypeReader : TypeReader
		{
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

		public class ColorTypeReader : TypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				Color? color = null;
				//By name
				if (Constants.COLORS.TryGetValue(input, out Color temp))
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

		public class BypassUserLimitTypeReader : TypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(Constants.BYPASS_STRING.CaseInsEquals(input)));
			}
		}

		public class CommandSwitchTypeReader : TypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (context is MyCommandContext)
				{
					var cont = context as MyCommandContext;
					var command = GetActions.GetCommand(cont.GuildSettings, input);
					if (command != null)
					{
						return Task.FromResult(TypeReaderResult.FromSuccess(command));
					}
				}
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, "Unable to find a command matching the supplied input."));
			}
		}

		public class GuildPermissionsTypeReader : TypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				//Check numbers first
				if (ulong.TryParse(input, out ulong rawValue))
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
				}
				//Then check permission names
				else if (!GetActions.TryGetValidGuildPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
				{
					var failureStr = FormattingActions.ERROR($"Invalid permission{GetActions.GetPlural(invalidPerms.Count())} provided: `{String.Join("`, `", invalidPerms)}`.");
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, failureStr));
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(GuildActions.ConvertGuildPermissionNamesToUlong(validPerms)));
				}
			}
		}

		public class ChannelPermissionsTypeReader : TypeReader
		{
			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				//Check numbers first
				if (ulong.TryParse(input, out ulong rawValue))
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(rawValue));
				}
				//Then check permission names
				else if (!GetActions.TryGetValidChannelPermissionNamesFromInputString(input, out var validPerms, out var invalidPerms))
				{
					var failureStr = FormattingActions.ERROR($"Invalid permission{GetActions.GetPlural(invalidPerms.Count())} provided: `{String.Join("`, `", invalidPerms)}`.");
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, failureStr));
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(ChannelActions.ConvertChannelPermissionNamesToUlong(validPerms)));
				}
			}
		}

		public abstract class SettingTypeReader : TypeReader
		{
			private static Dictionary<string, Dictionary<string, PropertyInfo>> _Settings = new Dictionary<string, Dictionary<string, PropertyInfo>>
			{
				{ nameof(GuildSettingTypeReader), GetActions.GetGuildSettings().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
				{ nameof(BotSettingTypeReader), GetActions.GetBotSettings().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
				{ nameof(BotSettingNonIEnumerableTypeReader), GetActions.GetBotSettingsThatArentIEnumerables().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
			};

			public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
			{
				if (!_Settings.TryGetValue(GetType().Name, out var dict))
				{
					throw new ArgumentException($"{GetType().Name} is not in the settings dictionary.");
				}
				else if (dict.TryGetValue(input, out PropertyInfo value))
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(value));
				}
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid setting for this command."));
			}
		}

		public class GuildSettingTypeReader : SettingTypeReader { }

		public class BotSettingTypeReader : SettingTypeReader { }

		public class BotSettingNonIEnumerableTypeReader : SettingTypeReader { }
	}
}
