using Advobot.Utilities;
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
			return GenerateUsage(command, c => c.IsNested, (c, l, i) => GetAllNestedClassesAndMethods(c, l, i));
		}
		/// <summary>
		/// Generates a string indicating how the command is used.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public static string GenerateUsage(ModuleInfo module)
		{
			return GenerateUsage(module, m => m.IsSubmodule, (m, l, i) => GetAllNestedClassesAndMethods(m, l, i));
		}
		private static string GenerateUsage<T>(T obj, Func<T, bool> nested, Action<T, MemberLists, int> fillLists)
		{
			if (nested(obj))
			{
				throw new ArgumentException("Only use this method on a non nested module.");
			}

			var lists = new MemberLists();
			fillLists(obj, lists, 0);
			lists.RemoveDuplicates();
			return CreateText(lists);
		}
		private static void GetAllNestedClassesAndMethods(ModuleInfo module, MemberLists lists, int deepness = 0)
		{
			foreach (var method in module.Commands)
			{
				if (method.Name != "Command")
				{
					var m = new MethodDetails(deepness, method.Name, method.Parameters.Count);
					lists.Methods.Add(m);
				}

				var parameters = method.Parameters;
				for (var i = 0; i < parameters.Count; ++i)
				{
					var pDeepness = (method.Name != "Command" ? 1 : 0) + deepness + i;
					var p = parameters[i];
					lists.Parameters.Add(new ParameterDetails(pDeepness, p));
				}
			}
			foreach (var nested in module.Submodules)
			{
				lists.Classes.Add(new ClassDetails(deepness, nested.Name));
				GetAllNestedClassesAndMethods(nested, lists, deepness + 1);
			}
		}
		private static void GetAllNestedClassesAndMethods(Type type, MemberLists lists, int deepness = 0)
		{
			var flags = BindingFlags.Instance | BindingFlags.Public;
			foreach (var method in type.GetMethods(flags).Where(x => x.GetCustomAttribute<CommandAttribute>() != null))
			{
				var m = new MethodDetails(deepness, method.GetCustomAttribute<CommandAttribute>().Text, method.GetParameters().Length);
				lists.Methods.Add(m);

				var parameters = method.GetParameters();
				for (var i = 0; i < parameters.Length; ++i)
				{
					var pDeepness = (m.Name != null ? 1 : 0) + deepness + i;
					var p = parameters[i];
					lists.Parameters.Add(new ParameterDetails(pDeepness, p));
				}
			}
			foreach (var nested in type.GetNestedTypes(flags).Where(x => x.GetCustomAttribute<GroupAttribute>() != null))
			{
				lists.Classes.Add(new ClassDetails(deepness, nested.GetCustomAttribute<GroupAttribute>().Prefix));
				GetAllNestedClassesAndMethods(nested, lists, deepness + 1);
			}
		}
		private static string CreateText(MemberLists lists)
		{
			//Don't include classes because they will always be 1 behind deepest methods at minimum.
			var upperBounds = new[]
			{
				lists.Methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				lists.Parameters.DefaultIfEmpty().Max(x => x?.Deepness ?? 0)
			};
			var maximumUpperBounds = upperBounds.Max(); //Highest of the two highs
			var minimumUpperBounds = upperBounds.Min(); //Lowest of the two highs

			var sb = new StringBuilder();
			for (var i = 0; i <= maximumUpperBounds; ++i)
			{
				//t = thisIteration
				var tClasses = lists.Classes.Where(x => x.Deepness == i).ToList();
				var tMethods = lists.Methods.Where(x => x.Deepness == i).ToList();
				var tParams = lists.Parameters.Where(x => x.Deepness == i).ToList();

				var optional = false;
				if (i >= minimumUpperBounds)
				{
					//Methods from before this iteration with no arguments means that anything after is optional
					var mNoName = lists.Methods.Where(x => x.Deepness <= i - minimumUpperBounds)
						.Any(x => x.Name == null && x.HasNoArgs);
					//Additional -1 to account for method names
					var mName = lists.Methods.Where(x => x.Deepness <= i - minimumUpperBounds - 1)
						.Any(x => x.Name != null && x.HasNoArgs);
					var m = mNoName || mName;

					//Any decrease in total arg count from an increment of i indicates that a command could
					//be used at position i - 1 with x args but at position i with x - y args
					//meaning that x - y args are optional
					var pCnt = lists.Parameters.Where(x => x.Deepness < i).GroupBy(x => x.Deepness)
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
					var bL = tParams.Any() && lists.Methods.Where(x => x.Deepness == i - 1).Any(x => x.Name != null && x.HasNoArgs);
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

		private sealed class MemberLists
		{
			public List<ClassDetails> Classes { get; private set; } = new List<ClassDetails>();
			public List<MethodDetails> Methods { get; private set; } = new List<MethodDetails>();
			public List<ParameterDetails> Parameters { get; private set; } = new List<ParameterDetails>();

			public void RemoveDuplicates()
			{
				RemoveDuplicateClasses();
				RemoveDuplicateMethods();
				RemoveDuplicateParameters();
			}
			private void RemoveDuplicateClasses()
			{
				Classes = Classes
					.GroupBy(x => new { x.Name, x.Deepness })
					.Select(g => g.First()).ToList();
			}
			private void RemoveDuplicateMethods()
			{
				Methods = Methods
					.GroupBy(x => new { x.Name, x.Deepness })
					.Select(g => g.OrderByDescending(x => x.ArgCount).First()).ToList();
			}
			private void RemoveDuplicateParameters()
			{
				Parameters = Parameters
					.GroupBy(x => new { x.Name, x.Deepness })
					.Select(g =>
					{
						var param = g.First();
						param.SetOccurences(g.Count());
						return param;
					}).ToList();
			}
		}
	}
}