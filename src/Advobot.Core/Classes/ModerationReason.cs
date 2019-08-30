using System;
using System.Linq;
using System.Text;

using Advobot.Modules;

using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Holds a reason and a time for a punishment.
	/// </summary>
	public readonly struct ModerationReason
	{
		/// <summary>
		/// Parses the time and reason for the punishment.
		/// </summary>
		/// <param name="input"></param>
		public ModerationReason(string input)
		{
			if (input == null)
			{
				Time = null;
				Reason = null;
				return;
			}

			Time = default;
			var sb = new StringBuilder();
			foreach (var part in input.Split(' '))
			{
				if (!part.CaseInsStartsWith("time:"))
				{
					sb.Append(part).Append(' ');
				}
				else if (uint.TryParse(part.Split(new[] { ':' }, 2).Last(), out var time))
				{
					Time = TimeSpan.FromMinutes((int)Math.Min(time, 60 * 24 * 7));
				}
			}
			Reason = sb.ToString();
		}

		/// <summary>
		/// The reason for a punishment.
		/// </summary>
		public string? Reason { get; }

		/// <summary>
		/// The time in minutes to give for a punishment.
		/// </summary>
		public TimeSpan? Time { get; }

		/// <summary>
		/// Converts this to a <see cref="PunishmentArgs"/>.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public PunishmentArgs ToPunishmentArgs(AdvobotModuleBase module)
		{
			var args = new PunishmentArgs();
			if (Time != null && module.Timers != null)
			{
				args = new PunishmentArgs(module.Timers, Time.Value);
			}
			args.Options = module.GenerateRequestOptions(Reason);
			return args;
		}
	}
}