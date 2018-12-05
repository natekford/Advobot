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
	public sealed class ImplicitAliasAttribute : AliasAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ImplicitAliasAttribute"/>.
		/// </summary>
		/// <param name="name"></param>
		public ImplicitAliasAttribute([CallerMemberName] string name = "") : this(new string[0], name) { }
		/// <summary>
		/// Creates an instance of <see cref="ImplicitAliasAttribute"/>.
		/// </summary>
		/// <param name="aliases"></param>
		/// <param name="name"></param>
		public ImplicitAliasAttribute(string[] aliases, [CallerMemberName] string name = "") : base(AliasUtils.ConcatCommandAliases(name, aliases)) { }
	}
}