using System;

using Discord;

namespace Advobot.Gacha.Interaction
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class UnicodeRepresentationAttribute : Attribute
	{
		public UnicodeRepresentationAttribute(string unicode)
		{
			Name = new Emoji(unicode).Name;
		}

		public string Name { get; }
	}
}