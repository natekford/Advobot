using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// A result which should only be logged once.
	/// </summary>
	public class UniqueResult : RuntimeResult, IUniqueResult
	{
		/// <inheritdoc />
		public Guid Guid { get; } = Guid.NewGuid();
		/// <inheritdoc />
		public bool AlreadyLogged { get; private set; }

		/// <summary>
		/// Creates an instance of <see cref="UniqueResult"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		protected UniqueResult(CommandError? error, string reason) : base(error, reason) { }

		/// <inheritdoc />
		public void MarkAsLogged()
			=> AlreadyLogged = true;

		/// <summary>
		/// Creates a successful result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static UniqueResult FromSuccess(string reason)
			=> new UniqueResult(null, reason);
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static UniqueResult FromFailure(string reason)
			=> new UniqueResult(CommandError.Unsuccessful, reason);
		/// <summary>
		/// Creates an error result.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static UniqueResult FromFailure(CommandError error, string reason)
			=> new UniqueResult(error, reason);
		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(UniqueResult result)
			=> Task.FromResult<RuntimeResult>(result);
	}
}