using Advobot.Classes.Results;
using Advobot.Enums;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

				var intefaces = parameter.Type.GetInterfaces();
				if (intefaces.Any(x => x == typeof(ITextChannel)))
				{
					value = context.Channel as ITextChannel;
				}
				else if (intefaces.Any(x => x == typeof(IVoiceChannel)))
				{
					value = (context.User as IGuildUser).VoiceChannel;
				}
				else if (intefaces.Any(x => x == typeof(IGuildUser)))
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
			var result = new VerifiedObjectResult(context, value, _Checks);
			return result.IsSuccess ? PreconditionResult.FromSuccess() : PreconditionResult.FromError(result);
		}
	}

	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	internal sealed class VerifyStringLengthAttribute : ParameterPreconditionAttribute
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
