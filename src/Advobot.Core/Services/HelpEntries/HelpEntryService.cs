using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
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
		private readonly Dictionary<string, string> _NameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		//Maps the name to the helpentry
		private readonly Dictionary<string, HelpEntry> _Source = new Dictionary<string, HelpEntry>(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc />
		public IHelpEntry? this[string name]
			=> _NameMap.TryGetValue(name, out var n) ? _Source[n] : null;

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
			}).Where(x => x.GetCustomAttribute<GroupAttribute>() != null && x.InheritsFromGeneric(typeof(ModuleBase<>)));
			foreach (var type in types)
			{
				VerifyCommandType(type);
				if (!IsCommand(type.GetCustomAttributes()) || !type.IsNested)
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
				if (!IsCommand(module.Attributes) || !module.IsSubmodule)
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
			foreach (var kvp in _Source.Where(x => x.Value.Name == helpEntry.Name).ToList())
			{
				_Source.Remove(kvp.Key);
				foreach (var kvp2 in _NameMap.Where(x => x.Value == kvp.Key).ToList())
				{
					_NameMap.Remove(kvp2.Key);
				}
			}
		}
		/// <inheritdoc />
		public IHelpEntry[] GetUnsetCommands(IEnumerable<string> setCommands)
			=> _Source.Values.Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
		/// <inheritdoc />
		public IHelpEntry[] GetHelpEntries(string? category = null)
		{
			return category == null
				? _Source.Values.ToArray()
				: _Source.Values.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
		/// <inheritdoc />
		public string[] GetCategories()
			=> _Source.Values.Select(x => x.Category).Distinct().ToArray();
		/// <inheritdoc />
		public IEnumerator<IHelpEntry> GetEnumerator()
			=> _Source.Values.GetEnumerator();
		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator()
			=> GetEnumerator();
		private bool IsCommand(IEnumerable<Attribute> attrs)
			=> attrs.Any(x => x is EnabledByDefaultAttribute);
		[Conditional("DEBUG")]
		private void VerifyCommandType(Type t)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public;
			var nested = GetAliases(t.GetNestedTypes(flags), x => x.GetCustomAttribute<AliasAttribute>());
			var method = GetAliases(t.GetMethods(flags), x => x.GetCustomAttribute<AliasAttribute>());
			VerifyAllAliasesAreDifferent(t.FullName, nested, method);
			if (t.IsNested)
			{
				return;
			}

			//Make sure is public
			if (t.IsNotPublic)
			{
				throw new InvalidOperationException($"{t.FullName} is not public and commands will not execute from it.");
			}
			//Make sure no commands are unmarked
			var methods = t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (methods.Any(x => x.GetCustomAttribute<CommandAttribute>() == null))
			{
				throw new InvalidOperationException($"{t.FullName} has a command missing the command attribute.");
			}
		}
		[Conditional("DEBUG")]
		private void VerifyCommandModule(ModuleInfo m)
		{
			var nested = GetAliases(m.Submodules, x => x.Attributes.GetAttribute<AliasAttribute>());
			var method = GetAliases(m.Commands, x => x.Attributes.GetAttribute<AliasAttribute>());
			VerifyAllAliasesAreDifferent(m.Name, nested, method);
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
						throw new InvalidOperationException($"The following aliases in {name} have conflicts: {string.Join(" + ", intersected)}");
					}
				}
			}
		}
		private static IEnumerable<string[]> GetAliases<T>(IReadOnlyCollection<T> source, Func<T, AliasAttribute?> getAlias)
		{
			foreach (var obj in source)
			{
				var aliasAttr = getAlias(obj);
				if (aliasAttr != null)
				{
					yield return aliasAttr.Aliases;
				}
			}
		}
	}
}