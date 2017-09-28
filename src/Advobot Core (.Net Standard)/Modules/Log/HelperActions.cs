using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Classes;
using Advobot.Enums;
using Advobot.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Advobot.Modules.Log
{
	internal static class HelperFunctions
	{
		public static async Task HandleJoiningUsersForRaidPrevention(ITimersModule timers, IGuildSettings guildSettings, IGuildUser user)
		{
			var antiRaid = guildSettings.RaidPreventionDictionary[RaidType.Regular];
			if (antiRaid != null && antiRaid.Enabled)
			{
				await antiRaid.RaidPreventionPunishment(guildSettings, user);
			}
			var antiJoin = guildSettings.RaidPreventionDictionary[RaidType.RapidJoins];
			if (antiJoin != null && antiJoin.Enabled)
			{
				antiJoin.Add(user.JoinedAt.Value.UtcDateTime);
				if (antiJoin.GetSpamCount() < antiJoin.UserCount)
				{
					return;
				}

				await antiJoin.RaidPreventionPunishment(guildSettings, user);
				if (guildSettings.ServerLog == null)
				{
					return;
				}

				await MessageActions.SendEmbedMessage(guildSettings.ServerLog, EmbedActions.MakeNewEmbed("Anti Rapid Join Mute", $"**User:** {user.FormatUser()}"));
			}
		}
	}
}
