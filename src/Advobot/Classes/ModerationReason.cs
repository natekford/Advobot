using AdvorangesUtils;
using System;
using System.Linq;
using System.Text;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a reason and a time for a punishment.
	/// </summary>
	public struct ModerationReason
	{
		/// <summary>
		/// The time in minutes to give for a punishment.
		/// </summary>
		public int Time { get; }
		/// <summary>
		/// The reason for a punishment.
		/// </summary>
		public string Reason { get; }

		/// <summary>
		/// Parses the time and reason for the punishment.
		/// </summary>
		/// <param name="input"></param>
		public ModerationReason(string input)
		{
			if (input == null)
			{
				Time = -1;
				Reason = null;
				return;
			}

			Time = -1;
			var sb = new StringBuilder();
			foreach (var part in input.Split(' '))
			{
				if (!part.CaseInsStartsWith("time:"))
				{
					sb.Append(part + " ");
				}
				else if (uint.TryParse(part.Split(':').Last(), out var time))
				{
					Time = (int)Math.Min(time, 60 * 24 * 7);
				}
			}
			Reason = sb.ToString();
		}
	}
}
