using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.UsageGeneration;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Creates a help entry for every command and then allows those to be accessed.
	/// </summary>
	public sealed class HelpEntryHolder
	{
		//Keep the names of the category to the category
		private Dictionary<string, string> _CategoryMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		//Maps the name and aliases of a command to the name
		private Dictionary<string, string> _NameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		//Maps the name to the helpentry
		private Dictionary<string, HelpEntry> _Source = new Dictionary<string, HelpEntry>(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Gathers command information from the supplied assemblies.
		/// </summary>
		/// <param name="commandAssemblies"></param>
		public HelpEntryHolder(IEnumerable<Assembly> commandAssemblies)
		{
			var commands = commandAssemblies.SelectMany(x =>
			{
				try
				{
					return x.GetTypes();
				}
				catch (ReflectionTypeLoadException e)
				{
					return e.Types;
				}
			}).Where(x => x != null && x.IsSubclassOf(typeof(NonSavingModuleBase)) && x.GetCustomAttribute<GroupAttribute>() != null).ToList();
			if (!commands.Any())
			{
				var assemblyNames = String.Join(", ", commandAssemblies.Select(x => x.GetName().Name));
				ConsoleUtils.WriteLine($"The following assemblies have no commands: '{assemblyNames}'.");
				Console.Read();
				throw new TypeLoadException($"The following assemblies have no commands: '{assemblyNames}'.");
			}

			foreach (var command in commands)
			{
				//Nested commands don't need to be added since they're added under the class they're nested in
				if (command.IsNested)
				{
					VerifyAllAliasesAreDifferent(command);
					continue;
				}

				var innerNamespace = command.Namespace.Substring(command.Namespace.LastIndexOf('.') + 1);
				if (!_CategoryMap.TryGetValue(innerNamespace, out var category))
				{
					_CategoryMap[innerNamespace] = category = innerNamespace;
				}

				var name = command.GetCustomAttribute<GroupAttribute>()?.Prefix;
				var aliases = command.GetCustomAttribute<AliasAttribute>()?.Aliases;
				var summary = command.GetCustomAttribute<SummaryAttribute>()?.Text;
				var usage = new UsageGenerator(command).Text;
				var permReqs = command.GetCustomAttribute<PermissionRequirementAttribute>()?.ToString();
				var otherReqs = command.GetCustomAttribute<OtherRequirementAttribute>()?.ToString();
				var defaultEnabled = command.GetCustomAttribute<DefaultEnabledAttribute>()?.Enabled ?? false;
				var unableToBeTurnedOff = command.GetCustomAttribute<DefaultEnabledAttribute>()?.AbleToToggle ?? true;

				//These are basically only here so I won't forget something.
				//Without them the bot should work fine, but may have tiny bugs.
				VerifyDefaultValueEnabledAttributeExists(command);
				VerifyClassIsPublic(command);
				VerifyAllCommandsHaveCommandAttribute(command);
				VerifyAllAliasesAreDifferent(command);
				VerifyShortAliasAttribute(command);

				var helpEntry = new HelpEntry(name, usage, new[] { permReqs, otherReqs }.JoinNonNullStrings(" | "),
					summary, aliases, category, defaultEnabled, unableToBeTurnedOff);
				_NameMap.Add(name.ToLower(), name);
				foreach (var alias in aliases ?? new string[0])
				{
					_NameMap.Add(alias.ToLower(), name);
				}

				_Source.Add(name, helpEntry);
			}
		}

		[Conditional("DEBUG")]
		private void VerifyDefaultValueEnabledAttributeExists(Type classType)
		{
			if (classType.GetCustomAttribute<DefaultEnabledAttribute>() == null)
			{
				throw new InvalidOperationException($"{classType.FullName} does not have a default enabled value set.");
			}
		}
		[Conditional("DEBUG")]
		private void VerifyClassIsPublic(Type classType)
		{
			if (classType.IsNotPublic)
			{
				throw new InvalidOperationException($"{classType.FullName} is not public and commands will not execute from it.");
			}
		}
		[Conditional("DEBUG")]
		private void VerifyAllCommandsHaveCommandAttribute(Type classType)
		{
			var methods = classType.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			if (methods.Any(x => x.GetCustomAttribute<CommandAttribute>() == null))
			{
				throw new InvalidOperationException($"{classType.FullName} has a command missing the command attribute.");
			}
		}
		[Conditional("DEBUG")]
		private void VerifyAllAliasesAreDifferent(Type classType)
		{
			var nestedAliases = classType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public)
				.Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases).Where(x => x != null);
			var methodAliases = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Select(x => x.GetCustomAttribute<AliasAttribute>()?.Aliases).Where(x => x != null);
			var both = nestedAliases.Concat(methodAliases).ToArray();
			for (var i = 0; i < both.Count(); ++i)
			{
				for (var j = i + 1; j < both.Count(); ++j)
				{
					var intersected = both[i].Intersect(both[j], StringComparer.OrdinalIgnoreCase).ToList();
					if (intersected.Any())
					{
						throw new InvalidOperationException($"The following aliases in {classType.FullName} have conflicts: {String.Join(" + ", intersected)}");
					}
				}
			}
		}
		[Conditional("DEBUG")]
		private void VerifyShortAliasAttribute(Type classType)
		{
			if (classType.GetCustomAttribute<TopLevelShortAliasAttribute>() == null)
			{
				throw new InvalidOperationException($"The class {classType.FullName} needs to have the {nameof(TopLevelShortAliasAttribute)} attribute.");
			}
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
		/// <summary>
		/// Returns an array of every <see cref="HelpEntry"/> which has the specified category.
		/// </summary>
		/// <param name="category"></param>
		/// <returns></returns>
		public HelpEntry[] GetHelpEntiresFromCategory(string category)
		{
			return _Source.Values.Where(x => x.Category.CaseInsEquals(category)).ToArray();
		}
		/// <summary>
		/// Returns an array of every command category.
		/// </summary>
		/// <returns></returns>
		public string[] GetCategories()
		{
			return _CategoryMap.Values.ToArray();
		}

		/// <summary>
		/// Attempt to get a command with its name.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public HelpEntry this[string name]
		{
			get => _NameMap.TryGetValue(name, out var n) ? _Source[n] : null;
		}
	}

	/// <summary>
	/// Holds information about a command, such as its name, aliases, usage, base permissions, description, category, and default enabled value.
	/// </summary>
	public class HelpEntry
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// How to use the command. This is automatically generated.
		/// </summary>
		public string Usage { get; }
		/// <summary>
		/// The base permissions to use the command.
		/// </summary>
		public string BasePerm { get; }
		/// <summary>
		/// Describes what the command does.
		/// </summary>
		public string Description { get; }
		/// <summary>
		/// Other names to invoke the command.
		/// </summary>
		public ImmutableList<string> Aliases { get; }
		/// <summary>
		/// The category the command is in.
		/// </summary>
		public string Category { get; }
		/// <summary>
		/// Whether or not the command is on by default.
		/// </summary>
		public bool DefaultEnabled { get; }
		/// <summary>
		/// Whether or not the command can be toggled.
		/// </summary>
		public bool AbleToBeToggled { get; }

		internal HelpEntry(string name, string usage, string basePerm, string description, string[] aliases,
			string category, bool defaultEnabled, bool ableToBeTurnedOff)
		{
			if (String.IsNullOrWhiteSpace(name))
			{
				throw new ArgumentException("cant be null or whitespace", nameof(name));
			}

			Name = name;
			Usage = usage ?? "";
			BasePerm = String.IsNullOrWhiteSpace(basePerm) ? "N/A" : basePerm;
			Description = String.IsNullOrWhiteSpace(description) ? "N/A" : description;
			Aliases = (aliases ?? new[] { "N/A" }).ToImmutableList();
			Category = category;
			DefaultEnabled = defaultEnabled;
			AbleToBeToggled = ableToBeTurnedOff;
		}

		/// <summary>
		/// Returns a string with all the information about the command.
		/// </summary>
		/// <returns></returns>
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
