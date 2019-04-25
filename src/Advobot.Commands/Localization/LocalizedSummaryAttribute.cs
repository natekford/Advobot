using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace Advobot.Commands.Localization
{
	public sealed class LocalizedSummaryAttribute : SummaryAttribute
	{
		public LocalizedSummaryAttribute(string resource) : base(strings.ResourceManager.GetString(resource)) { }
	}
}
