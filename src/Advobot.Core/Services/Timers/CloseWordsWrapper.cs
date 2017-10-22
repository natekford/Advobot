using Advobot.Core.Classes.CloseWords;
using Advobot.Core.Interfaces;
using Discord;
using System;


namespace Advobot.Core.Services.Timers
{
	/// <summary>
	/// Puts <see cref="CloseWords"/> and <see cref="Message"/> into what is basically a tuple with <see cref="IHasTime"/>.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal sealed class CloseWordsWrapper<T> : IHasTime where T : IDescription
	{
		public readonly CloseWords<T> CloseWords;
		public readonly IUserMessage Message;

		public CloseWordsWrapper(CloseWords<T> closeWords, IUserMessage message)
		{
			CloseWords = closeWords;
			Message = message;
		}

		public DateTime GetTime()
		{
			return CloseWords.GetTime();
		}
	}
}
