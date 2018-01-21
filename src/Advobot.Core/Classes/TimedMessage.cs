using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	public class TimedMessage : ITime
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
			await Author.SendMessageAsync(Text).CAF();
		}
    }
}
