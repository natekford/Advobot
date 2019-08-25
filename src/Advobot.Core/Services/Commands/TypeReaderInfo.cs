using System;
using System.Collections.Generic;
using System.Reflection;
using Advobot.Attributes;
using Discord.Commands;

namespace Advobot.Services.Commands
{
	internal sealed class TypeReaderInfo
	{
		public TypeReader Instance { get; }
		public TypeReaderTargetTypeAttribute Attribute { get; }

		public TypeReaderInfo(TypeReader instance, TypeReaderTargetTypeAttribute attribute)
		{
			Instance = instance;
			Attribute = attribute;
		}

		public static IReadOnlyList<TypeReaderInfo> Create(Assembly assembly)
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
