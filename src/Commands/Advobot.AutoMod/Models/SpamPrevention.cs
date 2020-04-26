using System;
using System.Collections.Generic;
using System.Linq;

using Advobot.AutoMod.ReadOnlyModels;
using Advobot.AutoMod.Utils;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using Discord;

namespace Advobot.AutoMod.Models
{
	public sealed class SpamPrevention : Punishment, IReadOnlySpamPrevention
	{
		public bool Enabled { get; set; }
		public TimeSpan Interval { get; set; }
		public int Size { get; set; }
		public SpamType SpamType { get; set; }

		public bool IsSpam(IMessage message)
			=> GetSpamCount(message) > Size;

		public bool ShouldPunish(IEnumerable<ulong> messages)
			=> messages.CountItemsInTimeFrame(Interval) > Instances;

		private int GetSpamCount(IMessage message) => SpamType switch
		{
			SpamType.Message => int.MaxValue,
			SpamType.LongMessage => message.Content?.Length ?? 0,
			SpamType.Link => message.GetLinkCount(),
			SpamType.Image => message.GetImageCount(),
			SpamType.Mention => message.MentionedUserIds.Distinct().Count(),
			_ => throw new ArgumentOutOfRangeException(nameof(SpamType)),
		};
	}
}