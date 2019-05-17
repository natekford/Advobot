using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Type reader for converting the passed in string to a basic type then using that basic type to find an object.
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	/// <typeparam name="TContext"></typeparam>
	public abstract class TypeReader<TBase, TContext> : TypeReader<TContext> where TContext : ICommandContext
	{
		/// <summary>
		/// Converts a string into an object of type <typeparamref name="TBase"/> asynchronously.
		/// </summary>
		public abstract AsyncTryConverter<TBase, TContext> TryConverter { get; }

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(TContext context, string input, IServiceProvider services)
		{
			var (success, value) = await TryConverter.Invoke(context, input, services).CAF();
			if (!success)
			{
				return TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(TBase).Name}.");
			}
			return await ReadAsync(context, value, services).CAF();
		}
		/// <summary>
		/// Finds an object with a converted input.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<TypeReaderResult> ReadAsync(TContext context, TBase input, IServiceProvider services);
	}
}
