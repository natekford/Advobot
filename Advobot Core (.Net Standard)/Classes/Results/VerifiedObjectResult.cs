using Advobot.Actions;
using Advobot.Enums;
using Discord;
using Discord.Commands;

namespace Advobot.Classes.Results
{
	public class VerifiedObjectResult : IResult
	{
		public CommandError? Error { get; private set; }
		public string ErrorReason { get; private set; }
		public bool IsSuccess { get; private set; }

		public VerifiedObjectResult(ICommandContext context, object value, ObjectVerification[] checks)
		{
			if (value is IGuildChannel guildChannel)
			{
				IsSuccess = ChannelActions.VerifyChannelMeetsRequirements(context, guildChannel, checks, out var error, out var errorReason);
				Error = error;
				ErrorReason = errorReason;
			}
			else if (value is IGuildUser guildUser)
			{
				IsSuccess = UserActions.VerifyUserMeetsRequirements(context, guildUser, checks, out var error, out var errorReason);
				Error = error;
				ErrorReason = errorReason;
			}
			else if (value is IRole role)
			{
				IsSuccess = RoleActions.VerifyRoleMeetsRequirements(context, role, checks, out var error, out var errorReason);
				Error = error;
				ErrorReason = errorReason;
			}
		}
	}
}
