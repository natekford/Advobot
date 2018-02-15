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
		public CloseQuotes(TimeSpan time, ICommandContext context, IGuildSettings settings, string input) 
			: base(time, context, settings.Quotes, input) { }

		protected override CloseWord FindCloseWord(Quote obj, string input)
		{
			return new CloseWord(FindCloseName(obj.Name, input), obj.Name, obj.Description);
		}
		protected override CloseWord FindCloseWord(IEnumerable<Quote> objs, IEnumerable<string> alreadyUsedNames, string input)
		{
			var obj = objs.FirstOrDefault(x => !alreadyUsedNames.Contains(x.Name) && x.Name.CaseInsContains(input));
			return new CloseWord(int.MaxValue, obj.Name, obj.Description);
		}
	}
}
