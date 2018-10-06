using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// A type reader with a specified context type.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class TypeReader<T> : TypeReader where T : ICommandContext
	{
		/// <summary>
		/// Whether to throw an exception when the context is not the correc type. If this is set to false an error response will be sent instead.
		/// </summary>
		public bool ThrowException { get; }

		/// <summary>
		/// Creates an instance of <see cref="TypeReader{T}"/>.
		/// </summary>
		/// <param name="throwException"></param>
		public TypeReader(bool throwException = true)
		{
			ThrowException = throwException;
		}

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
		{
			if (context is T correctType)
			{
				return ReadAsync(correctType, input, services);
			}

			var message = $"Invalid context. Expected {typeof(T).Name}.";
			return ThrowException
				? throw new ArgumentException(message, nameof(context))
				: Task.FromResult(TypeReaderResult.FromError(CommandError.Exception, message));
		}
		/// <summary>
		/// Acts as <see cref="ReadAsync(ICommandContext, string, IServiceProvider)"/> except with a casted command context.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<TypeReaderResult> ReadAsync(T context, string input, IServiceProvider services);
	}
}