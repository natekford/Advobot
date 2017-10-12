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
		 *		public async Task BAsync(string text, uint dog)
		 *		{
		 *			...
		 *		}
		 *		[Command()]
		 *		public async Task CAsync(int cat)
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
			RemoveDuplicateMethods(ref _Methods);
			RemoveDuplicateParameters(ref _Params);

			//Don't include classes because they will always be 1 behind deepest methods at minimum.
			var maximumUpperBounds = GetMaximumUpperBounds(_Methods, _Params); //Highest of the two highs
			var minimumUpperBounds = GetMinimumUpperBounds(_Methods, _Params); //Lowest of the two highs
			for (int i = 0; i <= maximumUpperBounds; ++i)
			{
				//t = thisIteration
				var tClasses = _Classes.Where(x => x.Deepness == i);
				var tMethods = _Methods.Where(x => x.Deepness == i);
				var tParams = _Params.Where(x => x.Deepness == i);

				var optional = false;
				if (i >= minimumUpperBounds)
				{
					//Methods from before this iteration with no arguments means that anything after is optional
					var mNoName = _Methods.Where(x => x.Deepness <= i - minimumUpperBounds)
						.Any(x => x.Name == null && x.HasNoArgs);
					//Additional -1 to account for method names
					var mName = _Methods.Where(x => x.Deepness <= i - minimumUpperBounds - 1)
						.Any(x => x.Name != null && x.HasNoArgs);
					var m = mNoName || mName;

					//Any decrease in total arg count from an increment of i indicates that a command could
					//be used at position i - 1 with x args but at position i with x - y args
					//meaning that x - y args are optional
					var pCnt = _Params.Where(x => x.Deepness < i).GroupBy(x => x.Deepness)
						.Select(x => x.Sum(y => y.Occurences)).DefaultIfEmpty(0)
						.Min() > tParams.Sum(x => x.Occurences);
					//Parameters marked with the optional attribute are optional
					var pOpt = tParams.Any(x => x.IsOptional);
					var p = pCnt || pOpt;

					//Both methods and params needed because if a method has no args and no name it means it
					//needs nothing more to fire but if there are args that means the args could be said instead
					//and thus are optional
					var bT = tParams.Any() && tMethods.Any(x => x.Name == null && x.HasNoArgs);
					//Additional -1 to account for method names
					var bL = tParams.Any() && _Methods.Where(x => x.Deepness == i - 1).Any(x => x.Name != null && x.HasNoArgs);
					var b = bT || bL;

					optional = m || p || b;
				}

				if (tClasses.Any() || tMethods.Any(x => x.Name != null) || tParams.Any())
				{
					StartArgument(sb, optional);
					AddOptions(sb, tClasses);
					AddOptions(sb, tMethods);
					AddOptions(sb, tParams);
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
				methods.Add(m);

				var weNeedToGoDeeper = m.Name != null ? 1 : 0;

				var p = method.GetParameters();
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

		private void RemoveDuplicateMethods(ref List<MethodDetails> methods)
		{
			var tempList = new List<MethodDetails>();
			foreach (var method in methods)
			{
				//Don't allow duplicate methods with the same name and deepness
				//Different deepnesses are find though, because they help affect things being marked
				//as optional while they won't be printed out usually because they have a null name
				var matchingNameAndDeepness = tempList.SingleOrDefault(x => x.Name == method.Name && x.Deepness == method.Deepness);
				if (matchingNameAndDeepness == null)
				{
					tempList.Add(method);
				}
				else if (matchingNameAndDeepness != null && matchingNameAndDeepness.ArgCount > method.ArgCount)
				{
					tempList.Remove(matchingNameAndDeepness);
					tempList.Add(method);
				}
			}
			methods = tempList;
		}
		private void RemoveDuplicateParameters(ref List<ParameterDetails> parameters)
		{
			var deepest = parameters.Where(x => !x.IsRemainder).DefaultIfEmpty().Max(x => x?.Deepness ?? 0);
			foreach (var parameter in parameters.Where(x => x.IsRemainder))
			{
				parameter.SetDeepness(deepest + 1);
			}

			var tempList = new List<ParameterDetails>();
			foreach (var parameter in parameters)
			{
				var matchingNameAndDeepness = tempList.SingleOrDefault(x => x.Name == parameter.Name && x.Deepness == parameter.Deepness);
				if (matchingNameAndDeepness != null)
				{
					matchingNameAndDeepness.IncrementOccurences();
				}
				else
				{
					tempList.Add(parameter);
				}
			}
			parameters = tempList;
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