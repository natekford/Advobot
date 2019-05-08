using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a parameter to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal sealed class ParameterDetails : UsageDetails
	{
		public bool IsOptional { get; }
		public bool IsParams { get; }
		public bool IsRemainder { get; }
		public int Occurences { get; private set; }
		public ImmutableArray<string> Preconditions { get; }
		public Type Type { get; }
		public string TypeName { get; }

		public ParameterDetails(int deepness, System.Reflection.ParameterInfo reflection) : base(deepness, reflection.Name)
		{
			var attrs = reflection.GetCustomAttributes();
			IsOptional = attrs.GetAttribute<OptionalAttribute>() != null;
			IsParams = attrs.GetAttribute<ParamArrayAttribute>() != null;
			IsRemainder = attrs.GetAttribute<RemainderAttribute>() != null;
			Occurences = 1;

			var (t, n) = GetType(reflection.ParameterType);
			Type = t;
			TypeName = n;

			Preconditions = attrs.OfType<ParameterPreconditionAttribute>().Select(x => x.ToString()).ToImmutableArray();
		}
		public ParameterDetails(int deepness, Discord.Commands.ParameterInfo discord) : base(deepness, discord.Name)
		{
			IsOptional = discord.IsOptional;
			IsParams = discord.IsMultiple;
			IsRemainder = discord.IsRemainder;
			Occurences = 1;

			var (t, n) = GetType(discord.Type);
			Type = t;
			TypeName = n;

			Preconditions = discord.Preconditions.Select(x => x.ToString()).ToImmutableArray();
		}

		private static (Type type, string typeName) GetType(Type parameterType)
		{
			var t = parameterType;
			var n = t.Name;
			if (t != typeof(string) && t.IsSubclassOf(typeof(IEnumerable)))
			{
				t = t.GetElementType();
				n = $"Multiple {t.Name}";
			}

			//Generics have `1, `2, etc for each instance of them in use
			return (t, n.Contains('`') ? n.Substring(0, n.IndexOf('`')) : n);
		}
		public void SetOccurences(int occurences)
			=> Occurences = occurences;
		public override string ToString()
		{
			if (Type.IsEnum)
			{
				var names = Enum.GetNames(Type);
				if (names.Length <= 7)
				{
					return $"{TypeName}: {string.Join("|", names)}";
				}
			}
			return $"{TypeName}: {Name}";
		}
	}
}