using System;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Classes.Attributes
{
	/// <summary>
	/// Shortens a name down to an initialism and throws an exception if there are any duplicates.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class TopLevelShortAliasAttribute : AliasAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="TopLevelShortAliasAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		public TopLevelShortAliasAttribute(Type type) : this(type, new string[0]) { }
		/// <summary>
		/// Creates an instance of <see cref="TopLevelShortAliasAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="aliases"></param>
		public TopLevelShortAliasAttribute(Type type, params string[] aliases) : base(AliasUtils.Concat(type, type.Name, aliases)) { }
	}
}
