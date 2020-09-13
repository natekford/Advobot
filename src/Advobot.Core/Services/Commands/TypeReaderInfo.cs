using System;
using System.Collections.Generic;
using System.Reflection;

using Advobot.Attributes;

using Discord.Commands;

namespace Advobot.Services.Commands
{
	internal readonly struct TypeReaderInfo
	{
		public TypeReader Instance { get; }
		public IReadOnlyList<Type> TargetTypes { get; }

		public TypeReaderInfo(TypeReader instance, TypeReaderTargetTypeAttribute attribute)
		{
			Instance = instance;
			TargetTypes = attribute.TargetTypes;
		}
	}

	internal static class TypeReaderInfoUtils
	{
		public static IReadOnlyList<TypeReaderInfo> CreateTypeReaders(this Assembly assembly)
		{
			var list = new List<TypeReaderInfo>();
			foreach (var type in assembly.GetTypes())
			{
				var attr = type.GetCustomAttribute<TypeReaderTargetTypeAttribute>();
				if (attr == null)
				{
					continue;
				}

				var instance = (TypeReader)Activator.CreateInstance(type);
				list.Add(new TypeReaderInfo(instance, attr));
			}
			return list;
		}
	}
}