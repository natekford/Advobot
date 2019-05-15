using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
{
	/// <summary>
	/// A prevention of some type which is timed.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class TimedPrev<T> : IGuildFormattable where T : struct, Enum
	{
		/// <summary>
		/// The type of thing this is preventing.
		/// </summary>
		[JsonProperty]
		public T Type { get; set; }
		/// <summary>
		/// The punishment to give raiders.
		/// </summary>
		[JsonProperty]
		public Punishment Punishment { get; set; }
		/// <summary>
		/// How long the prevention should look at.
		/// </summary>
		[JsonProperty]
		public TimeSpan TimeInterval { get; set; }
		/// <summary>
		/// The role to give as a punishment.
		/// </summary>
		[JsonProperty("Role")]
		public ulong? RoleId { get; set; }
		/// <summary>
		/// Whether or not this raid prevention is enabled.
		/// </summary>
		[JsonIgnore]
		public bool Enabled { get; protected set; }

		/// <summary>
		/// Counts how many times something has occurred within a given timeframe.
		/// Also modifies the queue by removing instances which are too old to matter (locks the source when doing so).
		/// Returns the listlength if seconds is less than 0 or the listlength is less than 2.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="time"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">When <paramref name="source"/> is not in order.</exception>
		protected static int CountItemsInTimeFrame(IEnumerable<ulong> source, TimeSpan? time)
		{
			ulong[] copy;
			lock (source)
			{
				copy = source.ToArray();
			}

			//No timeFrame given means that it's a timed prevention that doesn't check against time
			if (time == null || copy.Length < 2)
			{
				return copy.Length;
			}

			//If there is a timeFrame then that means to gather the highest amount of messages that are in the time frame
			var maxCount = 0;
			for (var i = 0; i < copy.Length; ++i)
			{
				//If the queue is out of order that kinda ruins the method
				if (i > 0 && copy[i - 1] > copy[i])
				{
					throw new ArgumentException("The queue must be in order from oldest to newest.", nameof(source));
				}

				var currentIterCount = 1;
				var iTime = SnowflakeUtils.FromSnowflake(copy[i]).UtcDateTime;
				for (var j = i + 1; j < copy.Length; ++j)
				{
					var jTime = SnowflakeUtils.FromSnowflake(copy[j]).UtcDateTime;
					if ((jTime - iTime) < time)
					{
						++currentIterCount;
						continue;
					}
					//Optimization by checking if the time difference between two numbers is too high to bother starting at j - 1
					var jMinOneTime = SnowflakeUtils.FromSnowflake(copy[j - 1]).UtcDateTime;
					if ((jTime - jMinOneTime) > time)
					{
						i = j + 1;
					}
					break;
				}
				maxCount = Math.Max(maxCount, currentIterCount);
			}

			return maxCount;
		}
		/// <summary>
		/// Enables this timed prevention.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public abstract Task EnableAsync(IGuild guild);
		/// <summary>
		/// Disables this timed prevention.
		/// </summary>
		/// <param name="guild"></param>
		/// <returns></returns>
		public abstract Task DisableAsync(IGuild guild);
		/// <inheritdoc />
		public abstract string Format(SocketGuild? guild = null);
		/// <inheritdoc />
		public override string ToString()
			=> Format();
	}
}