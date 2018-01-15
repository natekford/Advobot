using Advobot.Core.Interfaces;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.Punishments
{
	/// <summary>
	/// Messages that will get deleted after <see cref="GetTime"/> is less than <see cref="DateTime.UtcNow"/>.
	/// </summary>
	public struct RemovableMessage : IHasTime
	{
		public readonly IReadOnlyList<IMessage> Messages;
		public readonly ITextChannel Channel;
		private readonly DateTime _Time;

		public RemovableMessage(int seconds, params IMessage[] messages)
		{
			Messages = messages.ToList().AsReadOnly();
			Channel = messages.FirstOrDefault().Channel as ITextChannel;
			_Time = DateTime.UtcNow.AddSeconds(seconds);
		}

		public DateTime GetTime()
		{
			return _Time;
		}
	}
}
