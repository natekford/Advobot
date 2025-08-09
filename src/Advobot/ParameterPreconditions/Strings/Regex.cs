using Advobot.Utilities;

using Discord;
using Discord.Commands;

using System.Text;
using System.Text.RegularExpressions;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates a regex with various test cases.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Regex : StringLengthParameterPrecondition
{
	/// <inheritdoc />
	public override string StringType => "regex";

	/// <summary>
	/// Creates an instance of <see cref="Regex"/>.
	/// </summary>
	public Regex() : base(1, 100) { }

	/// <inheritdoc />
	protected override async Task<PreconditionResult> CheckPermissionsAsync(
		ICommandContext context,
		ParameterInfo parameter,
		IGuildUser invoker,
		string value,
		IServiceProvider services)
	{
		var result = await base.CheckPermissionsAsync(context, parameter, invoker, value, services).ConfigureAwait(false);
		if (!result.IsSuccess)
		{
			return result;
		}

		System.Text.RegularExpressions.Regex regex;
		try
		{
			regex = new(value);
		}
		catch (ArgumentException)
		{
			return PreconditionResult.FromError("Invalid regex provided.");
		}

		var tests = new (string Name, Func<string, bool> Test)[]
		{
			("empty", x => IsMatch("", x)),
			("space", x => IsMatch(" ", x)),
			("new line", x =>  IsMatch(Environment.NewLine, x)),
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
					if (IsMatch(p.ToString(), x))
					{
						++randomMatchCount;
					}
				}
				return randomMatchCount >= 5;
			}),
		};

		foreach (var (Name, Test) in tests)
		{
			if (Test.Invoke(value))
			{
				return PreconditionResult.FromError($"Invalid regex; matched {Name} when it should not have.");
			}
		}
		return this.FromSuccess();
	}

	private bool IsMatch(string input, string pattern)
	{
		try
		{
			return System.Text.RegularExpressions.Regex.IsMatch(
				input: input,
				pattern: pattern,
				options: RegexOptions.None,
				matchTimeout: TimeSpan.FromSeconds(.1)
			);
		}
		catch (RegexMatchTimeoutException)
		{
			return false;
		}
	}
}