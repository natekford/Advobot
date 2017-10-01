using Advobot.Actions;
using Advobot.Classes.Results;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Verifies the parameter this attribute is targetting fits all of the given conditions. Abstract since <see cref="_GetResultsDict"/> has to be created by a class inheriting this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		private bool _IfNullCheckFromContext;
		private ObjectVerification[] _Checks;

		public VerifyObjectAttribute(bool ifNullCheckFromContext, params ObjectVerification[] checks)
		{
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value == null)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (!_IfNullCheckFromContext)
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}

				if (typeof(ITextChannel).IsAssignableFrom(parameter.Type))
				{
					value = context.Channel as ITextChannel;
				}
				else if (typeof(IVoiceChannel).IsAssignableFrom(parameter.Type))
				{
					value = (context.User as IGuildUser).VoiceChannel;
				}
				else if (typeof(IGuildUser).IsAssignableFrom(parameter.Type))
				{
					value = context.User as IGuildUser;
				}
			}

			if (value is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					var preconditionResult = GetPreconditionResult(context, item);
					//Don't bother testing more if anything is a failure.
					if (!preconditionResult.IsSuccess)
					{
						return Task.FromResult(preconditionResult);
					}
				}
			}
			else
			{
				return Task.FromResult(GetPreconditionResult(context, value));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}

		private PreconditionResult GetPreconditionResult(ICommandContext context, object value)
		{
			VerifiedObjectResult result = default;
			if (value is IGuildChannel guildChannel)
			{
				result = ChannelActions.VerifyChannelMeetsRequirements(context, guildChannel, _Checks);
			}
			else if (value is IGuildUser guildUser)
			{
				result = UserActions.VerifyUserMeetsRequirements(context, guildUser, _Checks);
			}
			else if (value is IRole role)
			{
				result = RoleActions.VerifyRoleMeetsRequirements(context, role, _Checks);
			}
			else
			{
				result = new VerifiedObjectResult(value, CommandError.Exception, $"Please notify Advorange of this failure: {nameof(GetPreconditionResult)}");
			}

			return result.IsSuccess ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result);
		}
	}
}