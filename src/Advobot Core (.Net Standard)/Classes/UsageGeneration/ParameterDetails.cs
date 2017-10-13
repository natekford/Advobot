using Advobot.Classes.Attributes;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Advobot.Classes.UsageGeneration
{
	internal class ParameterDetails : IArgument
	{
		private static Dictionary<Type, Type> _TypeSwitcher = new Dictionary<Type, Type>
		{
			{ typeof(BanTypeReader), typeof(IBan) },
			{ typeof(BypassUserLimitTypeReader), typeof(string) },
			{ typeof(ChannelPermissionsTypeReader), typeof(ChannelPermissions) },
			{ typeof(ColorTypeReader), typeof(Color) },
			{ typeof(CommandSwitchTypeReader), typeof(CommandSwitch) },
			{ typeof(EmoteTypeReader), typeof(IEmote) },
			{ typeof(GuildPermissionsTypeReader), typeof(GuildPermissions) },
			{ typeof(InviteTypeReader), typeof(IInvite) },
			{ typeof(PruneTypeReader), typeof(string) },
			{ typeof(SettingTypeReader.GuildSettingTypeReader), typeof(string) },
			{ typeof(SettingTypeReader.BotSettingTypeReader), typeof(string) },
			{ typeof(UserIdTypeReader), typeof(IGuildUser) },
		};
		private static Dictionary<Type, string> _NameSwitcher = new Dictionary<Type, string>
		{
			{ typeof(BypassUserLimitTypeReader), BypassUserLimitTypeReader.BYPASS_STRING },
			{ typeof(BanTypeReader), "UserId|Username#Discriminator" },
			{ typeof(ColorTypeReader), "Hexadecimal|R/G/B|Name" },
			{ typeof(EmoteTypeReader), "EmoteId|Name" },
			{ typeof(PruneTypeReader), PruneTypeReader.PRUNE_STRING },
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

		public ParameterDetails(int deepness, System.Reflection.ParameterInfo parameter)
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

		private void SetType(System.Reflection.ParameterInfo parameter)
		{
			var overrideTypeReaderAttr = parameter.GetCustomAttribute<OverrideTypeReaderAttribute>();
			var typeReader = overrideTypeReaderAttr?.TypeReader;
			if (typeReader != null)
			{
				if (_TypeSwitcher.TryGetValue(typeReader, out var value))
				{
					Type = value;
					TypeName = Type.Name;
				}
				if (_NameSwitcher.TryGetValue(typeReader, out var name))
				{
					Name = name;
				}
			}
			else if (parameter.ParameterType != typeof(string) && parameter.ParameterType.GetInterfaces().Contains(typeof(IEnumerable)))
			{
				Type = parameter.ParameterType.GetElementType();
				TypeName = $"List of {Type.Name}";
			}
			else
			{
				Type = parameter.ParameterType;
				TypeName = Type.Name;
			}
			TypeName = TypeName.TrimEnd('`', '1');
		}
		private void SetText(System.Reflection.ParameterInfo parameter)
		{
			var verifyNumberAttr = parameter.GetCustomAttribute<VerifyNumberAttribute>();
			if (verifyNumberAttr != null)
			{
				Text += $" {verifyNumberAttr.ToString()}";
			}
			var verifyStringLengthAttr = parameter.GetCustomAttribute<VerifyStringLengthAttribute>();
			if (verifyStringLengthAttr != null)
			{
				Text += $" {verifyStringLengthAttr.ToString()}";
			}
			if (Type.IsGenericType && Type.GetGenericTypeDefinition() == typeof(CustomArguments<>))
			{
				if (!IsRemainder)
				{
					throw new ArgumentException($"{Type.Name} requires {nameof(RemainderAttribute)}.");
				}

				var result = Type.GetProperty(nameof(CustomArguments<object>.ArgNames)).GetValue(null);
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
		public void IncrementOccurences()
		{
			++Occurences;
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