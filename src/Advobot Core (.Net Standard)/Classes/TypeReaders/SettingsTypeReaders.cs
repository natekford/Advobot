using Advobot.Actions;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Utilizes derived classes' names to determine which settings to get.
	/// </summary>
	internal abstract class SettingTypeReader : TypeReader
	{
		private static Dictionary<string, Dictionary<string, PropertyInfo>> _Settings = new Dictionary<string, Dictionary<string, PropertyInfo>>
		{
			{ nameof(GuildSettingTypeReader), GetActions.GetGuildSettings().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
			{ nameof(BotSettingTypeReader), GetActions.GetBotSettings().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
			{ nameof(BotSettingNonIEnumerableTypeReader), GetActions.GetBotSettingsThatArentIEnumerables().ToDictionary(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase) },
		};

		/// <summary>
		/// Tries to get which settings to use based off of class name, then tries to get the settings via setting name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">If a class name isn't in the settings dictionary.</exception>
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

	internal class GuildSettingTypeReader : SettingTypeReader { }

	internal class BotSettingTypeReader : SettingTypeReader { }

	internal class BotSettingNonIEnumerableTypeReader : SettingTypeReader { }
}
