using Advobot.Actions;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Attributes
{
	/// <summary>
	/// Verifies the parameter this attribute is targetting fits all of the given conditions. Abstract since _GetResultsDict has to be created by a class inheriting this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public abstract class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		protected Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>> _GetResultsDict;
		protected bool _IfNullCheckFromContext;

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null && !_IfNullCheckFromContext)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			if (value is System.Collections.IEnumerable)
			{
				foreach (var item in value as System.Collections.IEnumerable)
				{
					//Don't bother trying to go farther if anything is a failure.
					var preconditionResult = GetPreconditionResult(context, item, item.GetType());
					if (!preconditionResult.IsSuccess)
					{
						return Task.FromResult(preconditionResult);
					}
				}
			}
			else
			{
				return Task.FromResult(GetPreconditionResult(context, value, parameter.Type));
			}

			return Task.FromResult(PreconditionResult.FromSuccess());
		}

		protected virtual PreconditionResult GetPreconditionResult(ICommandContext context, object value, Type type)
		{
			//Will provide exceptions if invalid VerifyObject attributes are used on objects. E.G. VerifyChannel used on a role
			var result = _GetResultsDict[type](context, value);
			var failureReason = result.Item1;
			var obj = result.Item2;

			return (failureReason != FailureReason.NotFailure) ? PreconditionResult.FromError(FormattingActions.FormatErrorString(context.Guild, failureReason, obj)) : PreconditionResult.FromSuccess();
		}
	}

	/// <summary>
	/// Uses ChannelVerification enum to verify certain aspects of a channel. Only works on ITextChannel, IVoiceChannel, and IGuildChannel.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyChannelAttribute : VerifyObjectAttribute
	{
		protected ChannelVerification[] _Checks;

		public VerifyChannelAttribute(bool ifNullCheckFromContext, params ChannelVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(ITextChannel), ITextChannelResult },
					{ typeof(IVoiceChannel), IVoiceChannelResult },
					{ typeof(IGuildChannel), IGuildChannelResult },
				};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private Tuple<FailureReason, object> ITextChannelResult(ICommandContext context, object value)
		{
			var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.Channel) as IGuildChannel);
			return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
		}
		private Tuple<FailureReason, object> IVoiceChannelResult(ICommandContext context, object value)
		{
			var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, (value ?? (context.User as IGuildUser).VoiceChannel) as IGuildChannel);
			return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
		}
		private Tuple<FailureReason, object> IGuildChannelResult(ICommandContext context, object value)
		{
			var returned = ChannelActions.GetChannel(context.Guild, context.User as IGuildUser, _Checks, value as IGuildChannel);
			return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
		}
	}

	/// <summary>
	/// Uses UserVerification enum to verify certain aspects of a user. Only works on IGuildUser and IUser.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyUserAttribute : VerifyObjectAttribute
	{
		protected UserVerification[] _Checks;

		public VerifyUserAttribute(bool ifNullCheckFromContext, params UserVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(IGuildUser), IGuildUserResult },
					{ typeof(IUser), IUserResult },
				};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private Tuple<FailureReason, object> IGuildUserResult(ICommandContext context, object value)
		{
			var returned = UserActions.GetGuildUser(context.Guild, context.User as IGuildUser, _Checks, (value ?? context.User) as IGuildUser);
			return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
		}
		private Tuple<FailureReason, object> IUserResult(ICommandContext context, object value)
		{
			//If user cannot be cast as an IGuildUser then they're not on the guild and thus anything can be used on them
			return (value as IGuildUser != null) ? IGuildUserResult(context, value) : Tuple.Create(FailureReason.NotFailure, value);
		}
	}

	/// <summary>
	/// Uses RoleVerification enum to verify certain aspects of a role. Only works on IRole.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyRoleAttribute : VerifyObjectAttribute
	{
		protected RoleVerification[] _Checks;

		public VerifyRoleAttribute(bool ifNullCheckFromContext, params RoleVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, Tuple<FailureReason, object>>>
				{
					{ typeof(IRole), IRoleResult },
				};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private Tuple<FailureReason, object> IRoleResult(ICommandContext context, object value)
		{
			var returned = RoleActions.GetRole(context.Guild, context.User as IGuildUser, _Checks, value as IRole);
			return Tuple.Create<FailureReason, object>(returned.Reason, returned.Object);
		}
	}

	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides error stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public class VerifyStringLengthAttribute : ParameterPreconditionAttribute
	{
		private static readonly Dictionary<Target, Tuple<int, int, string>> _MinsAndMaxesAndErrors = new Dictionary<Target, Tuple<int, int, string>>
			{
				{ Target.Guild,     Tuple.Create(Constants.MIN_GUILD_NAME_LENGTH,   Constants.MAX_GUILD_NAME_LENGTH,    "guild name") },
				{ Target.Channel,   Tuple.Create(Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH,  "channel name") },
				{ Target.Role,      Tuple.Create(Constants.MIN_ROLE_NAME_LENGTH,    Constants.MAX_ROLE_NAME_LENGTH,     "role name") },
				{ Target.Name,      Tuple.Create(Constants.MIN_USERNAME_LENGTH,     Constants.MAX_USERNAME_LENGTH,      "username") },
				{ Target.Nickname,  Tuple.Create(Constants.MIN_NICKNAME_LENGTH,     Constants.MAX_NICKNAME_LENGTH,      "nickname") },
				{ Target.Game,      Tuple.Create(Constants.MIN_GAME_LENGTH,         Constants.MAX_GAME_LENGTH,          "game") },
				{ Target.Stream,    Tuple.Create(Constants.MIN_STREAM_LENGTH,       Constants.MAX_STREAM_LENGTH,        "stream name") },
				{ Target.Topic,     Tuple.Create(Constants.MIN_TOPIC_LENGTH,        Constants.MAX_TOPIC_LENGTH,         "channel topic") },
				{ Target.Prefix,    Tuple.Create(Constants.MIN_PREFIX_LENGTH,       Constants.MAX_PREFIX_LENGTH,        "bot prefix") },
			};
		private int _Min;
		private int _Max;
		private string _TooShort;
		private string _TooLong;

		public VerifyStringLengthAttribute(Target target)
		{
			if (_MinsAndMaxesAndErrors.TryGetValue(target, out var minAndMaxAndError))
			{
				_Min = minAndMaxAndError.Item1;
				_Max = minAndMaxAndError.Item2;
				_TooShort = $"A {minAndMaxAndError.Item3} must be at least `{_Min}` characters long.";
				_TooLong = $"A {minAndMaxAndError.Item3} must be at most `{_Max}` characters long.";
			}
			else
			{
				throw new NotSupportedException($"{target.EnumName()} doesn't have a min and max or error output.");
			}
		}

		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			if (value.GetType() == typeof(string))
			{
				var str = value.ToString();
				if (str.Length < _Min)
				{
					return Task.FromResult(PreconditionResult.FromError(_TooShort));
				}
				else if (str.Length > _Max)
				{
					return Task.FromResult(PreconditionResult.FromError(_TooLong));
				}
				else
				{
					return Task.FromResult(PreconditionResult.FromSuccess());
				}
			}
			else
			{
				throw new NotSupportedException($"{nameof(VerifyStringLengthAttribute)} only supports strings.");
			}
		}
	}
}
