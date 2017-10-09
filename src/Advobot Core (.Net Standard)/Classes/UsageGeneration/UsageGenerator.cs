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

			GetAllNestedClassesAndMethods(0, classType, _Classes, _Methods, _Params);

			//Remove duplicates
			_Classes = _Classes.GroupBy(x => x.Name).Select(x => x.First()).ToList();
			_Methods = _Methods.GroupBy(x => x.Name).Select(x => x.First()).ToList();
			_Params = _Params.GroupBy(x => x.Name).Select(x => x.First()).ToList();

			var sb = new StringBuilder();
			for (int i = 0; i <= GetMaxDeepness(_Classes, _Methods, _Params); ++i)
			{
				var thisIterClasses = _Classes.Where(x => x.Deepness == i);
				var thisIterMethods = _Methods.Where(x => x.Deepness == i);
				var thisIterParams = _Params.Where(x => x.Deepness == i);

				var optional = false;
				if (i >= GetMinimumUpperBounds(_Classes, _Methods, _Params))
				{
					//Optional methods are when they have no name, or when they don't go deep enough
					//Optional parameters are easy to know. They just have the optional attribute.
					var m = (_Methods.Any() && !_Methods.Any(x => x.Deepness >= i)) ||
							(thisIterMethods.Any(x => x.Name == null) && !thisIterMethods.All(x => x.Name == null));
					var p = thisIterParams.Any() && thisIterParams.All(x => x.Optional);
					optional = m || p;
				}

				StartArgument(sb, optional);
				AddOptions(sb, thisIterClasses);
				AddOptions(sb, thisIterMethods);
				AddOptions(sb, thisIterParams);
				CloseArgument(sb, optional);
			}

			Text = sb.ToString().Trim();
		}

		private void GetAllNestedClassesAndMethods(int deepness, Type classType, List<ClassDetails> classes, List<MethodDetails> methods, List<ParameterDetails> parameters)
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
				GetAllNestedClassesAndMethods(deepness + 1, type, classes, methods, parameters);
				classes.Add(new ClassDetails(deepness, type));
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

		private int GetMaxDeepness(IEnumerable<ClassDetails> classes, IEnumerable<MethodDetails> methods, IEnumerable<ParameterDetails> parameters)
		{
			return new[]
			{
				methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				//Don't include classes because they will always be 1 behind deepest methods at minimum.
				//classes.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				parameters.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
			}.Max();
		}
		private int GetMinimumUpperBounds(IEnumerable<ClassDetails> classes, IEnumerable<MethodDetails> methods, IEnumerable<ParameterDetails> parameters)
		{
			return new[]
			{
				methods.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
				//Don't include classes because they will always be 1 behind deepest methods at minimum.
				//classes.DefaultIfEmpty().Max(x => x?.Deepness ?? 0),
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