using Advobot.Classes.Settings;
using Advobot.Interfaces;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="IGuildSettings.Quotes"/>.
	/// </summary>
	public class CloseQuotes : CloseWords<Quote>
	{
		/// <summary>
		/// Initializes the object. Parameterless constructor is used for the database.
		/// </summary>
		public CloseQuotes() { }
		/// <summary>
		/// Initializes the object with the supplied values.
		/// </summary>
		/// <param name="time"></param>
		/// <param name="context"></param>
		/// <param name="settings"></param>
		/// <param name="search"></param>
		public CloseQuotes(TimeSpan time, ICommandContext context, IGuildSettings settings, string search)  : base(time, context)
		{
			Populate(settings.Quotes, search);
		}

		/// <inheritdoc />
		protected override bool IsCloseWord(Quote obj, string search, out CloseWord closeWord)
		{
			var closeness = FindCloseness(obj.Name, search);
			var success = closeness < MaxAllowedCloseness;
			closeWord = success ? new CloseWord(closeness, obj.Name, obj.Description) : null;
			return success;
		}
		/// <inheritdoc />
		protected override bool TryGetCloseWord(
			IEnumerable<Quote> objs,
			IEnumerable<string> used,
			string search,
			out CloseWord closeWord)
		{
			var obj = objs.FirstOrDefault(x => !used.Contains(x.Name) && x.Name.CaseInsContains(search));
			closeWord = obj != null ? new CloseWord(int.MaxValue, obj.Name, obj.ToString()) : null;
			return obj != null;
		}
	}
}
