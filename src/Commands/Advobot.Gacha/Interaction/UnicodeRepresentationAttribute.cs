using Discord;

namespace Advobot.Gacha.Interaction;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public sealed class UnicodeRepresentationAttribute(string unicode) : Attribute
{
	public string Name { get; } = new Emoji(unicode).Name;
}