using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// Returns an error during runtime.
	/// </summary>
	public sealed class ErrorResult : RuntimeResult, IUniqueResult
	{
		/// <inheritdoc />
		public Guid Guid { get; } = Guid.NewGuid();

		/// <summary>
		/// Creates an instance of <see cref="ErrorResult"/>.
		/// </summary>
		/// <param name="error"></param>
		/// <param name="reason"></param>
		public ErrorResult(CommandError? error, string reason) : base(error, reason) { }
		/// <summary>
		/// Creates an instance of <see cref="ErrorResult"/>.
		/// </summary>
		/// <param name="reason"></param>
		public ErrorResult(string reason) : this(CommandError.Unsuccessful, reason) { }

		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(ErrorResult result)
			=> Task.FromResult<RuntimeResult>(result);
	}
}