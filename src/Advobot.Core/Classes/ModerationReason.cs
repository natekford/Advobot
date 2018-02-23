using Advobot.Core.Utilities;
using System;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Holds a reason and a time for a punishment.
	/// </summary>
	public struct ModerationReason
	{
		public int Time { get; }
		public string Reason { get; }

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
