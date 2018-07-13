using System;
using System.Linq;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens an alias down to an initialism. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class ShortAliasAttribute : AliasAttribute 
	{
		/// <summary>
		/// Initializes the attribute.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="otherAliases"></param>
		public ShortAliasAttribute(string name, params string[] otherAliases) : base(Shorten(name, otherAliases)) { }

		private static string[] Shorten(string name, string[] otherAliases)
		{
			var initialism = new Initialism(name, otherAliases, false);
			if (String.IsNullOrWhiteSpace(initialism.ToString()))
			{
				throw new ArgumentException("must have at least one capital letter", nameof(name));
			}

			return initialism.Aliases.ToArray();
		}
	}
}
