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
		private static Dictionary<Target, (int Min, int Max, string Name)> _MinsAndMaxesAndErrors = new Dictionary<Target, (int, int, string)>
		{
			{ Target.Guild,        (2, 100,  "guild name") },
			{ Target.Channel,      (2, 100,  "channel name") },
			{ Target.Role,         (1, 100,  "role name") },
			{ Target.Name,         (2, 32,   "username") },
			{ Target.Nickname,     (1, 32,   "nickname") },
			{ Target.Game,         (0, 128,  "game") },        //Yes, I know it CAN go past that, but it won't show for others.
			{ Target.Stream,       (4, 25,   "stream name") }, //Source: https://www.reddit.com/r/Twitch/comments/32w5b2/username_requirements/cqf8yh0/
			{ Target.Topic,        (0, 1024, "channel topic") },
			{ Target.Prefix,       (1, 10,   "bot prefix") },
			{ Target.Regex,        (1, 100,  "regex") },
			{ Target.RuleCategory, (1, 250,  "rule category") },
			{ Target.Rule,         (1, 150,  "rule") },
			{ Target.Category,     (2, 100,  "category") }
		};

		public int Min { get; }
		public int Max { get; }
		public string TooShort { get; }
		public string TooLong { get; }

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
				throw new NotSupportedException($"{target.ToString()} doesn't have a min and max or error output.");
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

			if (value is string str)
			{
				if (str.Length < Min)
				{
					return Task.FromResult(PreconditionResult.FromError(TooShort));
				}

				if (str.Length > Max)
				{
					return Task.FromResult(PreconditionResult.FromError(TooLong));
				}

				return Task.FromResult(PreconditionResult.FromSuccess());
			}

			throw new NotSupportedException($"{nameof(VerifyStringLengthAttribute)} only supports strings.");
		}

		public override string ToString()
		{
			return $"({Min} to {Max} chars)";
		}
	}
}
