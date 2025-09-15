using Advobot.Modules;

using System.Text;
using System.Text.RegularExpressions;

using YACCS.Preconditions;
using YACCS.Results;

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
	public override async ValueTask<IResult> CheckAsync(
		CommandMeta meta,
		IGuildContext context,
		string? value)
	{
		var result = await base.CheckAsync(meta, context, value).ConfigureAwait(false);
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
			// TODO: singleton
			return Result.Failure("Invalid regex provided.");
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
				// TODO: singleton?
				var error = $"Invalid regex; matched {Name} when it should not have.";
				return Result.Failure(error);
			}
		}
		return CachedResults.Success;
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