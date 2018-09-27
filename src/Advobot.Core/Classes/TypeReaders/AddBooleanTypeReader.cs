using System;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to return a value used for modifying a list.
	/// </summary>
	[TypeReaderTargetType(typeof(AddBoolean))]
	public sealed class AddBooleanTypeReader : TypeReader
	{
		/// <summary>
		/// Returns an <see cref="AddBoolean"/> if a valid word was supplied, otherwise returns an error.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return AddBoolean.TryCreate(input, out var value)
				? Task.FromResult(TypeReaderResult.FromSuccess(value))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Invalid action supplied for modifying a list: `{input}`."));
		}
	}
}
