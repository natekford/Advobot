using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
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
		/// <summary>
		/// Adds all of the collection to the list.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="collection"></param>
		public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (collection == null)
			{
				throw new ArgumentNullException(nameof(collection));
			}

			if (list is List<T> concrete)
			{
				concrete.AddRange(collection);
			}
			else
			{
				foreach (var item in collection)
				{
					list.Add(item);
				}
			}
		}
		/// <summary>
		/// Removes elements which match the supplied predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <param name="match"></param>
		public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
		{
			if (list == null)
			{
				throw new ArgumentNullException(nameof(list));
			}
			if (match == null)
			{
				throw new ArgumentNullException(nameof(match));
			}

			if (list is List<T> concrete)
			{
				return concrete.RemoveAll(match);
			}
			else
			{
				var removedCount = 0;
				for (int i = list.Count - 1; i >= 0; --i)
				{
					if (match(list[i]))
					{
						list.RemoveAt(i);
						++removedCount;
					}
				}
				return removedCount;
			}
		}
		/// <summary>
		/// Gets the amount of threads currently used.
		/// </summary>
		/// <returns></returns>
		public static int GetThreadCount()
		{
			using (var proc = Process.GetCurrentProcess())
			{
				return proc.Threads.Count;
			}
		}
		/// <summary>
		/// Gets the start time of the program.
		/// </summary>
		/// <returns></returns>
		public static DateTime GetStartTime()
		{
			using (var proc = Process.GetCurrentProcess())
			{
				return proc.StartTime;
			}
		}
	}
}