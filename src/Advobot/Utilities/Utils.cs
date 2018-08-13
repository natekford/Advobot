using System;
using System.Collections.Generic;
using System.IO;
using Advobot.Interfaces;

namespace Advobot.Utilities
{
	/// <summary>
	/// Random utilities.
	/// </summary>
	public static class Utils
	{
		/// <summary>
		/// Gets the specified attribute from the supplied attributes.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="attrs"></param>
		/// <returns></returns>
		public static T GetAttribute<T>(this IEnumerable<Attribute> attrs)
		{
			foreach (var attr in attrs)
			{
				if (attr is T t)
				{
					return t;
				}
			}
			return default;
		}
		/// <summary>
		/// Tests whether the specified generic type can be assigned to an instance of the current type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="c"></param>
		/// <returns></returns>
		public static bool IsAssignableFromGeneric(this Type type, Type c)
		{
			if (type == typeof(object))
			{
				return false;
			}
			if (type.IsGenericType && type.GetGenericTypeDefinition().IsAssignableFrom(c))
			{
				return true;
			}
			return IsAssignableFromGeneric(type.BaseType, c);
		}
		/// <summary>
		/// Gets the file inside the bot directory.
		/// </summary>
		/// <param name="accessor"></param>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
		{
			return new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
		}
	}
}