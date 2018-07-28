using Discord.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Limits the rate a command can be used.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RateLimitAttribute : PreconditionAttribute
	{
		private static ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>> _Times = new ConcurrentDictionary<string, ConcurrentDictionary<ulong, DateTime>>();
		private readonly TimeSpan _Time;

		/// <summary>
		/// Creates an instance of <see cref="RateLimitAttribute"/>.
		/// </summary>
		/// <param name="minutes">the amount of time in minutes for the rate limit to last.</param>
		public RateLimitAttribute(int minutes)
		{
			_Time = TimeSpan.FromMinutes(minutes);
		}

		/// <summary>
		/// Checks to make sure that the user can use the command at this time.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="command"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
		{
			var commandDict = _Times.GetOrAdd(command.Aliases[0].Split(' ')[0], new ConcurrentDictionary<ulong, DateTime>());
			if (commandDict.TryGetValue(context.User.Id, out var time) && DateTime.UtcNow < time)
			{
				return Task.FromResult(PreconditionResult.FromError($"Command can be next used at `{time.ToLongTimeString()}`."));
			}
			commandDict[context.User.Id] = DateTime.UtcNow.Add(_Time);
			return Task.FromResult(PreconditionResult.FromSuccess());
		}
	}
}
