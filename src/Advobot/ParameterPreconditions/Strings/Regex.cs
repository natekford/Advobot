using Advobot.Modules;

using System.Reactive.Joins;
using System.Text;
using System.Text.RegularExpressions;

using YACCS.Interactivity.Input;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates a regex with various test cases.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public sealed class Regex : StringLengthParameterPrecondition
{
	private static readonly Random _Rng = new();

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
			regex = new(value!, RegexOptions.None, TimeSpan.FromMilliseconds(50));
		}
		catch (ArgumentException)
		{
			// TODO: singleton
			return Result.Failure("Invalid regex provided.");
		}

		var tests = new (string Name, string Value)[]
		{
			("empty", ""),
			("space", " "),
			("new line", Environment.NewLine),
			("random", GenerateRandomString()),
		};

		foreach (var (Name, Value) in tests)
		{
			try
			{
				if (regex.IsMatch(Value))
				{
					// TODO: singleton?
					var error = $"Invalid regex; matched {Name} when it should not have.";
					return Result.Failure(error);
				}
			}
			catch (RegexMatchTimeoutException)
			{
				// TODO: singleton?
				var error = "Invalid regex; took longer than 50ms.";
				return Result.Failure(error);
			}
		}
		return CachedResults.Success;
	}

	private static string GenerateRandomString()
	{
		var sb = new StringBuilder();
		for (var i = 0; i < _Rng.Next(50, 100); ++i)
		{
			sb.Append((char)_Rng.Next(1, 10000));
		}
		return sb.ToString();
	}
}