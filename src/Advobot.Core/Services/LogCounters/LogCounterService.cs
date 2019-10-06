using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

using Advobot.Services.Commands;

using AdvorangesUtils;

using Discord.WebSocket;

namespace Advobot.Services.LogCounters
{
	internal sealed class LogCounterService : ILogCounterService, INotifyPropertyChanged
	{
		public LogCounter Animated { get; } = new LogCounter();
		public LogCounter AttemptedCommands { get; } = new LogCounter();
		public LogCounter FailedCommands { get; } = new LogCounter();
		public LogCounter Files { get; } = new LogCounter();
		public LogCounter Images { get; } = new LogCounter();
		public LogCounter MessageDeletes { get; } = new LogCounter();
		public LogCounter MessageEdits { get; } = new LogCounter();
		public LogCounter Messages { get; } = new LogCounter();
		public LogCounter SuccessfulCommands { get; } = new LogCounter();
		public LogCounter TotalGuilds { get; } = new LogCounter();
		public LogCounter TotalUsers { get; } = new LogCounter();
		public LogCounter UserChanges { get; } = new LogCounter();
		public LogCounter UserJoins { get; } = new LogCounter();
		public LogCounter UserLeaves { get; } = new LogCounter();

		ILogCounter ILogCounterService.Animated => Animated;
		ILogCounter ILogCounterService.AttemptedCommands => AttemptedCommands;
		ILogCounter ILogCounterService.FailedCommands => FailedCommands;
		ILogCounter ILogCounterService.Files => Files;
		ILogCounter ILogCounterService.Images => Images;
		ILogCounter ILogCounterService.MessageDeletes => MessageDeletes;
		ILogCounter ILogCounterService.MessageEdits => MessageEdits;
		ILogCounter ILogCounterService.Messages => Messages;
		ILogCounter ILogCounterService.SuccessfulCommands => SuccessfulCommands;
		ILogCounter ILogCounterService.TotalGuilds => TotalGuilds;
		ILogCounter ILogCounterService.TotalUsers => TotalUsers;
		ILogCounter ILogCounterService.UserChanges => UserChanges;
		ILogCounter ILogCounterService.UserJoins => UserJoins;
		ILogCounter ILogCounterService.UserLeaves => UserLeaves;

		public event PropertyChangedEventHandler? PropertyChanged;

		public LogCounterService(
			BaseSocketClient client,
			ICommandHandlerService commandHandler)
		{
			client.GuildAvailable += OnGuildAvailable;
			client.GuildUnavailable += OnGuildUnavailable;
			client.UserJoined += (_) => Add(TotalUsers, 1);
			client.UserLeft += (_) => Add(TotalUsers, -1);
			client.UserUpdated += (_, __) => Add(UserChanges, 1);
			client.MessageReceived += OnMessageReceived;
			client.MessageUpdated += (_, __, ___) => Add(MessageDeletes, 1);
			client.MessageDeleted += (_, __) => Add(MessageDeletes, 1);

			commandHandler.CommandInvoked += (_, result) =>
			{
				(result.IsSuccess ? SuccessfulCommands : FailedCommands).Add(1);
				AttemptedCommands.Add(1);
				return Task.CompletedTask;
			};
		}

		private Task Add(LogCounter counter, int amount)
		{
			AddAndNotify(counter, amount);
			return Task.CompletedTask;
		}

		private void AddAndNotify(LogCounter counter, int amount)
		{
			counter.Add(amount);
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(counter.Name));
		}

		private Task OnGuildAvailable(SocketGuild guild)
		{
			AddAndNotify(TotalUsers, guild.MemberCount);
			AddAndNotify(TotalGuilds, 1);
			return Task.CompletedTask;
		}

		private Task OnGuildUnavailable(SocketGuild guild)
		{
			AddAndNotify(TotalUsers, -guild.MemberCount);
			AddAndNotify(TotalGuilds, -1);
			return Task.CompletedTask;
		}

		private Task OnMessageReceived(SocketMessage message)
		{
			Messages.Add(1);

			int files = 0, images = 0, animated = 0;
			foreach (var attachment in message.Attachments)
			{
				var ext = MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(attachment.Url));
				if (ext.CaseInsContains("video/") || ext.CaseInsContains("/gif"))
				{
					++animated;
				}
				else if (ext.CaseInsContains("image/"))
				{
					++images;
				}
				else
				{
					++files;
				}
			}
			foreach (var embed in message.Embeds)
			{
				if (embed.Video != null)
				{
					++animated;
				}
				else if (embed.Image != null)
				{
					++images;
				}
			}
			AddAndNotify(Files, files);
			AddAndNotify(Images, images);
			AddAndNotify(Animated, animated);
			return Task.CompletedTask;
		}
	}
}