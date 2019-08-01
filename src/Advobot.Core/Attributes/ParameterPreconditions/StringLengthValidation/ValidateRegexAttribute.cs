using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Attributes.ParameterPreconditions.StringLengthValidation
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
		public override async Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			string value,
			IServiceProvider services)
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

			var tests = new (string Name, Func<string, bool> Test)[]
			{
				("empty", x => RegexUtils.IsMatch("", x)),
				("space", x => RegexUtils.IsMatch(" ", x)),
				("new line", x =>  RegexUtils.IsMatch(Environment.NewLine, x)),
				("random", x =>
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
						if (RegexUtils.IsMatch(p.ToString(), x))
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
				if (Test.Invoke(value))
				{
					return PreconditionResult.FromError($"Invalid regex; matched {Name} when it should not have.");
				}
			}
			return PreconditionResult.FromSuccess();
		}
	}
}
