using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Utilities;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Advobot.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a parameter to be used in <see cref="UsageGenerator"/>.
	/// </summary>
	internal sealed class ParameterDetails : UsageDetails
	{
		private static readonly ImmutableDictionary<Type, Type> _TypeSwitcher = new Dictionary<Type, Type>
		{
			{ typeof(BanTypeReader), typeof(IBan) },
			{ typeof(BypassUserLimitTypeReader), typeof(string) },
			{ typeof(ChannelPermissionsTypeReader), typeof(ChannelPermissions) },
			{ typeof(ColorTypeReader), typeof(Color) },
			{ typeof(EmoteTypeReader), typeof(IEmote) },
			{ typeof(GuildPermissionsTypeReader), typeof(GuildPermissions) },
			{ typeof(InviteTypeReader), typeof(IInvite) },
			{ typeof(PruneTypeReader), typeof(string) },
		}.ToImmutableDictionary();
		private static readonly ImmutableDictionary<Type, string> _NameSwitcher = new Dictionary<Type, string>
		{
			{ typeof(BypassUserLimitTypeReader), BypassUserLimitTypeReader.BYPASS_STRING },
			{ typeof(BanTypeReader), "UserId|Username#Discriminator" },
			{ typeof(ColorTypeReader), "Hexadecimal|R/G/B|Name" },
			{ typeof(EmoteTypeReader), "EmoteId|Name" },
			{ typeof(PruneTypeReader), PruneTypeReader.PRUNE_STRING }
		}.ToImmutableDictionary();

		public bool IsOptional { get; }
		public bool IsParams { get; }
		public bool IsRemainder { get; }
		public int Occurences { get; private set; }
		public string Text { get; }
		public Type Type { get; }
		public string TypeName { get; }

		public ParameterDetails(int deepness, System.Reflection.ParameterInfo reflection) : base(deepness, reflection.Name)
		{
			var attrs = reflection.GetCustomAttributes();
			IsOptional = attrs.GetAttribute<OptionalAttribute>() != null;
			IsParams = attrs.GetAttribute<ParamArrayAttribute>() != null;
			IsRemainder = attrs.GetAttribute<RemainderAttribute>() != null;
			Occurences = 1;

			var (t, n) = GetType(reflection.ParameterType, attrs.GetAttribute<OverrideTypeReaderAttribute>()?.TypeReader);
			Type = t;
			TypeName = n;

			Text = GetText(Type, attrs, IsRemainder);
		}
		public ParameterDetails(int deepness, Discord.Commands.ParameterInfo discord) : base(deepness, discord.Name)
		{
			var attrs = discord.Attributes;
			IsOptional = discord.IsOptional;
			IsParams = discord.IsMultiple;
			IsRemainder = discord.IsRemainder;
			Occurences = 1;

			var (t, n) = GetType(discord.Type, attrs.GetAttribute<OverrideTypeReaderAttribute>()?.TypeReader);
			Type = t;
			TypeName = n;

			Text = GetText(Type, attrs, IsRemainder);
		}

		private static (Type type, string typeName) GetType(Type parameterType, Type typeReader)
		{
			var t = parameterType;
			var n = t.Name;
			if (typeReader != null)
			{
				if (_TypeSwitcher.TryGetValue(typeReader, out var value))
				{
					t = value;
				}
				if (_NameSwitcher.TryGetValue(typeReader, out var name))
				{
					n = name;
				}
				if (typeReader.IsGenericType)
				{
					t = typeReader.GetGenericArguments()[0];
				}
			}
			if (t != typeof(string) && t.IsSubclassOf(typeof(IEnumerable)))
			{
				t = t.GetElementType();
				n = $"List of {t.Name}";
			}

			//Generics have `1, `2, etc for each instance of them in use
			return (t, n.Contains('`') ? n.Substring(0, n.IndexOf('`')) : n);
		}
		private static string GetText(Type parameterType, IEnumerable<Attribute> attrs, bool isRemainder)
		{
			var text = "";
			text += attrs.GetAttribute<ValidateNumberAttribute>() is ValidateNumberAttribute v ? $" {v}" : "";
			text += attrs.GetAttribute<ValidateStringAttribute>() is ValidateStringAttribute s ? $" {s}" : "";
			if (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == typeof(NamedArguments<>))
			{
				if (!isRemainder)
				{
					throw new ArgumentException($"Named arguments requires {nameof(RemainderAttribute)}.", parameterType.FullName);
				}

				var result = parameterType.GetProperty(nameof(NamedArguments<object>.ArgNames)).GetValue(null);
				var argNames = ((IEnumerable)result).Cast<string>().Select(x => CapitalizeFirstLetter(x));
				text += $" ({String.Join("|", argNames)})";
			}
			return text;
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