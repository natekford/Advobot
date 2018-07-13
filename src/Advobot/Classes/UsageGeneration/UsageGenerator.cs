using AdvorangesUtils;
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
	public static class UsageGenerator
	{
		/* Example:
		 * public class Top
		 * {
		 *		[Group("a")]
		 *		public class Nested
		 *		{
		 *			[Command("q")]
		 *			public async Task QAsync()
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

		/// <summary>
		/// Generates a string indicating how the command is used.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public static string GenerateUsage(Type command)
		{
			if (command.IsNested)
			{
				throw new ArgumentException("only use this method on a non nested class", nameof(command));
			}

			var classes = new List<ClassDetails>();
			var methods = new List<MethodDetails>();
			var parameters = new List<ParameterDetails>();
			GetAllNestedClassesAndMethods(command, classes, methods, parameters);
			RemoveDuplicateClasses(ref classes);
			RemoveDuplicateMethods(ref methods);
			RemoveDuplicateParameters(ref parameters);

			return CreateText(classes, methods, parameters);
		}
		private static void GetAllNestedClassesAndMethods(Type command, List<ClassDetails> classes, List<MethodDetails> methods, List<ParameterDetails> parameters, int deepness = 0)
		{
			foreach (var method in GetCommands(command))
			{
				var m = new MethodDetails(deepness, method);
				methods.Add(m);

				var p = method.GetParameters();
				for (var i = 0; i < p.Length; ++i)
				{
					parameters.Add(new ParameterDetails((m.Name != null ? 1 : 0) + deepness + i, p[i]));
				}
			}

			foreach (var type in GetNestedCommandClasses(command))
			{
				classes.Add(new ClassDetails(deepness, type));
				GetAllNestedClassesAndMethods(type, classes, methods, parameters, deepness + 1);
			}
		}
		private static IEnumerable<Type> GetNestedCommandClasses(Type command)
		{
			return command.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<GroupAttribute>() != null);
		}
		private static IEnumerable<MethodInfo> GetCommands(Type command)
		{
			return command.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<CommandAttribute>() != null);
		}
		private static void RemoveDuplicateClasses(ref List<ClassDetails> classes)
		{
			classes = classes
				.GroupBy(x => new { x.Name, x.Deepness })
				.Select(g => g.First()).ToList();
		}
		private static void RemoveDuplicateMethods(ref List<MethodDetails> methods)
		{
			methods = methods
				.GroupBy(x => new { x.Name, x.Deepness })
				.Select(g => g.OrderByDescending(x => x.ArgCount).First()).ToList();
		}
		private static void RemoveDuplicateParameters(ref List<ParameterDetails> parameters)
		{
			parameters = parameters
				.GroupBy(x => new { x.Name, x.Deepness })
				.Select(g =>
				{
					var param = g.First();
					param.SetOccurences(g.Count());
					return param;
				}).ToList();
		}
		private static string CreateText(List<ClassDetails> classes, List<MethodDetails> methods, List<ParameterDetails> parameters)
		{
			//Don't include classes because they will always be 1 behind deepest methods at minimum.
			var upperBounds = new[]
			{
				methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				parameters.DefaultIfEmpty().Max(x => x?.Deepness ?? 0)
			};
			var maximumUpperBounds = upperBounds.Max(); //Highest of the two highs
			var minimumUpperBounds = upperBounds.Min(); //Lowest of the two highs

			var sb = new StringBuilder();
			for (var i = 0; i <= maximumUpperBounds; ++i)
			{
				//t = thisIteration
				var tClasses = classes.Where(x => x.Deepness == i).ToList();
				var tMethods = methods.Where(x => x.Deepness == i).ToList();
				var tParams = parameters.Where(x => x.Deepness == i).ToList();

				var optional = false;
				if (i >= minimumUpperBounds)
				{
					//Methods from before this iteration with no arguments means that anything after is optional
					var mNoName = methods.Where(x => x.Deepness <= i - minimumUpperBounds)
						.Any(x => x.Name == null && x.HasNoArgs);
					//Additional -1 to account for method names
					var mName = methods.Where(x => x.Deepness <= i - minimumUpperBounds - 1)
						.Any(x => x.Name != null && x.HasNoArgs);
					var m = mNoName || mName;

					//Any decrease in total arg count from an increment of i indicates that a command could
					//be used at position i - 1 with x args but at position i with x - y args
					//meaning that x - y args are optional
					var pCnt = parameters.Where(x => x.Deepness < i).GroupBy(x => x.Deepness)
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
					var bL = tParams.Any() && methods.Where(x => x.Deepness == i - 1).Any(x => x.Name != null && x.HasNoArgs);
					var b = bT || bL;

					optional = m || p || b;
				}

				if (tClasses.Any() || tMethods.Any(x => x.Name != null) || tParams.Any())
				{
					tClasses.RemoveAll(c => tMethods.Any(m => m.Name.CaseInsEquals(c.Name)));

					while (sb.Length > 0 && sb[0] == '|')
					{
						sb.Remove(1, sb.Length - 2);
					}
					sb.Append(optional ? "<" : "[");

					AddOptions(sb, tClasses);
					AddOptions(sb, tMethods);
					AddOptions(sb, tParams);

					while (sb.Length > 0 && sb[sb.Length - 1] == '|')
					{
						sb.Remove(sb.Length - 1, 1);
					}
					sb.Append(optional ? "> " : "] ");
				}
			}

			return sb.ToString().Trim();
		}
		private static void AddOptions<T>(StringBuilder sb, IEnumerable<T> options)
		{
			var converted = options.Select(x => x.ToString()).Where(x => !String.IsNullOrWhiteSpace(x));
			var addOrToEnd = converted.Any(x => !String.IsNullOrWhiteSpace(x)) ? "|" : "";
			sb.Append(converted.JoinNonNullStrings("|") + addOrToEnd);
		}
	}
}