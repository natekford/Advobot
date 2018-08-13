using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	internal sealed class HelpEntryService : IHelpEntryService
	{
		//Maps the name and aliases of a command to the name
		private Dictionary<string, string> _NameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		//Maps the name to the helpentry
		private Dictionary<string, HelpEntry> _Source = new Dictionary<string, HelpEntry>(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc />
		public IHelpEntry this[string name]
		{
			get => _NameMap.TryGetValue(name, out var n) ? _Source[n] : null;
		}
		/// <inheritdoc />
		public void Add(IEnumerable<Assembly> assemblies)
		{
			var types = assemblies.SelectMany(x =>
			{
				try
				{
					return x.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					return e.Types;
				}
			}).Where(x => x.GetCustomAttribute<GroupAttribute>() != null && x.IsAssignableFromGeneric(typeof(ModuleBase<>)));
			foreach (var type in types)
			{
				VerifyCommandType(type);
				//Nested commands don't need to be added since they're added under the class they're nested in
				if (type.IsNested)
				{
					continue;
				}

				var helpEntry = new HelpEntry(type);
				foreach (var alias in helpEntry.Aliases)
				{
					_NameMap.Add(alias, helpEntry.Name);
				}
				_NameMap.Add(helpEntry.Name, helpEntry.Name);
				_Source.Add(helpEntry.Name, helpEntry);
			}
		}
		/// <inheritdoc />
		public void Add(IEnumerable<ModuleInfo> modules)
		{
			foreach (var module in modules)
			{
				VerifyCommandModule(module);
				//Nested modules don't need to be added since they're added under the module they're nested in
				if (module.IsSubmodule)
				{
					continue;
				}

				var helpEntry = new HelpEntry(module);
				foreach (var alias in helpEntry.Aliases)
				{
					_NameMap.Add(alias, helpEntry.Name);
				}
				_Source.Add(helpEntry.Name, helpEntry);
			}
		}
		/// <inheritdoc />
		public void Remove(IHelpEntry helpEntry)
		{
			throw new NotImplementedException();
		}
		/// <inheritdoc />
		public IHelpEntry[] GetUnsetCommands(IEnumerable<string> setCommands)
		{
			return _Source.Values.Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
		}
		/// <inheritdoc />
		public IHelpEntry[] GetHelpEntries(string category = null)
		{
			return category == null
				? _Source.Values.ToArray()
				: _Source.Values.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
		/// <inheritdoc />
		public string[] GetCategories()
		{
			return _Source.Values.Select(x => x.Category).Distinct().ToArray();
		}
		/// <inheritdoc />
		public IEnumerator<IHelpEntry> GetEnumerator()
		{
			return _Source.Values.GetEnumerator();
		}
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		[Conditional("DEBUG")]
		private void VerifyCommandType(Type type)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var nestedAliases = type.GetNestedTypes(flags).Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases);
			var methodAliases = type.GetMethods(flags).Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases);

			VerifyAllAliasesAreDifferent(type.FullName, nestedAliases, methodAliases);
			if (type.IsNested)
			{
				return;
			}

			//Make sure is public
			if (type.IsNotPublic)
			{
				throw new InvalidOperationException($"{type.FullName} is not public and commands will not execute from it.");
			}
			//Make sure no commands are unmarked
			var methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (methods.Any(x => x.GetCustomAttribute<CommandAttribute>() == null))
			{
				throw new InvalidOperationException($"{type.FullName} has a command missing the command attribute.");
			}
		}
		[Conditional("DEBUG")]
		private void VerifyCommandModule(ModuleInfo module)
		{
			var nestedAliases = module.Submodules.Select(x => x.Attributes.GetAttribute<AliasAttribute>()?.Aliases);
			var methodAliases = module.Commands.Select(x => x.Attributes.GetAttribute<AliasAttribute>()?.Aliases);
			VerifyAllAliasesAreDifferent(module.Name, nestedAliases, methodAliases);
		}
		[Conditional("DEBUG")]
		private void VerifyAllAliasesAreDifferent(string name, IEnumerable<string[]> nestedAliases, IEnumerable<string[]> methodAliases)
		{
			var both = nestedAliases.Concat(methodAliases).Where(x => x != null).ToArray();
			for (var i = 0; i < both.Length; ++i)
			{
				for (var j = i + 1; j < both.Length; ++j)
				{
					var intersected = both[i].Intersect(both[j], StringComparer.OrdinalIgnoreCase).ToList();
					if (intersected.Any())
					{
						throw new InvalidOperationException($"The following aliases in {name} have conflicts: {String.Join(" + ", intersected)}");
					}
				}
			}
		}
	}
}