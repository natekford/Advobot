using System.Threading.Tasks;

using Advobot.Logging.Service;

using Discord;

namespace Advobot.Logging.Context.Users
{
	public class UserUpdatedState : UserState
	{
		public IUser Before { get; }

		public UserUpdatedState(IUser before, IGuildUser user)
			: base(user)
		{
			Before = before;
		}

		public override Task<bool> CanLog(ILoggingService service, ILoggingContext context)
			=> Task.FromResult(!(User.IsBot || User.IsWebhook));
	}
}