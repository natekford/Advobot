using Advobot.Actions.Formatting;
using Advobot.Classes.Attributes;
using Advobot.Classes.UsageGeneration;
using Advobot.Enums;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Advobot.Classes
{
	public class HelpEntryHolder
	{
		private readonly ImmutableList<HelpEntry> _Source;

		public HelpEntryHolder()
		{
			var temp = new List<HelpEntry>();
			var types = Assembly.GetExecutingAssembly().GetTypes();
			var cmds = types.Where(x => x.IsSubclassOf(typeof(AdvobotModuleBase)) && x.GetCustomAttribute<GroupAttribute>() != null);
			foreach (var classType in cmds)
			{
				var innerMostNameSpace = classType.Namespace.Substring(classType.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
					throw new ArgumentException($"{innerMostNameSpace} is not currently in the CommandCategory enum.");
				}
				//Nested commands don't need to be added since they're added under the class they're nested in
				else if (classType.IsNested)
				{
#if DEBUG
					AssertAllAliasesAreDifferent(classType);
#endif
					continue;
				}

				var name = classType.GetCustomAttribute<GroupAttribute>()?.Prefix;
				var aliases = classType.GetCustomAttribute<AliasAttribute>()?.Aliases;
				var summary = classType.GetCustomAttribute<SummaryAttribute>()?.Text;
				var usage = new UsageGenerator(classType).Text;
				var permReqs = classType.GetCustomAttribute<PermissionRequirementAttribute>()?.ToString();
				var otherReqs = classType.GetCustomAttribute<OtherRequirementAttribute>()?.ToString();
				var defaultEnabled = classType.GetCustomAttribute<DefaultEnabledAttribute>()?.Enabled ?? false;

#if DEBUG
				//These are basically only here so I won't forget something.
				//Without them the bot should work fine, but may have tiny bugs.
				AssertNoDuplicateCommandNamesOrAliases(temp, name, aliases);
				AssertDefaultValueEnabledAttributeExists(classType);
				AssertClassIsPublic(classType);
				AssertAllCommandsHaveCommandAttribute(classType);
				AssertAllAliasesAreDifferent(classType);
				AssertShortAliasAttribute(classType);
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
