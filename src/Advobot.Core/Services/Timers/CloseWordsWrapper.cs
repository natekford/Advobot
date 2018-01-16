using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Interfaces;
using Discord;
using System;

namespace Advobot.Core.Services.Timers
{
	/// <summary>
	/// Puts <see cref="CloseWords"/> and <see cref="Message"/> into what is basically a tuple with <see cref="ITime"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class CloseWordsWrapper<T> : ITime where T : IDescription
	{
		public CloseWords<T> CloseWords { get; }
		public IUserMessage Message { get; }
		public DateTime Time => CloseWords.Time;

		public CloseWordsWrapper(CloseWords<T> closeWords, IUserMessage message)
		{
			CloseWords = closeWords;
			Message = message;
		}
	}
}
