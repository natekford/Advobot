using Discord.Commands;
using System;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens an alias down to an initialism. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ShortAliasAttribute : AliasAttribute 
	{
		public ShortAliasAttribute(string name, params string[] otherAliases) : base(Shorten(name, otherAliases)) { }

		private static string[] Shorten(string name, string[] otherAliases)
		{
			var initialism = new InitialismHolder(name, otherAliases, false);
			if (String.IsNullOrWhiteSpace(initialism.ToString()))
			{
				throw new ArgumentException("Invalid alias provided. Must have at least one capital letter.");
			}

			return initialism.Aliases;
		}
	}
}
