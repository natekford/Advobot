using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Discord.Commands;

namespace Advobot.Localization
{
	/// <summary>
	/// Used for a localized alias.
	/// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public abstract class LocalizedAliasBaseAttribute : AliasAttribute, ILocalized
	{
		/// <summary>
		/// The names of the aliases to use for localization.
		/// </summary>
		public IReadOnlyList<string> Names { get; }
		/// <inheritdoc />
		public ResourceManager ResourceManager { get; }

		/// <summary>
		/// Creates an instance of <see cref="LocalizedAliasBaseAttribute"/>.
		/// </summary>
		/// <param name="names"></param>
		/// <param name="resources"></param>
		public LocalizedAliasBaseAttribute(string[] names, ResourceManager resources)
			: base(names.Select(x => resources.GetString(x)).ToArray())
		{
			Names = names;
			ResourceManager = resources;
		}
	}
}
