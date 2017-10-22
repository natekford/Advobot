using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.UsageGeneration;
using Advobot.Core.Enums;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	public class HelpEntryHolder
	{
		private readonly ImmutableList<HelpEntry> _Source;

		public HelpEntryHolder()
		{
			var types = Constants.COMMAND_ASSEMBLY.GetTypes().Where(x => x.IsSubclassOf(typeof(AdvobotModuleBase)) && x.GetCustomAttribute<GroupAttribute>() != null);
			if (!types.Any())
			{
				ConsoleActions.WriteLine($"The assembly {Constants.COMMAND_ASSEMBLY.GetName().Name} has no commands. Press any key to close the program.");
				Console.ReadKey();
				throw new TypeLoadException($"The assembly {Constants.COMMAND_ASSEMBLY.GetName().Name} has no commands.");
			}

			var temp = new List<HelpEntry>();
			foreach (var t in types)
			{
				var innerMostNameSpace = t.Namespace.Substring(t.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
					throw new ArgumentException($"{innerMostNameSpace} is not currently in the CommandCategory enum.");
				}
				//Nested commands don't need to be added since they're added under the class they're nested in
				else if (t.IsNested)
				{
#if DEBUG
					AssertAllAliasesAreDifferent(t);
#endif
					continue;
				}

				var name = t.GetCustomAttribute<GroupAttribute>()?.Prefix;
				var aliases = t.GetCustomAttribute<AliasAttribute>()?.Aliases;
				var summary = t.GetCustomAttribute<SummaryAttribute>()?.Text;
				var usage = new UsageGenerator(t).Text;
				var permReqs = t.GetCustomAttribute<PermissionRequirementAttribute>()?.ToString();
				var otherReqs = t.GetCustomAttribute<OtherRequirementAttribute>()?.ToString();
				var defaultEnabled = t.GetCustomAttribute<DefaultEnabledAttribute>()?.Enabled ?? false;

#if DEBUG
				//These are basically only here so I won't forget something.
				//Without them the bot should work fine, but may have tiny bugs.
				AssertNoDuplicateCommandNamesOrAliases(temp, name, aliases);
				AssertDefaultValueEnabledAttributeExists(t);
				AssertClassIsPublic(t);
				AssertAllCommandsHaveCommandAttribute(t);
				AssertAllAliasesAreDifferent(t);
				AssertShortAliasAttribute(t);
#endif

				temp.Add(new HelpEntry(name, usage, GeneralFormatting.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, aliases, category, defaultEnabled));
			}
			_Source = temp.ToImmutableList();
		}

		private void AssertNoDuplicateCommandNamesOrAliases(IEnumerable<HelpEntry> alreadyUsed, string name, string[] aliases)
		{
			var similarCmds = alreadyUsed.Where(x => x.Name.CaseInsEquals(name) || (x.Aliases != null && aliases != null && x.Aliases.Intersect(aliases, StringComparer.OrdinalIgnoreCase).Any()));
			if (similarCmds.Any())
			{
				throw new ArgumentException($"The following commands have conflicts: {String.Join(" + ", similarCmds.Select(x => x.Name))} + {name}");
			}
		}
		private void AssertDefaultValueEnabledAttributeExists(Type classType)
		{
			if (classType.GetCustomAttribute<DefaultEnabledAttribute>() == null)
			{
				throw new ArgumentException($"{classType.Name} does not have a default enabled value set.");
			}
		}
		private void AssertClassIsPublic(Type classType)
		{
			if (classType.IsNotPublic)
			{
				throw new ArgumentException($"{classType.Name} is not public and commands will not execute from it.");
			}
		}
		private void AssertAllCommandsHaveCommandAttribute(Type classType)
		{
			var methods = classType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (methods.Any(x => x.GetCustomAttribute<CommandAttribute>() == null))
			{
				throw new ArgumentException($"{classType.Name} has a command missing the command attribute.");
			}
		}
		private void AssertAllAliasesAreDifferent(Type classType)
		{
			var nestedAliases = classType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public)
				.Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases).Where(x => x != null);
			var methodAliases = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases).Where(x => x != null);
			var both = nestedAliases.Concat(methodAliases).ToArray();
			for (int i = 0; i < both.Count(); ++i)
			{
				for (int j = i + 1; j < both.Count(); ++j)
				{
					var intersected = both[i].Intersect(both[j], StringComparer.OrdinalIgnoreCase);
					if (intersected.Any())
					{
						throw new ArgumentException($"The following aliases in {classType.Name} have conflicts: {String.Join(" + ", intersected)}");
					}
				}
			}
		}
		private void AssertShortAliasAttribute(Type classType)
		{
			if (classType.GetCustomAttribute<TopLevelShortAliasAttribute>() == null)
			{
				throw new ArgumentException($"The class {classType.Name} needs to have the {nameof(TopLevelShortAliasAttribute)} attribute.");
			}
		}

		/// <summary>
		/// Returns all the names of every command.
		/// </summary>
		/// <returns></returns>
		public string[] GetCommandNames()
		{
			return _Source.Select(x => x.Name).ToArray();
		}
		/// <summary>
		/// Retrurns an array of <see cref="HelpEntry"/> which have not had their values set in guild settings.
		/// </summary>
		/// <param name="setCommands"></param>
		/// <returns></returns>
		public HelpEntry[] GetUnsetCommands(IEnumerable<string> setCommands)
		{
			return _Source.Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
		}
		/// <summary>
		/// Returns an array of every <see cref="HelpEntry"/>.
		/// </summary>
		/// <returns></returns>
		public HelpEntry[] GetHelpEntries()
		{
			return _Source.ToArray();
		}

		public HelpEntry this[string nameOrAlias]
		{
			get => _Source.SingleOrDefault(x => x.Name.CaseInsEquals(nameOrAlias) || x.Aliases.CaseInsContains(nameOrAlias));
		}
		public HelpEntry[] this[CommandCategory category]
		{
			get => _Source.Where(x => x.Category == category).ToArray();
		}
	}
}
