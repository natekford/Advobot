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
			{ typeof(PruneTypeReader), PruneTypeReader.PRUNE_STRING },
		};

		public int Deepness { get; }
		public string Name { get; }
		public string Text { get; }
		public bool Optional { get; }

		public Type Type { get; }
		public string TypeName { get; }

		public ParameterDetails(int deepness, System.Reflection.ParameterInfo parameter)
		{
			var optionalAttr = parameter.GetCustomAttribute<OptionalAttribute>();
			var overrideTypeReaderAttr = parameter.GetCustomAttribute<OverrideTypeReaderAttribute>();
			var verifyNumberAttr = parameter.GetCustomAttribute<VerifyNumberAttribute>();
			var verifyStringLengthAttr = parameter.GetCustomAttribute<VerifyStringLengthAttribute>();

			Deepness = deepness;
			Name = CapitalizeFirstLetter(parameter.Name);
			Optional = optionalAttr != null;

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

			if (verifyNumberAttr != null)
			{
				Text = $" {verifyNumberAttr.ToString()}";
			}
			else if (verifyStringLengthAttr != null)
			{
				Text = $" {verifyStringLengthAttr.ToString()}";
			}
		}

		private string CapitalizeFirstLetter(string n)
		{
			return n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1);
		}

		public override string ToString()
		{
			if (Type.IsEnum)
			{
				var names = Enum.GetNames(Type);
				if (names.Length <= 5)
				{
					return $"{TypeName}: {String.Join("|", names)}";
				}
			}
			return $"{TypeName}: {Name}{Text}";
		}
	}
}