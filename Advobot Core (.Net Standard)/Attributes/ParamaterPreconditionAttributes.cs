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
	/// Verifies the parameter this attribute is targetting fits all of the given conditions. Abstract since <see cref="_GetResultsDict"/> has to be created by a class inheriting this.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal abstract class VerifyObjectAttribute : ParameterPreconditionAttribute
	{
		protected Dictionary<Type, Func<ICommandContext, object, FailureReason>> _GetResultsDict;
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
			return (result != FailureReason.NotFailure) ? PreconditionResult.FromError(FormattingActions.FormatErrorString(context.Guild, result, value)) : PreconditionResult.FromSuccess();
		}
	}

	/// <summary>
	/// Uses <see cref="ChannelVerification"/> to verify certain aspects of a channel. Only works on <see cref="ITextChannel"/>, 
	/// <see cref="IVoiceChannel"/>, and <see cref="IGuildChannel"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class VerifyChannelAttribute : VerifyObjectAttribute
	{
		protected ChannelVerification[] _Checks;

		public VerifyChannelAttribute(bool ifNullCheckFromContext, params ChannelVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, FailureReason>>
			{
				{ typeof(ITextChannel), ITextChannelResult },
				{ typeof(IVoiceChannel), IVoiceChannelResult },
				{ typeof(IGuildChannel), IGuildChannelResult },
			};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private FailureReason ITextChannelResult(ICommandContext context, object value)
		{
			return ChannelActions.VerifyChannelMeetsRequirements(context, (value ?? context.Channel) as IGuildChannel, _Checks);
		}
		private FailureReason IVoiceChannelResult(ICommandContext context, object value)
		{
			return ChannelActions.VerifyChannelMeetsRequirements(context, (value ?? (context.User as IGuildUser).VoiceChannel) as IGuildChannel, _Checks);
		}
		private FailureReason IGuildChannelResult(ICommandContext context, object value)
		{
			return ChannelActions.VerifyChannelMeetsRequirements(context, value as IGuildChannel, _Checks);
		}
	}

	/// <summary>
	/// Uses <see cref="UserVerification"/> to verify certain aspects of a user. Only works on <see cref="IGuildUser"/> and <see cref="IUser"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class VerifyUserAttribute : VerifyObjectAttribute
	{
		protected UserVerification[] _Checks;

		public VerifyUserAttribute(bool ifNullCheckFromContext, params UserVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, FailureReason>>
			{
				{ typeof(IGuildUser), IGuildUserResult },
				{ typeof(IUser), IUserResult },
			};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private FailureReason IGuildUserResult(ICommandContext context, object value)
		{
			return UserActions.VerifyUserMeetsRequirements(context, (value ?? context.User) as IGuildUser, _Checks);
		}
		private FailureReason IUserResult(ICommandContext context, object value)
		{
			//If user cannot be cast as an IGuildUser then they're not on the guild and thus anything can be used on them
			return (value as IGuildUser != null) ? IGuildUserResult(context, value) : FailureReason.NotFailure;
		}
	}

	/// <summary>
	/// Uses <see cref="RoleVerification"/> to verify certain aspects of a role. Only works on <see cref="IRole"/>.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal class VerifyRoleAttribute : VerifyObjectAttribute
	{
		protected RoleVerification[] _Checks;

		public VerifyRoleAttribute(bool ifNullCheckFromContext, params RoleVerification[] checks)
		{
			_GetResultsDict = new Dictionary<Type, Func<ICommandContext, object, FailureReason>>
			{
				{ typeof(IRole), IRoleResult },
			};
			_IfNullCheckFromContext = ifNullCheckFromContext;
			_Checks = checks;
		}

		private FailureReason IRoleResult(ICommandContext context, object value)
		{
			return RoleActions.VerifyRoleMeetsRequirements(context, _Checks, value as IRole);
		}
	}

	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
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

		/// <summary>
		/// Sets the values by looking up <paramref name="target"/> in a dictionary.
		/// </summary>
		/// <param name="target"></param>
		/// <exception cref="NotSupportedException">Only supports some of the values in <see cref="Target"></see>/></exception>
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

		/// <summary>
		/// Checks against the min and max.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="parameter"></param>
		/// <param name="value"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		/// <exception cref="NotSupportedException">This class only works on strings.</exception>
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
