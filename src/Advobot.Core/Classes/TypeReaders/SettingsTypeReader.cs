using Advobot.Core.Interfaces;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Utilizes derived classes' names to determine which settings to get.
	/// </summary>
	public abstract class SettingTypeReader : TypeReader
	{
		private static Dictionary<string, Dictionary<string, FieldInfo>> _Settings = new Dictionary<string, Dictionary<string, FieldInfo>>
		{
			{ nameof(GuildSettingTypeReader), SettingsBase.GetSettings(typeof(IGuildSettings)) },
			{ nameof(BotSettingTypeReader), SettingsBase.GetSettings(typeof(IBotSettings)) },
		};

		/// <summary>
		/// Tries to get which settings to use based off of class name, then tries to get the settings via setting name.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">If a class name isn't in the settings dictionary.</exception>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!_Settings.TryGetValue(GetType().Name, out var dict))
			{
				throw new ArgumentException("not in the settings dictionary", GetType().FullName);
			}
			if (dict.TryGetValue(input, out var value))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(value));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{input}` is not a valid setting for this command."));
		}

		public sealed class GuildSettingTypeReader : SettingTypeReader { }
		public sealed class BotSettingTypeReader : SettingTypeReader { }
	}
}
