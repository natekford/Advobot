using System;
using System.Threading.Tasks;
using Advobot.Core.Classes.NamedArguments;
using Discord.Commands;

namespace Advobot.Core.Classes.TypeReaders
{
	/// <summary>
	/// Returns custom arguments.
	/// </summary>
	public sealed class NamedArgumentsTypeReader<T> : TypeReader where T : new()
	{
		/// <summary>
		/// Creates custom arguments from the given input.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			return Task.FromResult(TypeReaderResult.FromSuccess(new NamedArguments<T>(input)));
		}
	}
}
