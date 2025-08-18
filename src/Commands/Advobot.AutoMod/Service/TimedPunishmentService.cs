using Advobot.AutoMod.Database;
using Advobot.AutoMod.Database.Models;
using Advobot.Punishments;
using Advobot.Services;
using Advobot.Services.Punishments;
using Advobot.Services.Time;

using Discord;
using Discord.Net;

using Microsoft.Extensions.Logging;

using System.Net;

namespace Advobot.AutoMod.Service;

public sealed class TimedPunishmentService(
	ILogger<TimedPunishmentService> logger,
	TimedPunishmentDatabase db,
	IDiscordClient client,
	ITimeService time
) : StartableService, IPunishmentService
{
	public async Task PunishAsync(IPunishment punishment, RequestOptions? options = null)
	{
		TimedPunishment ToDbModel(IPunishment context)
		{
			return new()
			{
				GuildId = context.Guild.Id,
				UserId = context.UserId,
				RoleId = context.RoleId,
				PunishmentType = context.Type,
				EndTimeTicks = (time.UtcNow + context.Duration)?.Ticks ?? -1
			};
		}

		try
		{
			await punishment.ExecuteAsync(options).ConfigureAwait(false);
			if (punishment.IsGive && punishment.Duration.HasValue)
			{
				await db.AddTimedPunishmentAsync(ToDbModel(punishment)).ConfigureAwait(false);
			}
			else if (!punishment.IsGive)
			{
				await db.DeleteTimedPunishmentAsync(ToDbModel(punishment)).ConfigureAwait(false);
			}
		}
		catch (Exception e)
		{
			logger.LogWarning(
				exception: e,
				message: "Exception occurred while handling punishment. {@Info}",
				args: new
				{
					Guild = punishment.Guild.Id,
					User = punishment.UserId,
					Role = punishment.RoleId,
					PunishmentType = punishment.Type,
					IsGive = punishment.IsGive,
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
		var guild = await client.GetGuildAsync(punishment.GuildId).ConfigureAwait(false);
		if (guild is null)
		{
			return false;
		}

		var context = new DynamicPunishment(guild, punishment.UserId, false, punishment.PunishmentType)
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