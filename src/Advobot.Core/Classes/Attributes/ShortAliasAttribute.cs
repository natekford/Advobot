using Discord.Commands;
using System;

namespace Advobot.Core.Classes.Attributes
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
			var initialism = new Initialism(name, otherAliases, false);
			if (String.IsNullOrWhiteSpace(initialism.ToString()))
			{
				throw new ArgumentException("must have at least one capital letter", nameof(name));
			}

			return initialism.Aliases;
		}
	}
}
