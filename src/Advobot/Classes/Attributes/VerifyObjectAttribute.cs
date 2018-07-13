using Advobot.Classes.Results;
using Advobot.Enums;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Verifies the parameter this attribute is targetting fits all of the given conditions.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		private ImmutableList<ObjectVerification> _Checks;
		private bool _IfNullCheckFromContext;

		/// <summary>
		/// Sets the variables saying what checks to use and if to use the values in the context if null.
		/// </summary>
		/// <param name="ifNullCheckFromContext"></param>
		/// <param name="checks"></param>
		public VerifyObjectAttribute(bool ifNullCheckFromContext, params ObjectVerification[] checks)
		{
			_Checks = checks.ToImmutableList();
			_IfNullCheckFromContext = ifNullCheckFromContext;
		}

		/// <summary>
		/// Returns success if the user can do the actions on the supplied object.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{
			if (value != null)
			{
				return Task.FromResult(GetPreconditionResult(context, value));
			}
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (!_IfNullCheckFromContext)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			if (typeof(ITextChannel).IsAssignableFrom(parameter.Type))
			{
				value = context.Channel as ITextChannel;
			}
			else if (typeof(IVoiceChannel).IsAssignableFrom(parameter.Type) && context.User is IGuildUser user)
			{
				value = user.VoiceChannel;
			}
			else if (typeof(IGuildUser).IsAssignableFrom(parameter.Type))
			{
				value = context.User as IGuildUser;
			}
			return Task.FromResult(GetPreconditionResult(context, value));
		}

		private PreconditionResult GetPreconditionResult(ICommandContext context, object value)
		{
			VerifiedObjectResult result;
			switch (value)
			{
				case IEnumerable enumerable:
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
					break;
				case IGuildChannel guildChannel:
					result = guildChannel.Verify(context, _Checks);
					break;
				case IGuildUser guildUser:
					result = guildUser.Verify(context, _Checks);
					break;
				case IRole role:
					result = role.Verify(context, _Checks);
					break;
				default:
					result = new VerifiedObjectResult(value, CommandError.Exception, $"Please notify Advorange of this failure: {nameof(GetPreconditionResult)}");
					break;
			}
			return result.IsSuccess ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result);
		}
	}
}