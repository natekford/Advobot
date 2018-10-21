using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	/// <summary>
	/// Returns a success during runtime.
	/// </summary>
	public sealed class SuccessResult : RuntimeResult, IUniqueResult
	{
		/// <inheritdoc />
		public Guid Guid { get; } = Guid.NewGuid();

		/// <summary>
		/// Creates an instance of <see cref="SuccessResult"/>.
		/// </summary>
		/// <param name="reason"></param>
		public SuccessResult(string reason) : base(null, reason) { }

		/// <summary>
		/// Converts the result into a task returning the result.
		/// </summary>
		/// <param name="result"></param>
		public static implicit operator Task<RuntimeResult>(SuccessResult result)
			=> Task.FromResult<RuntimeResult>(result);
	}
}