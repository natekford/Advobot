using Advobot.Core.Actions;
using Advobot.Core.Classes.Attributes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Allows named arguments to be used via an overly complex system of attributes and reflection.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CustomArguments<T> where T : class
	{
		protected static Dictionary<Type, Func<string, object>> _TryParses = new Dictionary<Type, Func<string, object>>
		{
			{ typeof(bool), (value) => bool.TryParse(value, out var result) ? result : false },
			{ typeof(int), (value) => int.TryParse(value, out var result) ? result : default },
			{ typeof(int?), (value) => int.TryParse(value, out var result) ? result as int? : null },
			{ typeof(uint), (value) => uint.TryParse(value, out var result) ? result : default },
			{ typeof(uint?), (value) => uint.TryParse(value, out var result) ? result as uint? : null },
			{ typeof(long), (value) => long.TryParse(value, out var result) ? result : default },
			{ typeof(long?), (value) => long.TryParse(value, out var result) ? result as long? : null },
			{ typeof(ulong), (value) => ulong.TryParse(value, out var result) ? result : default },
			{ typeof(ulong?), (value) => ulong.TryParse(value, out var result) ? result as ulong? : null },
		};

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
		static CustomArguments()
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
		public CustomArguments(string input)
		{
			ArgNames.ForEach(x => this._Args.Add(x, null));

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
					if (this._ParamArgs.Count() < _ParamsLength)
					{
						this._ParamArgs.Add(arg.Value);
					}
				}
				//If the args dictionary has the value as a key, set it.
				else if (this._Args.ContainsKey(arg.Key))
				{
					this._Args[arg.Key] = arg.Value;
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
				var t = p.ParameterType.IsArray ? p.ParameterType.GetElementType() : p.ParameterType;

				//Check params first otherwise will go into the middle else if
				if (p.GetCustomAttribute<ParamArrayAttribute>() != null)
				{
					//Convert all from string to whatever type they need to be
					//NEEDS TO BE AN ARRAY SINCE PARAMS IS AN ARRAY!
					var convertedArgs = this._ParamArgs.Select(x => ConvertValue(t, x)).ToArray();
					//Have to use this method otherwise create instance throws exception
					//because this will send object[] instead of T[] when empty
					return convertedArgs.Any() ? convertedArgs : Array.CreateInstance(t, 0);
				}
				//Checking against the attribute again in case arguments have duplicate names
				else if (p.GetCustomAttribute<CustomArgumentAttribute>() != null && this._Args.TryGetValue(p.Name, out var arg))
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

				return t.IsValueType ? Activator.CreateInstance(t) : null;
			}).ToArray();

			try
			{
				return (T)Activator.CreateInstance(typeof(T), parameters);
			}
			catch (MissingMethodException e)
			{
				ConsoleActions.ExceptionToConsole(e);
				return (T)Activator.CreateInstance(typeof(T));
			}
		}
		/// <summary>
		/// Converts the string into the given <paramref name="type"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object ConvertValue(Type type, string value)
		{
			var t = Nullable.GetUnderlyingType(type) ?? type;

			//If no argument then return the default value
			if (String.IsNullOrWhiteSpace(value))
			{
				return Activator.CreateInstance(type);
			}
			//If the type is an enum see if it's a valid name
			//If invalid then return default
			else if (t.IsEnum)
			{
				return ConvertEnum(type, value);
			}
			//If a type in the tryparses dictionary then return the tryparse's value
			else if(_TryParses.TryGetValue(t, out var f))
			{
				return f(value);
			}
			//If not then throw it into a final try catch of changetype
			else
			{
				try
				{
					return Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
				}
				catch
				{
					return Activator.CreateInstance(type);
				}
			}
		}
		/// <summary>
		/// Converts a string to an enum value.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		private object ConvertEnum(Type type, string value)
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
