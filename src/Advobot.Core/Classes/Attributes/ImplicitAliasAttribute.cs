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
		/// <param name="caller"></param>
		public ImplicitAliasAttribute([CallerMemberName] string caller = "") : this(new string[0], caller) { }
		/// <summary>
		/// Creates an instance of <see cref="ImplicitAliasAttribute"/>.
		/// </summary>
		/// <param name="aliases"></param>
		/// <param name="caller"></param>
		public ImplicitAliasAttribute(string[] aliases, [CallerMemberName] string caller = "") : base(AliasUtils.ConcatCommandAliases(caller, aliases)) { }
	}
}