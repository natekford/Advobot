using Advobot.Modules;

using System.Text;
using System.Text.RegularExpressions;

using YACCS.Commands.Attributes;
using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.ParameterPreconditions.Strings;

/// <summary>
/// Validates a regex with various test cases.
/// </summary>
[AttributeUsage(AttributeUtils.PARAMETERS, AllowMultiple = false, Inherited = true)]
public sealed class Regex() : StringLengthParameterPrecondition(1, 100)
{
	private static readonly Random _Rng = new();

	/// <inheritdoc />
	public override string StringType => "regex";

	/// <inheritdoc />
	protected override async ValueTask<IResult> CheckNotNullAsync(
		CommandMeta meta,
		IGuildContext context,
		string value)
	{
		var result = await base.CheckNotNullAsync(meta, context, value).ConfigureAwait(false);
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
			return Result.Failure($"`{value}` has invalid regex syntax.");
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
					return Result.Failure($"`{value}` is not a good regex; matched `{Name}`.");
				}
			}
			catch (RegexMatchTimeoutException)
			{
				return Result.Failure($"`{value}` is not a good regex; took longer than 50ms.");
			}
		}
		return Result.EmptySuccess;
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