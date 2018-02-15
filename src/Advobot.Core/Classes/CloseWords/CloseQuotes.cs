using Advobot.Core.Classes.Settings;
using Advobot.Core.Interfaces;
using Discord.Commands;
using System;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="IGuildSettings.Quotes"/>.
	/// </summary>
	public class CloseQuotes : CloseWords<Quote>
	{
		public CloseQuotes(TimeSpan time, ICommandContext context, IGuildSettings settings, string input) 
			: base(time, context, settings.Quotes, input) { }

		protected override int FindCloseness(Quote obj, string input)
		{
			return FindCloseName(obj.Name, input);
		}
	}
}
