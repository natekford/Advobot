using System;
using System.Collections.Immutable;
using System.Linq;
using Advobot.Core.Interfaces;
using Discord;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after the time is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovableMessage : ITime
	{
		public ImmutableList<IMessage> Messages { get; }
		public ITextChannel Channel { get; }
		public DateTime Time { get; }

		public RemovableMessage(TimeSpan time, params IMessage[] messages)
		{
			Messages = messages.ToImmutableList();
			Channel = messages.FirstOrDefault().Channel as ITextChannel;
			Time = DateTime.UtcNow.Add(time);
		}
	}
}
