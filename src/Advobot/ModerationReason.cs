using AdvorangesUtils;

namespace Advobot;

/// <summary>
/// Holds a reason and a time for a punishment.
/// </summary>
public readonly struct ModerationReason
{
	/// <summary>
	/// The reason for a punishment.
	/// </summary>
	public string? Reason { get; }
	/// <summary>
	/// The time in minutes to give for a punishment.
	/// </summary>
	public TimeSpan? Time { get; }

	/// <summary>
	/// Parses the time and reason for the punishment.
	/// </summary>
	/// <param name="input"></param>
	public ModerationReason(string input)
	{
		if (input is null)
		{
			Time = null;
			Reason = null;
			return;
		}

		Time = default;
		foreach (var part in input.Split(' '))
		{
			if (!part.CaseInsStartsWith("time:"))
			{
				continue;
			}
			if (uint.TryParse(part.Split([':'], 2)[^1], out var time))
			{
				Time = TimeSpan.FromMinutes((int)Math.Min(time, 60 * 24 * 7));
			}
		}
		Reason = input.TrimEnd();
	}
}