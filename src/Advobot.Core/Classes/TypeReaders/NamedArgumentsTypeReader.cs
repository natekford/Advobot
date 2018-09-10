using System;
using System.Linq;
using System.Threading.Tasks;
using AdvorangesSettingParser;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Returns custom arguments.
	/// </summary>
	public sealed class NamedArgumentsTypeReader<T> : TypeReader where T : class
	{
		/// <summary>
		/// Creates custom arguments from the given input.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
			=> Task.FromResult(TypeReaderResult.FromSuccess(new NamedArguments<T>(input)));
	}

	//TODO: replace namedarguments with this
	public sealed class ParsableTypeReader<T> : TypeReader where T : IParsable, new()
	{
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			var obj = new T();
			var response = obj.Parser.Parse(input);
			if (response.Errors.Length != 0)
			{
				var str = string.Join("\n", response.Errors);
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"The following are parsing errors:\n{str}."));
			}
			if (response.UnusedParts.Length != 0)
			{
				var str = string.Join("\n", response.UnusedParts);
				return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, $"There following are unused parts:\n{str}."));
			}
			return Task.FromResult(TypeReaderResult.FromSuccess(obj));
		}
	}

	public interface IParsable
	{
		SettingParser Parser { get; }
	}

	public class Mock : IParsable
	{
		public SettingParser Parser { get; }

		public int Dog { get; private set; }
		public string Fish { get; private set; }
		public ulong Cat { get; private set; }

		public Mock()
		{
			Parser = new SettingParser
			{
				new Setting<int>(new[] { nameof(Dog), "d" }, x => Dog = x),
				new Setting<string>(new[] { nameof(Fish), "f" }, x => Fish = x),
				new Setting<ulong>(new[] { nameof(Cat), "c" }, x => Cat = x),
			};
		}
	}
}
