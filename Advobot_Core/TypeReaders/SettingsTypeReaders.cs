using Advobot.Actions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.TypeReaders
{
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
