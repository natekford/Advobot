using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Utilities;
using AdvorangesSettingParser.Implementation;
using AdvorangesSettingParser.Interfaces;
using AdvorangesSettingParser.Results;
using AdvorangesSettingParser.Utils;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Parses arguments with the name supplied into an object.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ParsableTypeReader<T> : TypeReader where T : class, new()
	{
		/// <summary>
		/// Creates an instance of <see cref="ParsableTypeReader{T}"/> and verifies the type can be parsed.
		/// </summary>
		public ParsableTypeReader()
		{
			if (!(StaticSettingParserRegistry.Instance.TryRetrieve<T>(out _) || typeof(IParsable).IsAssignableFrom(typeof(T))))
			{
				throw new ArgumentException($"{typeof(T).Name} does not have a static setting parser registered and is not parsable directly.");
			}
		}

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (!ParseArgs.TryParse(input, out var args))
			{
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "There is a quote mismatch."));
			}

			var obj = new T();
			var parser = StaticSettingParserRegistry.Instance.GetSettingParser(obj);
			var response = parser.Parse(obj, input);
			if (response.Errors.Any())
			{
				var str = string.Join(", ", response.Errors);
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"The following are parsing errors: {str}."));
			}
			if (response.UnusedParts.Any())
			{
				var str = response.UnusedParts.Cast<SetValueResult>().Join(", ", x => $"`{x.Value}`");
				return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, $"The following are unused parts: {str}."));
			}
			var neededSettings = parser.GetNeededSettings(obj).ToList();
			if (neededSettings.Count != 0)
			{
				var str = string.Join("`, `", neededSettings);
				var resp = $"The following parts are required to be set: `{str}`.";
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, resp));
			}
			return Task.FromResult(TypeReaderResult.FromSuccess(obj));
		}
	}
}
