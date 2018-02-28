using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace Advobot.Core.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a parameter to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal class ParameterDetails
	{
		private static Dictionary<Type, Type> _TypeSwitcher = new Dictionary<Type, Type>
		{
			{ typeof(BanTypeReader), typeof(IBan) },
			{ typeof(BypassUserLimitTypeReader), typeof(string) },
			{ typeof(ChannelPermissionsTypeReader), typeof(ChannelPermissions) },
			{ typeof(ColorTypeReader), typeof(Color) },
			{ typeof(EmoteTypeReader), typeof(IEmote) },
			{ typeof(GuildPermissionsTypeReader), typeof(GuildPermissions) },
			{ typeof(InviteTypeReader), typeof(IInvite) },
			{ typeof(PruneTypeReader), typeof(string) },
		};
		private static Dictionary<Type, string> _NameSwitcher = new Dictionary<Type, string>
		{
			{ typeof(BypassUserLimitTypeReader), BypassUserLimitTypeReader.BYPASS_STRING },
			{ typeof(BanTypeReader), "UserId|Username#Discriminator" },
			{ typeof(ColorTypeReader), "Hexadecimal|R/G/B|Name" },
			{ typeof(EmoteTypeReader), "EmoteId|Name" },
			{ typeof(PruneTypeReader), PruneTypeReader.PRUNE_STRING }
		};

		public int Deepness { get; private set; }
		public string Name { get; private set; }
		public string Text { get; private set; }
		public bool IsOptional { get; private set; }
		public bool IsParams { get; private set; }
		public bool IsRemainder { get; private set; }
		public int Occurences { get; private set; }

		public Type Type { get; private set; }
		public string TypeName { get; private set; }

		public ParameterDetails(int deepness, ParameterInfo parameter)
		{
			Deepness = deepness;
			Name = CapitalizeFirstLetter(parameter.Name);
			IsOptional = parameter.GetCustomAttribute<OptionalAttribute>() != null;
			IsParams = parameter.GetCustomAttribute<ParamArrayAttribute>() != null;
			IsRemainder = parameter.GetCustomAttribute<RemainderAttribute>() != null;
			Occurences = 1;

			SetType(parameter);
			SetText(parameter);
		}

		private void SetType(ParameterInfo parameter)
		{
			var overrideTypeReaderAttr = parameter.GetCustomAttribute<OverrideTypeReaderAttribute>();
			var typeReader = overrideTypeReaderAttr?.TypeReader;
			var pType = parameter.ParameterType;

			if (typeReader != null)
			{
				if (_TypeSwitcher.TryGetValue(typeReader, out var value))
				{
					Type = value;
				}
				else if (typeReader.IsGenericType)
				{
					Type = typeReader.GetGenericArguments()[0];
				}

				if (_NameSwitcher.TryGetValue(typeReader, out var name))
				{
					Name = name;
				}
			}
			else if (pType != typeof(string) && pType.GetInterfaces().Contains(typeof(IEnumerable)))
			{
				Type = pType.GetElementType();
				TypeName = $"List of {Type.Name}";
			}
			/* Not sure if needed right now
			else if (pType.IsGenericType)
			{
				Type = pType.GetGenericArguments()[0];
				TypeName = Type.Name;
			}*/
			else
			{
				Type = pType;
			}

			var n = TypeName ?? Type.Name;
			//Generics have `1, `2, etc for each instance of them in use
			TypeName = n.Contains('`') ? n.Substring(0, n.IndexOf('`')) : n;
		}
		private void SetText(ParameterInfo parameter)
		{
			var verifyNumberAttr = parameter.GetCustomAttribute<VerifyNumberAttribute>();
			if (verifyNumberAttr != null)
			{
				Text += $" {verifyNumberAttr}";
			}
			var verifyStringLengthAttr = parameter.GetCustomAttribute<VerifyStringLengthAttribute>();
			if (verifyStringLengthAttr != null)
			{
				Text += $" {verifyStringLengthAttr}";
			}
			if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(NamedArguments<>))
			{
				if (!IsRemainder)
				{
					throw new ArgumentException($"requires {nameof(RemainderAttribute)}", Type.FullName);
				}

				var result = Type.GetProperty(nameof(NamedArguments<object>.ArgNames)).GetValue(null);
				var argNames = ((IEnumerable)result).Cast<string>().Select(x => CapitalizeFirstLetter(x));
				Text += $" ({String.Join("|", argNames)})";
			}
		}

		private string CapitalizeFirstLetter(string n)
		{
			return n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1);
		}
		public void SetDeepness(int deepness)
		{
			Deepness = deepness;
		}
		public void SetOccurences(int occurences)
		{
			Occurences = occurences;
		}

		public override string ToString()
		{
			if (Type.IsEnum)
			{
				var names = Enum.GetNames(Type);
				if (names.Length <= 7)
				{
					return $"{TypeName}: {String.Join("|", names)}{Text}";
				}
			}
			return $"{TypeName}: {Name}{Text}";
		}
	}
}