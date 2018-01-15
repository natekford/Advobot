using Advobot.Core.Utilities;
using Advobot.Core.Enums;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Attributes
{
	/// <summary>
	/// Certain objects in Discord have minimum and maximum lengths for the names that can be set for them. This attribute verifies those lengths and provides errors stating the min/max if under/over.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class VerifyStringLengthAttribute : ParameterPreconditionAttribute
	{
		private static readonly Dictionary<Target, (int Min, int Max, string Name)> _MinsAndMaxesAndErrors = new Dictionary<Target, (int, int, string)>
		{
			{ Target.Guild, (Constants.MIN_GUILD_NAME_LENGTH, Constants.MAX_GUILD_NAME_LENGTH, "guild name") },
			{ Target.Channel, (Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH, "channel name") },
			{ Target.Role, (Constants.MIN_ROLE_NAME_LENGTH, Constants.MAX_ROLE_NAME_LENGTH, "role name") },
			{ Target.Name, (Constants.MIN_USERNAME_LENGTH, Constants.MAX_USERNAME_LENGTH, "username") },
			{ Target.Nickname, (Constants.MIN_NICKNAME_LENGTH, Constants.MAX_NICKNAME_LENGTH, "nickname") },
			{ Target.Game, (Constants.MIN_GAME_LENGTH, Constants.MAX_GAME_LENGTH, "game") },
			{ Target.Stream, (Constants.MIN_STREAM_LENGTH, Constants.MAX_STREAM_LENGTH, "stream name") },
			{ Target.Topic, (Constants.MIN_TOPIC_LENGTH, Constants.MAX_TOPIC_LENGTH, "channel topic") },
			{ Target.Prefix, (Constants.MIN_PREFIX_LENGTH, Constants.MAX_PREFIX_LENGTH, "bot prefix") },
			{ Target.Regex, (Constants.MIN_REGEX_LENGTH, Constants.MAX_REGEX_LENGTH, "regex") },
			{ Target.RuleCategory, (Constants.MIN_RULE_CATEGORY_LENGTH, Constants.MAX_RULE_CATEGORY_LENGTH, "rule category") },
			{ Target.Rule, (Constants.MIN_RULE_LENGTH, Constants.MAX_RULE_LENGTH, "rule") },
			{ Target.Category, (Constants.MIN_CHANNEL_NAME_LENGTH, Constants.MAX_CHANNEL_NAME_LENGTH, "category") },
		};

		public readonly int Min;
		public readonly int Max;
		public readonly string TooShort;
		public readonly string TooLong;

		/// <summary>
		/// Sets the values by looking up <paramref name="target"/> in a dictionary.
		/// </summary>
		/// <param name="target"></param>
		/// <exception cref="NotSupportedException">Only supports some of the values in <see cref="Target"></see>/></exception>
		public VerifyStringLengthAttribute(Target target)
		{
			if (_MinsAndMaxesAndErrors.TryGetValue(target, out var minAndMaxAndError))
			{
				TooShort = $"A {minAndMaxAndError.Name} must be at least `{(Min = minAndMaxAndError.Min)}` characters long.";
				TooLong = $"A {minAndMaxAndError.Name} must be at most `{(Max = minAndMaxAndError.Max)}` characters long.";
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
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services)
		{
			//Getting to this point means the OptionalAttribute has already been checked, so it's ok to just return success on null
			if (value == null)
			{
				return Task.FromResult(PreconditionResult.FromSuccess());
			}
			else if (value is string str)
			{
				if (str.Length < Min)
				{
					return Task.FromResult(PreconditionResult.FromError(TooShort));
				}
				else if (str.Length > Max)
				{
					return Task.FromResult(PreconditionResult.FromError(TooLong));
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

		public override string ToString()
		{
			return $"({Min} to {Max} chars)";
		}
	}
}
