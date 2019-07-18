using Advobot.Classes.Modules;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes.ParameterPreconditions.StringLengthValidation
{
	/// <summary>
	/// Validates the regex by making sure it is between 1 and 100 characters.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
	public sealed class ValidateRegexAttribute : ValidateStringLengthAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ValidateRegexAttribute"/>.
		/// </summary>
		public ValidateRegexAttribute() : base(1, 100) { }

		/// <inheritdoc />
		public override async Task<PreconditionResult> CheckPermissionsAsync(AdvobotCommandContext context, ParameterInfo parameter, string value, IServiceProvider services)
		{
			var result = await base.CheckPermissionsAsync(context, parameter, value, services).CAF();
			if (!result.IsSuccess)
			{
				return result;
			}

			Regex regex;
			try
			{
				regex = new Regex(value);
			}
			catch (ArgumentException)
			{
				return PreconditionResult.FromError("Invalid regex provided.");
			}

			var tests = new (string Name, Func<bool> Test)[]
			{
				("empty", () => RegexUtils.IsMatch("", value)),
				("space", () => RegexUtils.IsMatch(" ", value)),
				("new line", () =>  RegexUtils.IsMatch(Environment.NewLine, value)),
				("random", () =>
				{
					var randomMatchCount = 0;
					for (var i = 0; i < 10; ++i)
					{
						var r = new Random();
						var p = new StringBuilder();
						for (var j = 0; j < r.Next(1, 100); ++j)
						{
							p.Append((char)r.Next(1, 10000));
						}
						if (RegexUtils.IsMatch(p.ToString(), value))
						{
							++randomMatchCount;
						}
					}
					return randomMatchCount >= 5;
				}),
			};

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
			foreach (var (Name, Test) in tests)
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
			{
				if (Test())
				{
					return PreconditionResult.FromError($"Invalid regex; matched {Name} when it should not have.");
				}
			}
			return PreconditionResult.FromSuccess();
		}
	}
}
