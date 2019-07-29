using System;
using Advobot.Utilities;
using Discord.Commands;

namespace Advobot.Attributes
{
	/// <summary>
	/// Shortens a name down to an initialism and throws an exception if there are any duplicates.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class ModuleInitialismAliasAttribute : AliasAttribute
	{
		/// <summary>
		/// Creates an instance of <see cref="ModuleInitialismAliasAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		public ModuleInitialismAliasAttribute(Type type) : this(new string[0], type) { }
		/// <summary>
		/// Creates an instance of <see cref="ModuleInitialismAliasAttribute"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="aliases"></param>
		public ModuleInitialismAliasAttribute(string[] aliases, Type type) : base(AliasUtils.ConcatModuleAliases(type, aliases)) { }
	}
}
