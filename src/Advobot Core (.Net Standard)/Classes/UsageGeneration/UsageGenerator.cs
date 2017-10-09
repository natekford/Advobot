using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Uses reflection to generate a string which explains how to use a command.
	/// </summary>
	public class UsageGenerator
	{
		/* Example:
		 * public class Top
		 * {
		 *		[Group("a")]
		 *		public class Nested
		 *		{
		 *			[Command("q")]
		 *			{
		 *				...
		 *			}
		 *		}
		 *		[Command("b")]
		 *		public async Task B(string text, uint dog)
		 *		{
		 *			...
		 *		}
		 *		[Command()]
		 *		public async Task C(int cat)
		 *		{
		 *			...
		 *		}
		 * }
		 * Gets formatted to [A|B|cat] <q|text> <dog>
		 */

		public string Text { get; }
		private List<ClassDetails> _Classes = new List<ClassDetails>();
		private List<MethodDetails> _Methods = new List<MethodDetails>();
		private List<ParameterDetails> _Params = new List<ParameterDetails>();

		public UsageGenerator(Type classType)
		{
			if (classType.IsNested)
			{
				throw new ArgumentException("Only use this method on a non nested class.");
			}

			var sb = new StringBuilder();
			GetAllNestedClassesAndMethods(classType, _Classes, _Methods, _Params);
			RemoveDuplicateClasses(ref _Classes);
			RemoveDuplicateMethods(ref _Methods);
			RemoveDuplicateParameters(ref _Params);

			//Don't include classes because they will always be 1 behind deepest methods at minimum.
			var maximumUpperBounds = GetMaximumUpperBounds(_Methods, _Params); //Highest of the two highs
			var minimumUpperBounds = GetMinimumUpperBounds(_Methods, _Params); //Lowest of the two highs
			for (int i = 0; i <= maximumUpperBounds; ++i)
			{
				var thisIterClasses = _Classes.Where(x => x.Deepness == i);
				var thisIterMethods = _Methods.Where(x => x.Deepness == i);
				var thisIterParams = _Params.Where(x => x.Deepness == i);

				var optional = false;
				if (i >= minimumUpperBounds)
				{
					//Any methods from before this iteration with no arguments means that anything after is optional
					var m = _Methods.Where(x => x.Deepness <= i - minimumUpperBounds).Any(x => x.NoArgs);
					//Any parameters marked with the optional attr are optional.
					var p = thisIterParams.Any() && thisIterParams.All(x => x.Optional);
					optional =  m || p;
				}

				if (thisIterClasses.Any() || thisIterMethods.Any(x => x.Name != null) || thisIterParams.Any())
				{
					StartArgument(sb, optional);
					AddOptions(sb, thisIterClasses);
					AddOptions(sb, thisIterMethods);
					AddOptions(sb, thisIterParams);
					CloseArgument(sb, optional);
				}
			}

			Text = sb.ToString().Trim();
		}

		private void GetAllNestedClassesAndMethods(Type classType, List<ClassDetails> classes, List<MethodDetails> methods, List<ParameterDetails> parameters, int deepness = 0)
		{
			foreach (var method in GetCommands(classType))
			{
				var m = new MethodDetails(deepness, method);
				var p = method.GetParameters();

				//If the name isn't null the method has to be added since it's necessary to invoke a command
				//If the name is null, check if it has any arguments.
				//If none then it has to be added here to specify no further text is needed (it's optional)
				//If there are some then they get added in as parameter details (it's not optional)
				if (m.Name != null || (m.Name == null && !p.Any()))
				{
					methods.Add(m);
				}

				var weNeedToGoDeeper = m.Name != null ? 1 : 0;
				for (int i = 0; i < p.Length; ++i)
				{
					parameters.Add(new ParameterDetails(weNeedToGoDeeper + deepness + i, p[i]));
				}
			}

			foreach (var type in GetNestedCommandClasses(classType))
			{
				classes.Add(new ClassDetails(deepness, type));
				GetAllNestedClassesAndMethods(type, classes, methods, parameters, deepness + 1);
			}
		}
		private IEnumerable<Type> GetNestedCommandClasses(Type classType)
		{
			return classType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<GroupAttribute>() != null);
		}
		private IEnumerable<MethodInfo> GetCommands(Type classType)
		{
			return classType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<CommandAttribute>() != null);
		}

		private void RemoveDuplicateClasses(ref List<ClassDetails> classes)
		{
			classes = classes.GroupBy(x => x.Name).Select(x => x.First()).ToList();
		}
		private void RemoveDuplicateMethods(ref List<MethodDetails> methods)
		{
			var tempList = new List<MethodDetails>();
			foreach (var method in methods)
			{
				var matchingNameAndDeepness = tempList.SingleOrDefault(x => x.Name == method.Name && x.Deepness == method.Deepness);
				if (matchingNameAndDeepness != null && !matchingNameAndDeepness.NoArgs)
				{
					tempList.Remove(matchingNameAndDeepness);
					tempList.Add(method);
				}
				else if (!tempList.Any(x => x.Name == method.Name))
				{
					tempList.Add(method);
				}
			}
			methods = tempList;
		}
		private void RemoveDuplicateParameters(ref List<ParameterDetails> parameters)
		{
			parameters = parameters.GroupBy(x => x.Name).Select(x => x.First()).ToList();
		}

		private int GetMaximumUpperBounds(IEnumerable<MethodDetails> methods, IEnumerable<ParameterDetails> parameters)
		{
			return new[]
			{
				methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				parameters.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
			}.Max();
		}
		private int GetMinimumUpperBounds(IEnumerable<MethodDetails> methods, IEnumerable<ParameterDetails> parameters)
		{
			return new[]
			{
				methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				parameters.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
			}.Min();
		}

		private void StartArgument(StringBuilder sb, bool optional)
		{
			while (sb.Length > 0 && sb[0] == '|')
			{
				sb.Remove(1, sb.Length - 2);
			}

			sb.Append(optional ? "<" : "[");
		}
		private void CloseArgument(StringBuilder sb, bool optional)
		{
			while (sb.Length > 0 && sb[sb.Length - 1] == '|')
			{
				sb.Remove(sb.Length - 1, 1);
			}

			sb.Append(optional ? "> " : "] ");
		}
		private void AddOptions<T>(StringBuilder sb, IEnumerable<T> options) where T : IArgument
		{
			var converted = options.Where(x => !String.IsNullOrWhiteSpace(x?.Name)).Select(x => x.ToString());
			var addOrToEnd = converted.Any(x => !String.IsNullOrWhiteSpace(x)) ? "|" : "";
			sb.Append(GeneralFormatting.JoinNonNullStrings("|", converted.ToArray()) + addOrToEnd);
		}
	}
}