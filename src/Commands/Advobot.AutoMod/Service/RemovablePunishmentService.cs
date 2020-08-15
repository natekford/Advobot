using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.Net;

namespace Advobot.AutoMod.Service
{
	public sealed class RemovablePunishmentService
	{
		private readonly IDiscordClient _Client;
		private readonly IRemovablePunishmentDatabase _Db;
		private readonly IPunisher _Punisher;
		private readonly ITime _Time;

		public RemovablePunishmentService(
			IRemovablePunishmentDatabase db,
			IDiscordClient client,
			ITime time,
			IPunisher punisher)
		{
			_Db = db;
			_Client = client;
			_Time = time;
			_Punisher = punisher;

			punisher.PunishmentGiven += OnPunishmentGiven;
			punisher.PunishmentRemoved += OnPunishmentRemoved;
		}

		public void Start()
		{
			new AsyncProcessor(1, async () =>
			{
				while (true)
				{
					var now = _Time.UtcNow.Ticks;
					var values = await _Db.GetOldPunishmentsAsync(now).CAF();

					var handled = new List<IReadOnlyRemovablePunishment>();
					try
					{
						foreach (var p in values)
						{
							var isHandled = await RemovePunishmentAsync(p).CAF();
							if (isHandled)
							{
								handled.Add(p);
							}
						}
					}
					finally
					{
						await _Db.DeleteRemovablePunishmentsAsync(handled).CAF();
					}

					await Task.Delay(TimeSpan.FromMinutes(1)).CAF();
				}
			}).Start();
		}

		private Task OnPunishmentGiven(IPunishmentContext context)
		{
			if (!context.Time.HasValue)
			{
				return Task.CompletedTask;
			}
			return _Db.AddRemovablePunishmentAsync(ToDbModel(context));
		}

		private Task OnPunishmentRemoved(IPunishmentContext context)
			=> _Db.DeleteRemovablePunishmentAsync(ToDbModel(context));

		private async Task<bool> RemovePunishmentAsync(IReadOnlyRemovablePunishment punishment)
		{
			// Older than a week should be ignored and removed
			if (_Time.UtcNow - punishment.EndTime > TimeSpan.FromDays(7))
			{
				return true;
			}

			var guild = await _Client.GetGuildAsync(punishment.GuildId).CAF();
			if (guild is null)
			{
				return false;
			}

			try
			{
				await _Punisher.DynamicHandleAsync(guild, punishment.UserId,
					punishment.PunishmentType, false, punishment.RoleId, null).CAF();
			}
			// Lacking permissions, assume the punishment is being handled by someone else
			catch (HttpException e) when (e.HttpCode == HttpStatusCode.Forbidden)
			{
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}

		private IReadOnlyRemovablePunishment ToDbModel(IPunishmentContext context)
		{
			return new RemovablePunishment
			{
				GuildId = context.Guild.Id,
				UserId = context.UserId,
				RoleId = context.Role?.Id ?? 0,
				PunishmentType = context.Type,
				EndTimeTicks = (_Time.UtcNow + context.Time!.Value).Ticks
			};
		}
	}
}