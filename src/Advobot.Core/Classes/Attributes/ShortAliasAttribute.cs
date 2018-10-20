using System;
using System.Runtime.CompilerServices;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens a name down to an initialism. 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public class ShortAliasAttribute : AliasAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ShortAliasAttribute"/>.
		/// </summary>
		/// <param name="aliases"></param>
		/// <param name="name"></param>
		public ShortAliasAttribute(string[] aliases, [CallerMemberName] string name = null) : base(AliasUtils.Concat(null, name, aliases)) { }
		/// <summary>
		/// Creates an instance of <see cref="ShortAliasAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="aliases"></param>
		public ShortAliasAttribute(string name, params string[] aliases) : this(aliases, name) { }
		/// <summary>
		/// Creates an instance of <see cref="ShortAliasAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public ShortAliasAttribute([CallerMemberName] string name = null) : this(new string[0], name) { }
	}
}