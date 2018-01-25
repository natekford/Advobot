using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Enums;
using Advobot.Core.Utilities;

namespace Advobot.Core.Classes.NamedArguments
{
	/// <summary>
	/// Allows named arguments to be used via an overly complex system of attributes and reflection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NamedArguments<T> where T : new()
	{
		public static ImmutableList<string> ArgNames { get; }

		private static ConstructorInfo _Constructor;
		private static bool _HasParams;
		private static int _ParamsLength;
		private static string _ParamsName;

		private Dictionary<string, string> _Args = ArgNames.ToDictionary(x => x, x => default(string), StringComparer.OrdinalIgnoreCase);
		private List<string> _ParamArgs = new List<string>();

		/// <summary>
		/// Sets the constructor, argnames, and params information.
		/// </summary>
		static NamedArguments()
		{
			_Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
				.Single(x => x.GetCustomAttribute<NamedArgumentConstructorAttribute>() != null);

			//Make sure no invalid types have CustomArgumentAttribute
			var argNames = new List<string>();
			foreach (var p in _Constructor.GetParameters())
			{
				var customArgumentAttr = p.GetCustomAttribute<NamedArgumentAttribute>();
				if (customArgumentAttr == null)
				{
					continue;
				}

				var t = p.ParameterType;
				t = t.IsArray ? t.GetElementType() : t; //To allow arrays
				t = Nullable.GetUnderlyingType(t) ?? t; //To allow nullables

				//Only allow primitives, enums, and string
				if (!t.IsPrimitive && !t.IsEnum && t != typeof(string))
				{
					throw new ArgumentException("don't use on anything other than primitives/enums/strings", nameof(NamedArgumentAttribute));
				}

				if (argNames.CaseInsContains(p.Name))
				{
					throw new InvalidOperationException("Argument names are case insensitive, so don't have duplicates.");
				}

				if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
				{
					_HasParams = true;
					_ParamsLength = customArgumentAttr.Length > 0 ? customArgumentAttr.Length : int.MaxValue;
					_ParamsName = p.Name;
				}

				//Let params name fall down to here so that it can be shown when ArgNames gets accessed by UsageGenerator
				argNames.Add(p.Name);
			}
			ArgNames = argNames.ToImmutableList();
		}
		/// <summary>
		/// Splits the input by spaces except when in quotes then attempts to fit them into a dictionary of arguments
		/// or a list of params arguments.
		/// </summary>
		/// <param name="input"></param>
		public NamedArguments(string input)
		{
			//Split by spaces except when in quotes
			var split = input.Split('"').Select((x, index) =>
			{
				return index % 2 == 0
					? x.Split(' ')
					: new[] { x };
			}).SelectMany(x => x).Where(x => !String.IsNullOrWhiteSpace(x));

			//Split by colons. Left half is key, right half is value.
			var argKvps = split.Select(x =>
			{
				var kvp = x?.Split(new[] { ':' }, 2);
				return kvp?.Length == 2
					? (Key: kvp[0], Value: kvp[1])
					: (Key: null, Value: null);
			}).Where(x => x.Key != null && x.Value != null);

			foreach (var (key, value) in argKvps)
			{
				//If the params name is the arg name then add to params
				if (_HasParams && _ParamsName.CaseInsEquals(key))
				{
					//Keep this inside here so that if there are too many
					//params args it won't potentially go into the else if
					if (_ParamArgs.Count() < _ParamsLength)
					{
						_ParamArgs.Add(value);
					}
				}
				//If the args dictionary has the value as a key, set it.
				else if (_Args.ContainsKey(key))
				{
					_Args[key] = value;
				}
			}
		}

		/// <summary>
		/// Creates whatever <see cref="T"/> is with the gathered arguments.
		/// <paramref name="additionalArgs"/> 
		/// </summary>
		/// <param name="additionalArgs"></param>
		/// <returns></returns>
		public T CreateObject(params object[] additionalArgs)
		{
			var additionalArgsList = new List<object>(additionalArgs);
			var parameters = _Constructor.GetParameters().Select(p =>
			{
				//For arrays get the underlying type
				var t = p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType;

				//Check params first otherwise will go into the middle else if
				if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
				{
					//Convert all from string to whatever type they need to be
					//NEEDS TO BE AN ARRAY SINCE PARAMS IS AN ARRAY!
					var convertedArgs = _ParamArgs.Select(x => NamedArgumentsUtils.ConvertValue(t, x)).ToArray();
					//Have to use this method otherwise create instance throws exception
					//because this will send object[] instead of T[] when empty
					if (!convertedArgs.Any())
					{
						return Array.CreateInstance(t, 0);
					}

					//Have to use create instance here so the array will be the correct type
					//Wish there was a better way to cast the array but not sure
					var correctTypeArray = Array.CreateInstance(t, convertedArgs.Length);
					Array.Copy(convertedArgs, correctTypeArray, convertedArgs.Length);
					return correctTypeArray;
				}
				//Checking against the attribute again in case arguments have duplicate names

				if (p.GetCustomAttribute<NamedArgumentAttribute>() != null && _Args.TryGetValue(p.Name, out var arg))
				{
					return NamedArgumentsUtils.ConvertValue(t, arg);
				}
				//Finally see if any additional args should be used

				if (additionalArgsList.Any())
				{
					var value = additionalArgsList.FirstOrDefault(x => x.GetType() == t);
					if (value != null)
					{
						additionalArgsList.Remove(value);
						return value;
					}
				}
				return NamedArgumentsUtils.CreateDefault(t);
			}).ToArray();

			try
			{
				return (T)_Constructor.Invoke(parameters);
			}
			catch (MissingMethodException e)
			{
				e.Write();
				return new T();
			}
		}
	}

