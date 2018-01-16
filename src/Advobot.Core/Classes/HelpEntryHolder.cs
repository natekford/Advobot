using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.UsageGeneration;
using Advobot.Core.Enums;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Advobot.Core.Interfaces;
using System.Collections.ObjectModel;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	public class HelpEntryHolder
	{
		//Maps the name and aliases of a command to the name
		private Dictionary<string, string> _NameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		//Maps the name to the helpentry
		private Dictionary<string, HelpEntry> _Source = new Dictionary<string, HelpEntry>();

		public HelpEntryHolder()
		{
			var types = Constants.COMMAND_ASSEMBLY.GetTypes().Where(x => x.IsSubclassOf(typeof(AdvobotModuleBase)) && x.GetCustomAttribute<GroupAttribute>() != null);
			if (!types.Any())
			{
				ConsoleUtils.WriteLine($"The assembly {Constants.COMMAND_ASSEMBLY.GetName().Name} has no commands. Press any key to close the program.");
				Console.ReadKey();
				throw new TypeLoadException($"The assembly {Constants.COMMAND_ASSEMBLY.GetName().Name} has no commands.");
			}

			foreach (var t in types)
			{
				var innerMostNameSpace = t.Namespace.Substring(t.Namespace.LastIndexOf('.') + 1);
				if (!Enum.TryParse(innerMostNameSpace, true, out CommandCategory category))
				{
					throw new ArgumentException($"is not currently in the {nameof(CommandCategory)} enum", innerMostNameSpace);
				}
				//Nested commands don't need to be added since they're added under the class they're nested in
				else if (t.IsNested)
				{
#if DEBUG
					VerifyAllAliasesAreDifferent(t);
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
				VerifyDefaultValueEnabledAttributeExists(t);
				VerifyClassIsPublic(t);
				VerifyAllCommandsHaveCommandAttribute(t);
				VerifyAllAliasesAreDifferent(t);
				VerifyShortAliasAttribute(t);
#endif

				var helpEntry = new HelpEntry(name, usage, GeneralFormatting.JoinNonNullStrings(" | ", new[] { permReqs, otherReqs }), summary, aliases, category, defaultEnabled);
				_NameMap.Add(name.ToLower(), name);
				foreach (var alias in aliases ?? new string[0])
				{
					_NameMap.Add(alias.ToLower(), name);
				}

				_Source.Add(name, helpEntry);
			}
		}

		private void VerifyDefaultValueEnabledAttributeExists(Type classType)
		{
			if (classType.GetCustomAttribute<DefaultEnabledAttribute>() == null)
			{
				throw new InvalidOperationException($"{classType.FullName} does not have a default enabled value set.");
			}
		}
		private void VerifyClassIsPublic(Type classType)
		{
			if (classType.IsNotPublic)
			{
				throw new InvalidOperationException($"{classType.FullName} is not public and commands will not execute from it.");
			}
		}
		private void VerifyAllCommandsHaveCommandAttribute(Type classType)
		{
			var methods = classType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (methods.Any(x => x.GetCustomAttribute<CommandAttribute>() == null))
			{
				throw new InvalidOperationException($"{classType.FullName} has a command missing the command attribute.");
			}
		}
		private void VerifyAllAliasesAreDifferent(Type classType)
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
						throw new InvalidOperationException($"The following aliases in {classType.FullName} have conflicts: {String.Join(" + ", intersected)}");
					}
				}
			}
		}
		private void VerifyShortAliasAttribute(Type classType)
		{
			if (classType.GetCustomAttribute<TopLevelShortAliasAttribute>() == null)
			{
				throw new InvalidOperationException($"The class {classType.FullName} needs to have the {nameof(TopLevelShortAliasAttribute)} attribute.");
			}
		}

		/// <summary>
		/// Returns all the names of every command.
		/// </summary>
		/// <returns></returns>
		public string[] GetCommandNames()
		{
			return _Source.Keys.ToArray();
		}
		/// <summary>
		/// Retrurns an array of <see cref="HelpEntry"/> which have not had their values set in guild settings.
		/// </summary>
		/// <param name="setCommands"></param>
		/// <returns></returns>
		public HelpEntry[] GetUnsetCommands(IEnumerable<string> setCommands)
		{
			return _Source.Values.Where(x => !setCommands.CaseInsContains(x.Name)).ToArray();
		}
		/// <summary>
		/// Returns an array of every <see cref="HelpEntry"/>.
		/// </summary>
		/// <returns></returns>
		public HelpEntry[] GetHelpEntries()
		{
			return _Source.Values.ToArray();
		}
	
		public HelpEntry this[string nameOrAlias]
		{
			get => _NameMap.TryGetValue(nameOrAlias, out var name) ? _Source[name] : null;
		}
		public HelpEntry[] this[CommandCategory category]
		{
			get => _Source.Values.Where(x => x.Category == category).ToArray();
		}

		/// <summary>
		/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
		/// </summary>
		public class HelpEntry : IDescription
		{
			public string Name { get; }
			public string Usage { get; }
			public string BasePerm { get; }
			public string Description { get; }
			public string[] Aliases { get; }
			public CommandCategory Category { get; }
			public bool DefaultEnabled { get; }

			internal HelpEntry(string name, string usage, string basePerm, string description, string[] aliases, CommandCategory category, bool defaultEnabled)
			{
				if (String.IsNullOrWhiteSpace(name))
				{
					throw new ArgumentException("cant be null or whitespace", nameof(name));
				}

				Name = name;
				Usage = usage ?? "";
				BasePerm = String.IsNullOrWhiteSpace(basePerm) ? "N/A" : basePerm;
				Description = String.IsNullOrWhiteSpace(description) ? "N/A" : description;
				Aliases = aliases ?? new[] { "N/A" };
				Category = category;
				DefaultEnabled = defaultEnabled;
			}

			public override string ToString()
			{
				return $"**Aliases:** {String.Join(", ", Aliases)}\n" +
					$"**Usage:** {Constants.PLACEHOLDER_PREFIX}{Name} {Usage}\n" +
					$"**Enabled By Default:** {(DefaultEnabled ? "Yes" : "No")}\n\n" +
					$"**Base Permission(s):**\n{BasePerm}\n\n" +
					$"**Description:**\n{Description}";
			}
		}
	}
}
