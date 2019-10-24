using System.Threading.Tasks;

using Discord;
using Discord.Commands;

namespace Advobot.Criterions
{
	/// <summary>
	/// Determines if a message is from the specified user.
	/// </summary>
	public sealed class EnsureFromUserCriterion : ICriterion<IMessage>
	{
		private readonly ulong _UserId;

		/// <summary>
		/// Creates an instance of <see cref="EnsureFromUserCriterion"/>.
		/// </summary>
		/// <param name="user"></param>
		public EnsureFromUserCriterion(IUser user)
		{
			_UserId = user.Id;
		}

		/// <inheritdoc />
		public Task<bool> JudgeAsync(ICommandContext context, IMessage parameter)
			=> Task.FromResult(_UserId == parameter.Author.Id);
	}
}