using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.Punishments;
using Advobot.Services.Time;

using Discord;
using Discord.Net;

using Microsoft.Extensions.Logging;

using System.Net;

namespace Advobot.AutoMod.Service;

public sealed class TimedPunishmentService(
	ILogger<TimedPunishmentService> logger,
	ITimedPunishmentDatabase db,
	IDiscordClient client,
	ITimeService time
) : IPunishmentService
{
	public async Task HandleAsync(IPunishmentContext context)
	{
		TimedPunishment ToDbModel(IPunishmentContext context)
		{
			return new()
			{
				GuildId = context.Guild.Id,
				UserId = context.UserId,
				RoleId = context.Role?.Id ?? 0,
				PunishmentType = context.Type,
				EndTimeTicks = (time.UtcNow + context.Time)?.Ticks ?? -1
			};
		}

		await context.ExecuteAsync().ConfigureAwait(false);
		if (context.IsGive && context.Time.HasValue)
		{
			await db.AddTimedPunishmentAsync(ToDbModel(context)).ConfigureAwait(false);
		}
		else if (!context.IsGive)
		{
			await db.DeleteTimedPunishmentAsync(ToDbModel(context)).ConfigureAwait(false);
		}
	}

	public void Start()
	{
		_ = Task.Run(async () =>
		{
			while (true)
			{
				var values = await db.GetExpiredPunishmentsAsync(time.UtcNow.Ticks).ConfigureAwait(false);

				var handled = new List<TimedPunishment>();
				foreach (var punishment in values)
				{
					try
					{
						if (await RemovePunishmentAsync(punishment).ConfigureAwait(false))
						{
							handled.Add(punishment);
						}
					}
					catch (Exception e)
					{
						logger.LogWarning(
							exception: e,
							message: "Exception occurred while removing a punishment. {@Info}.",
							args: punishment
						);
					}
				}
				await db.DeleteTimedPunishmentsAsync(handled).ConfigureAwait(false);

				await Task.Delay(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
			}
		});
	}

	private async Task<bool> RemovePunishmentAsync(TimedPunishment punishment)
	{
		// Older than a week should be ignored and removed
		if (time.UtcNow - punishment.EndTime > TimeSpan.FromDays(7))
		{
			return true;
		}

		var guild = await client.GetGuildAsync(punishment.GuildId).ConfigureAwait(false);
		if (guild is null)
		{
			return false;
		}

		try
		{
			var context = new DynamicPunishmentContext(guild, punishment.UserId, false, punishment.PunishmentType)
			{
				RoleId = punishment.RoleId
			};
			await HandleAsync(context).ConfigureAwait(false);
		}
		// Lacking permissions, assume the punishment will be handled by someone else
		catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
		{
		}
		catch
		{
			// TODO: differentiate between each error type?
			// E.G. on 500 retry eventually, but on a 404 or something don't?
			return false;
		}

		return true;
	}
}