using Advobot.Core.Classes.Settings;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Advobot.Core.Classes.CloseWords
{
	/// <summary>
	/// Implementation of <see cref="CloseWords{T}"/> which searches through <see cref="IGuildSettings.Quotes"/>.
	/// </summary>
	public class CloseQuotes : CloseWords<Quote>
	{
		public CloseQuotes() { }
		public CloseQuotes(TimeSpan time, ICommandContext context, IGuildSettings settings, string search) 
			: base(time, context, settings.Quotes, search) { }

		protected override CloseWord FindCloseWord(Quote obj, string search)
		{
			return new CloseWord(FindCloseName(obj.Name, search), obj.Name, obj.Description);
		}
		protected override CloseWord FindCloseWord(IEnumerable<Quote> objs, IEnumerable<string> alreadyUsedNames, string search)
		{
			var obj = objs.FirstOrDefault(x => !alreadyUsedNames.Contains(x.Name) && x.Name.CaseInsContains(search));
			return obj == null ? new CloseWord(-1, null, null) : new CloseWord(int.MaxValue, obj.Name, obj.Description);
		}
	}
}