	public static class NamedArgumentsUtils
	{
		/// <summary>
		/// Converts the string into the given <paramref name="type"/>.
		/// Null or whitespace strings will return default values.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object ConvertValue(Type type, string value)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				//If the type is nullable and value is null then allowed to return null, otherwise try to make not null 
				return Nullable.GetUnderlyingType(type) != null ? null : CreateDefault(type);
			}

			var t = Nullable.GetUnderlyingType(type) ?? type;
			//If the type is an enum see if it's a valid name. If invalid then return default
			if (t.IsEnum)
			{
				return ConvertEnum(t, value);
			}

			//Converters should work for primitives. Not sure what else it works for.
			var converter = TypeDescriptor.GetConverter(t);
			//I think ConvertFromInvariantString works with commas, but only if the computer's culture is set to one that uses it. 
			//Can't really test that easily because I CBA to switch my computer's language.
			if (converter != null && converter.IsValid(value))
			{
				return converter.ConvertFromInvariantString(value);
			}

			//If there's only one constructor that accepts strings, use that one
			//Method signatures mean there will only be one constructor that accepts 1 string if it does exist
			var constructor = t.GetConstructors().SingleOrDefault(x =>
			{
				var parameters = x.GetParameters();
				return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
			});
			return constructor != null ? constructor.Invoke(new object[] { value }) : CreateDefault(t);
		}
		/// <summary>
		/// Value types and classes with parameterless constructors can be created with no parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="createParameterless"></param>
		/// <returns></returns>
		public static object CreateDefault(Type type, bool createParameterless = false)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			if (createParameterless)
			{
				var constructor = type.GetConstructors().SingleOrDefault(x => !x.GetParameters().Any());
				if (constructor != null)
				{
					return constructor.Invoke(Array.Empty<object>());
				}
			}
			return null;
		}
		/// <summary>
		/// Converts a string to an enum value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object ConvertEnum(Type type, string value)
		{
			if (type.GetCustomAttribute<FlagsAttribute>() == null)
			{
				return Enum.IsDefined(type, value)
					? Enum.Parse(type, value, true)
					: Activator.CreateInstance(type);
			}

			//Allow people to 'OR' things together (kind of)
			var e = (uint)Activator.CreateInstance(type);
			foreach (var s in value.Split('|'))
			{
				if (Enum.IsDefined(type, s))
				{
					e |= (uint)Enum.Parse(type, s, true);
				}
			}
			return Enum.ToObject(type, e);
		}
		/// <summary>
		/// Returns objects where the function does not return null and is either equal to, less than, or greater than a specified number.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="objects"></param>
		/// <param name="target"></param>
		/// <param name="count"></param>
		/// <param name="f"></param>
		/// <returns></returns>
		public static IEnumerable<T> GetObjectsBasedOffCount<T>(this IEnumerable<T> objects, CountTarget target, uint? count, Func<T, int?> f)
		{
			switch (target)
			{
				case CountTarget.Equal:
					objects = objects.Where(x => { var val = f(x); return val != null && val == count; });
					break;
				case CountTarget.Below:
					objects = objects.Where(x => { var val = f(x); return val != null && val < count; });
					break;
				case CountTarget.Above:
					objects = objects.Where(x => { var val = f(x); return val != null && val > count; });
					break;
			}
			return objects;
		}
	}
}
