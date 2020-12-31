using System;
using System.Threading;
using System.Threading.Tasks;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.AutoMod.Models
{
	public sealed class AutoModSettings : IReadOnlyAutoModSettings
	{
		public bool CheckDuration => Duration != Timeout.InfiniteTimeSpan;
		public TimeSpan Duration { get; set; } = Timeout.InfiniteTimeSpan;
		public ulong GuildId { get; set; }
		public bool IgnoreAdmins { get; set; } = true;
		public bool IgnoreHigherHierarchy { get; set; } = true;
		public long Ticks
		{
			get => Duration.Ticks;
			set => Duration = new TimeSpan(value);
		}

		public AutoModSettings()
		{
		}

		public AutoModSettings(ulong guildId)
		{
			GuildId = guildId;
		}

		public ValueTask<bool> ShouldScanMessageAsync(IMessage message, TimeSpan ts)
		{
			if (message.Author is not IGuildUser user)
			{
				return new ValueTask<bool>(false);
			}
			else if (IgnoreAdmins && user.GuildPermissions.Administrator)
			{
				return new ValueTask<bool>(false);
			}
			else if (CheckDuration && ts > Duration)
			{
				return new ValueTask<bool>(false);
			}
			else if (!IgnoreHigherHierarchy)
			{
				return new ValueTask<bool>(false);
			}

			static async ValueTask<bool> CheckHierarchyAsync(IGuildUser user)
			{
				var bot = await user.Guild.GetCurrentUserAsync().CAF();
				return bot.CanModify(user);
			}

			return CheckHierarchyAsync(user);
		}
	}
}