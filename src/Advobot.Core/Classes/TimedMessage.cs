using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public abstract class TimedMessage : ITime
    {
		public IGuildUser Author { get; }
		public DateTime Time { get; }
		public string Text { get; }

		public TimedMessage(IGuildUser author, TimeSpan time, string text)
		{
			Author = author;
			Time = DateTime.UtcNow.Add(time);
			Text = text;
		}

		public async Task SendAsync()
		{
			await MessageUtils.SendMessageAsync(await GetChannelAsync().CAF(), Text).CAF();
		}
		public abstract Task<IMessageChannel> GetChannelAsync();
    }

	public class TimedUserMessage : TimedMessage
	{
		public IUser User { get; }

		public TimedUserMessage(IGuildUser author, IUser user, TimeSpan time, string text) : base(author, time, text)
		{
			User = user;
		}

		public override async Task<IMessageChannel> GetChannelAsync()
		{
			return await User.GetOrCreateDMChannelAsync().CAF();
		}
	}
	
	public class TimedChannelMessage : TimedMessage
	{
		public ITextChannel Channel { get; }

		public TimedChannelMessage(IGuildUser author, ITextChannel channel, TimeSpan time, string text) : base(author, time, text)
		{
			Channel = channel;
		}

		public override Task<IMessageChannel> GetChannelAsync()
		{
			return Task.FromResult((IMessageChannel)Channel);
		}
	}
}
