using Advobot.Core.Utilities;
using Advobot.Core.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Allows named arguments to be used via an overly complex system of attributes and reflection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class NamedArguments<T> where T : class
	{
		public static ImmutableList<string> ArgNames { get; }

		private static ConstructorInfo _Constructor;
		private static bool _HasParams;
		private static int _ParamsLength;
		private static string _ParamsName;

		private Dictionary<string, string> _Args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		private List<string> _ParamArgs = new List<string>();

		/// <summary>
		/// Sets the constructor, argnames, and params information.
		/// </summary>
		static NamedArguments()
		{
			_Constructor = typeof(T).GetConstructors(BindingFlags.Public | BindingFlags.Instance)
				.Single(x => x.GetCustomAttribute<CustomArgumentConstructorAttribute>() != null);

			//Make sure no invalid types have CustomArgumentAttribute
			var argNames = new List<string>();
			foreach (var p in _Constructor.GetParameters())
			{
				var customArgumentAttr = p.GetCustomAttribute<CustomArgumentAttribute>();
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
					throw new ArgumentException($"Do not use {nameof(CustomArgumentAttribute)} on anything other than primitives, enums, and strings.");
				}
				else if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
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
			ArgNames.ForEach(x => _Args.Add(x, null));

			//Split by spaces except when in quotes
			var split = input.Split('"').Select((x, index) =>
			{
				return index % 2 == 0
					? x.Split(new[] { ' ' })
					: new[] { x };
			}).SelectMany(x => x).Where(x => !String.IsNullOrWhiteSpace(x));

			//Split by colons. Left half is key, right half is value.
			var argKvps = split.Select(x =>
			{
				var kvp = x?.Split(new[] { ':' }, 2);
				return kvp.Length == 2
					? (Key: kvp[0], Value: kvp[1])
					: (Key: null, Value: null);
			}).Where(x => x.Key != null && x.Value != null);

			foreach (var arg in argKvps)
			{
				//If the params name is the arg name then add to params
				if (_HasParams && _ParamsName.CaseInsEquals(arg.Key))
				{
					//Keep this inside here so that if there are too many
					//params args it won't potentially go into the else if
					if (_ParamArgs.Count() < _ParamsLength)
					{
						_ParamArgs.Add(arg.Value);
					}
				}
				//If the args dictionary has the value as a key, set it.
				else if (_Args.ContainsKey(arg.Key))
				{
					_Args[arg.Key] = arg.Value;
				}
			}
		}

		/// <summary>
		/// Creates whatever <see cref="T"/> is with the gathered arguments. 
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
					var convertedArgs = _ParamArgs.Select(x => ConvertValue(t, x)).ToArray();
					//Have to use this method otherwise create instance throws exception
					//because this will send object[] instead of T[] when empty
					return convertedArgs.Any() ? convertedArgs : Array.CreateInstance(t, 0);
				}
				//Checking against the attribute again in case arguments have duplicate names
				else if (p.GetCustomAttribute<CustomArgumentAttribute>() != null && _Args.TryGetValue(p.Name, out var arg))
				{
					return ConvertValue(t, arg);
				}
				//Finally see if any additional args should be used
				else if (additionalArgsList.Any())
				{
					var value = additionalArgsList.FirstOrDefault(x => x.GetType() == t);
					if (value != null)
					{
						additionalArgsList.Remove(value);
						return value;
					}
				}
				return CreateInstance(t);
			}).ToArray();

			try
			{
				return (T)Activator.CreateInstance(typeof(T), parameters);
			}
			catch (MissingMethodException e)
			{
				e.Write();
				return (T)CreateInstance(typeof(T));
			}
		}
		/// <summary>
		/// Converts the string into the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public static object ConvertValue(Type type, string value)
		{
			var t = Nullable.GetUnderlyingType(type) ?? type;

			if (String.IsNullOrWhiteSpace(value))
			{
				//If the type is nullable then 
				return Nullable.GetUnderlyingType(type) == null ? CreateInstance(type) : null;
			}
			//If the type is an enum see if it's a valid name
			//If invalid then return default
			else if (t.IsEnum)
			{
				return ConvertEnum(t, value);
			}

			//Converters should work for primitives. Not sure what else it works for.
			var converter = TypeDescriptor.GetConverter(t);
			//I think ConvertFromInvariantString works with commas, but only if the computer's culture is set to one that uses it. 
			//Can't really test that easily because I CBA to switch my computer's language.
			return converter != null && converter.IsValid(value) ? converter.ConvertFromInvariantString(value) : CreateInstance(t);
		}
		/// <summary>
		/// Value types and classes with parameterless constructors can be created with no parameters.
		/// </summary>
		/// <param name="t"></param>
		/// <returns></returns>
		public static object CreateInstance(Type t)
			=> t.IsValueType || t.GetConstructors().Any(x => !x.GetParameters().Any()) ? Activator.CreateInstance(t) : null;
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
	}
}
