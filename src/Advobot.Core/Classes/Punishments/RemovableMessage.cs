using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovableMessage : ITime
	{
		public ImmutableArray<IMessage> Messages { get; }
		public ITextChannel Channel { get; }
		public DateTime Time { get; }

		public RemovableMessage(TimeSpan timeUntilRemoval, params IMessage[] messages)
		{
			Messages = messages.ToImmutableArray();
			Channel = messages.FirstOrDefault().Channel as ITextChannel;
			Time = DateTime.UtcNow.Add(timeUntilRemoval);
		}
	}
}
