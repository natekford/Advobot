using Advobot.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Advobot.Classes
{
	public class CustomArguments<T> where T : class
	{
		public static ImmutableList<string> ArgNames { get; }

		private static ConstructorInfo _Constructor;
		private static bool _HasParams;
		private static int _ParamsLength;
		private static string _ParamsName;

		private Dictionary<string, string> _Args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private List<string> _ParamArgs = new List<string>();

		static CustomArguments()
		{
			_Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Single(x => x.GetCustomAttribute<CustomArgumentConstructorAttribute>() != null);

			//Make sure no invalid types have CustomArgumentAttribute
			var argNames = new List<string>();
			foreach (var p in _Constructor.GetParameters())
			{
				if (p.GetCustomAttribute<CustomArgumentAttribute>() != null)
				{
					var t = p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType;
					if (!t.IsPrimitive && t != typeof(string))
					{
						throw new ArgumentException($"Do not use {nameof(CustomArgumentAttribute)} on non primitive arguments.");
					}
					else if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
					{
						_HasParams = true;
						_ParamsLength = p.GetCustomAttribute<CustomArgumentAttribute>().Length;
						_ParamsName = p.Name;
					}

					argNames.Add(p.Name);
				}
			}
			ArgNames = argNames.ToImmutableList();
		}

		public CustomArguments(string input)
		{
			ArgNames.ForEach(x => _Args.Add(x, null));

			//Split except when in quotes
			var args = input.Split('"')
			.Select((x, index) =>
			{
				return index % 2 == 0
					? x.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
					: new[] { x };
			}).SelectMany(x => x)
			.Select(x =>
			{
				var split = x?.Split(new[] { ':' }, 2);
				return split.Length == 2 ? (Key: split[0], Value: split[1]) : (Key: null, Value: null);
			}).Where(x => x.Key != null && x.Value != null);

			//If the _Args dictionary has the first half of the split, set it to the second half
			//Otherwise don't do anything with it since it's not important
			foreach (var arg in args)
			{
				if (_HasParams && _ParamsName.CaseInsEquals(arg.Key) && _ParamArgs.Count() < _ParamsLength)
				{
					_ParamArgs.Add(arg.Value);
				}
				else if (_Args.ContainsKey(arg.Key))
				{
					_Args[arg.Key] = arg.Value;
				}
			}
		}

		public T CreateObject(params object[] additionalArgs)
		{
			var additionalArgCounter = 0;
			var parameters = _Constructor.GetParameters().Select(p =>
			{
				var t = p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType;
				//Check params first otherwise will go into the middle else if
				if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
				{
					//Convert all from string to whatever type they need to be
					var temp = new List<object>();
					foreach (var arg in _ParamArgs)
					{
						temp.Add(Convert.ChangeType(arg, t, CultureInfo.InvariantCulture));
					}
					return temp.ToArray();
				}
				//Checking against the attribute again in case arguments have duplicate names
				else if (p.GetCustomAttribute<CustomArgumentAttribute>() != null && _Args.TryGetValue(p.Name, out string value))
				{
					return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
				}
				else if (additionalArgCounter < additionalArgs.Length)
				{
					//Increment the counter but also subtract 1 to get the current counter
					return additionalArgs[++additionalArgCounter - 1];
				}

				return t.IsValueType ? Activator.CreateInstance(t) : null;
			}).ToArray();

			return (T)Activator.CreateInstance(typeof(T), parameters);
		}
	}
}
