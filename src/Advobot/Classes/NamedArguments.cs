using Advobot.Classes.Attributes;
using AdvorangesUtils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Advobot.Classes
{
	/// <summary>
	/// Allows named arguments to be used via an overly complex system of attributes and reflection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public sealed class NamedArguments<T> where T : class
	{
		/// <summary>
		/// The argument names for the specified generic parameter.
		/// </summary>
		public static ImmutableList<string> ArgNames { get; }

		private static readonly ConstructorInfo _Constructor;
		private static readonly bool _HasParams;
		private static readonly int _ParamsLength;
		private static readonly string _ParamsName;

		private readonly Dictionary<string, string> _Args;
		private readonly List<string> _ParamArgs;

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
			_Args = ArgNames.ToDictionary(x => x, x => default(string), StringComparer.OrdinalIgnoreCase);
			_ParamArgs = new List<string>();

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
		/// Creates whatever the generic parameter is with the gathered arguments.
		/// <paramref name="additionalArgs"/> 
		/// </summary>
		/// <param name="additionalArgs"></param>
		/// <param name="obj"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		public bool TryCreateObject(IEnumerable<object> additionalArgs, out T obj, out Error error)
		{
			var additionalArgsList = new List<object>(additionalArgs);
			try
			{
				var parameters = _Constructor.GetParameters().Select(p => GetValueForParameter(p, additionalArgsList)).ToArray();
				obj = (T)_Constructor.Invoke(parameters);
				error = null;
				return true;
			}
			catch (Exception e)
			{
				obj = null;
				error = new Error(e);
				return false;
			}
		}
		/// <summary>
		/// Attempts to find or create the value for a parameter in a constructor.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="additionalArgs"></param>
		/// <returns></returns>
		private object GetValueForParameter(ParameterInfo p, List<object> additionalArgs)
		{
			//For arrays get the underlying type
			var t = p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType;

			//Check params first otherwise will go into the middle else if
			if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
			{
				//Convert all from string to whatever type they need to be
				//NEEDS TO BE AN ARRAY SINCE PARAMS IS AN ARRAY!
				var convertedArgs = _ParamArgs.Select(x => ConvertValue(t, x)).ToArray();
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
				return ConvertValue(t, arg);
			}
			//Finally see if any additional args should be used
			if (additionalArgs.Any())
			{
				var value = additionalArgs[0];
				additionalArgs.RemoveAt(0);
				return value;
			}
			return CreateDefault(t);
		}
		/// <summary>
		/// Converts the string into the given <paramref name="type"/>.
		/// Null or whitespace strings will return default values.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object ConvertValue(Type type, string value)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				//If the type is nullable and value is null then allowed to return null, otherwise try to make not null 
				return Nullable.GetUnderlyingType(type) != null ? null : CreateDefault(type);
			}

			type = Nullable.GetUnderlyingType(type) ?? type;
			//If the type is an enum see if it's a valid name. If invalid then return default
			if (type.IsEnum)
			{
				return CreateEnum(type, value);
			}

			//I think ConvertFromInvariantString works with commas, but only if the computer's culture is set to one that uses it. 
			//Can't really test that easily because I CBA to switch my computer's language.
			if (TypeDescriptor.GetConverter(type) is TypeConverter converter && converter.IsValid(value))
			{
				return converter.ConvertFromInvariantString(value);
			}

			//If there's only one constructor that accepts strings, use that one
			//Method signatures mean there will only be one constructor that accepts 1 string if it does exist
			return type.GetConstructors().SingleOrDefault(x =>
			{
				var parameters = x.GetParameters();
				return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
			})?.Invoke(new object[] { value }) ?? CreateDefault(type);
		}
		/// <summary>
		/// Value types and classes with parameterless constructors can be created with no parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="createParameterless"></param>
		/// <returns></returns>
		private object CreateDefault(Type type, bool createParameterless = false)
		{
			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			if (createParameterless)
			{
				return type.GetConstructors().SingleOrDefault(x => !x.GetParameters().Any())?.Invoke(new object[0]);
			}
			return null;
		}
		/// <summary>
		/// Creates an enum from the supplied string. Attempts to parse first regularly, then attempts to parse as flags.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object CreateEnum(Type type, string value)
		{
			if (type.GetCustomAttribute<FlagsAttribute>() == null)
			{
				return Enum.IsDefined(type, value) ? Enum.Parse(type, value, true) : Activator.CreateInstance(type);
			}

			//Allow people to 'OR' things together (kind of)
			var e = (ulong)Activator.CreateInstance(type);
			foreach (var s in value.Split('|'))
			{
				if (Enum.IsDefined(type, s))
				{
					e |= (ulong)Enum.Parse(type, s, true);
				}
			}
			return Enum.ToObject(type, e);
		}
	}
}
