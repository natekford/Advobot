using Advobot.Attributes;

using AdvorangesUtils;

using Discord.Commands;

using System.Diagnostics;
using System.Reflection;

namespace Advobot.Services.Commands;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
internal readonly struct TypeReaderInfo(TypeReader instance, TypeReaderTargetTypeAttribute attribute)
{
	public TypeReader Instance { get; } = instance;
	public IReadOnlyList<Type> TargetTypes { get; } = attribute.TargetTypes;
	private string DebuggerDisplay => TargetTypes.Join(x => x.FullName, ", ");
}

internal static class TypeReaderInfoUtils
{
	public static List<TypeReaderInfo> CreateTypeReaders(this Assembly assembly)
	{
		var list = new List<TypeReaderInfo>();
		foreach (var type in assembly.GetTypes())
		{
			var attr = type.GetCustomAttribute<TypeReaderTargetTypeAttribute>();
			if (attr == null)
			{
				continue;
			}

			if (Activator.CreateInstance(type) is not TypeReader instance)
			{
				throw new InvalidCastException($"{type} is not a {nameof(TypeReader)}.");
			}
			list.Add(new TypeReaderInfo(instance, attr));
		}
		return list;
	}
}