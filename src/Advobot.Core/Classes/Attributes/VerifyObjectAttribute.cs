using Advobot.Core.Utilities;
using Advobot.Core.Classes.Results;
using Advobot.Core.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Verifies the parameter this attribute is targetting fits all of the given conditions. Abstract since <see cref="_GetResultsDict"/> has to be created by a class inheriting this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		public ImmutableList<ObjectVerification> Checks { get; }
		public bool IfNullCheckFromContext { get; }

		public VerifyObjectAttribute(bool ifNullCheckFromContext, params ObjectVerification[] checks)
		{
			Checks = checks.ToImmutableList();
			IfNullCheckFromContext = ifNullCheckFromContext;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			if (value == null)
			{
				//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
				if (!IfNullCheckFromContext)
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

			return Task.FromResult(GetPreconditionResult(context, value));
		}

		private PreconditionResult GetPreconditionResult(ICommandContext context, object value)
		{
			VerifiedObjectResult result = default;
			if (value is IEnumerable enumerable)
			{
				foreach (var item in enumerable)
				{
					var preconditionResult = GetPreconditionResult(context, item);
					//Don't bother testing more if anything is a failure.
					if (!preconditionResult.IsSuccess)
					{
						return preconditionResult;
					}
				}
				//If nothing failed then it gets to this point, so return success
				result = new VerifiedObjectResult(value, null, null);
			}
			else if (value is IGuildChannel guildChannel)
			{
				result = guildChannel.Verify(context, Checks);
			}
			else if (value is IGuildUser guildUser)
			{
				result = guildUser.Verify(context, Checks);
			}
			else if (value is IRole role)
			{
				result = role.Verify(context, Checks);
			}
			else
			{
				result = new VerifiedObjectResult(value, CommandError.Exception, $"Please notify Advorange of this failure: {nameof(GetPreconditionResult)}");
			}

			return result.IsSuccess ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result);
		}
	}
}