using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Punishments;
using Advobot.Services;
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
) : StartableService, IPunishmentService
{
	public async Task PunishAsync(IPunishmentContext context)
	{
		TimedPunishment ToDbModel(IPunishmentContext context)
		{
			return new()
			{
				GuildId = context.Guild.Id,
				UserId = context.UserId,
				RoleId = context.Role?.Id ?? 0,
				PunishmentType = context.Type,
				EndTimeTicks = (time.UtcNow + context.Duration)?.Ticks ?? -1
			};
		}

		try
		{
			await context.ExecuteAsync().ConfigureAwait(false);
			if (context.IsGive && context.Duration.HasValue)
			{
				await db.AddTimedPunishmentAsync(ToDbModel(context)).ConfigureAwait(false);
			}
			else if (!context.IsGive)
			{
				await db.DeleteTimedPunishmentAsync(ToDbModel(context)).ConfigureAwait(false);
			}
		}
		catch (Exception e)
		{
			logger.LogWarning(
				exception: e,
				message: "Exception occurred while handling punishment. {@Info}",
				args: new
				{
					Guild = context.Guild.Id,
					User = context.UserId,
					Role = context.Role?.Id ?? 0,
					PunishmentType = context.Type,
					IsGive = context.IsGive,
				}
			);
			throw;
		}
	}

	protected override Task StartAsyncImpl()
	{
		_ = Task.Run(async () =>
		{
			while (IsRunning)
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

				await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
			}
		});
		return Task.CompletedTask;
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

		var context = new DynamicPunishmentContext(guild, punishment.UserId, false, punishment.PunishmentType)
		{
			RoleId = punishment.RoleId
		};
		try
		{
			await PunishAsync(context).ConfigureAwait(false);
		}
		// Lacking permissions, assume the punishment will be handled by someone else
		catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
		{
			return true;
		}
		catch
		{
			return false;
		}

		return true;
	}
}