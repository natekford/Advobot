﻿using System;
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
		// If the background task is processing removable punishments it adds them to a list
		// to remove a big batch at once
		// However, when actually removing the punishment on Discord via the punisher
		// the event from it still fires back into this class
		// To prevent removing the punishment by itself then again in the batch removal
		// this hashset prevents that
		private readonly HashSet<IPunishmentContext> _WillBeBatchRemoved = new HashSet<IPunishmentContext>();

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
						_WillBeBatchRemoved.Clear();
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
		{
			if (_WillBeBatchRemoved.Remove(context))
			{
				return Task.CompletedTask;
			}
			return _Db.DeleteRemovablePunishmentAsync(ToDbModel(context));
		}

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
				var context = new DynamicPunishmentContext(guild, punishment.UserId, false, punishment.PunishmentType)
				{
					RoleId = punishment.RoleId
				};
				_WillBeBatchRemoved.Add(context);
				await _Punisher.HandleAsync(context).CAF();
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

		private IReadOnlyRemovablePunishment ToDbModel(IPunishmentContext context)
		{
			return new RemovablePunishment
			{
				GuildId = context.Guild.Id,
				UserId = context.UserId,
				RoleId = context.Role?.Id ?? 0,
				PunishmentType = context.Type,
				EndTimeTicks = (_Time.UtcNow + context.Time)?.Ticks ?? -1
			};
		}
	}
}