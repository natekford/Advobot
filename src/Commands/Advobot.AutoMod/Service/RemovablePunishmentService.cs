using System.Threading.Tasks;

using Advobot.AutoMod.Database;
using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;
using Advobot.Services.Time;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

namespace Advobot.AutoMod.Service
{
	public sealed class RemovablePunishmentService
	{
		private readonly BaseSocketClient _Client;
		private readonly RemovablePunishmentDatabase _Db;
		private readonly IPunisher _Punisher;
		private readonly ITime _Time;

		public RemovablePunishmentService(
			RemovablePunishmentDatabase db,
			BaseSocketClient client,
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
			new AsyncProcessingQueue(1, async () =>
			{
				var now = _Time.UtcNow.Ticks;
				var values = await _Db.GetOldPunishmentsAsync(now).CAF();

				foreach (var p in values)
				{
					if (!(_Client.GetGuild(p.GuildId) is IGuild guild))
					{
						continue;
					}

					await _Punisher.DynamicHandleAsync(guild, p.UserId, p.PunishmentType, false, p.RoleId, null).CAF();
				}
			}).Start();
		}

		private Task OnPunishmentGiven(IPunishmentContext context)
		{
			if (!context.Time.HasValue)
			{
				return Task.CompletedTask;
			}
			return _Db.AddRemovablePunishment(ToDbModel(context));
		}

		private Task OnPunishmentRemoved(IPunishmentContext context)
			=> _Db.DeleteRemovablePunishment(ToDbModel(context));

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