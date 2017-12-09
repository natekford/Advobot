using Advobot.Core.Classes.Attributes;
using Advobot.Core.Classes.TypeReaders;
using Advobot.Core.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Advobot.Core.Classes.UsageGeneration
{
	/// <summary>
	/// Information about a parameter to be used in <see cref="UsageGenerator"/>.
	/// </summary>
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
			{ typeof(UserIdTypeReader), typeof(ulong) },
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
			this.Deepness = deepness;
			this.Name = CapitalizeFirstLetter(parameter.Name);
			this.IsOptional = parameter.GetCustomAttribute<OptionalAttribute>() != null;
			this.IsParams = parameter.GetCustomAttribute<ParamArrayAttribute>() != null;
			this.IsRemainder = parameter.GetCustomAttribute<RemainderAttribute>() != null;
			this.Occurences = 1;

			SetType(parameter);
			SetText(parameter);
		}

		private void SetType(System.Reflection.ParameterInfo parameter)
		{
			var overrideTypeReaderAttr = parameter.GetCustomAttribute<OverrideTypeReaderAttribute>();
			var typeReader = overrideTypeReaderAttr?.TypeReader;
			var pType = parameter.ParameterType;

			if (typeReader != null)
			{
				if (_TypeSwitcher.TryGetValue(typeReader, out var value))
				{
					this.Type = value;
				}
				else if (typeReader.IsGenericType)
				{
					this.Type = typeReader.GetGenericArguments()[0];
				}

				if (_NameSwitcher.TryGetValue(typeReader, out var name))
				{
					this.Name = name;
				}
			}
			else if (pType != typeof(string) && pType.GetInterfaces().Contains(typeof(IEnumerable)))
			{
				this.Type = pType.GetElementType();
				this.TypeName = $"List of {this.Type.Name}";
			}
			/* Not sure if needed right now
			else if (pType.IsGenericType)
			{
				Type = pType.GetGenericArguments()[0];
				TypeName = Type.Name;
			}*/
			else
			{
				this.Type = pType;
			}

			var n = this.TypeName ?? this.Type.Name;
			this.TypeName = n.Substring(0, n.IndexOf('`') + 1);
		}
		private void SetText(System.Reflection.ParameterInfo parameter)
		{
			var verifyNumberAttr = parameter.GetCustomAttribute<VerifyNumberAttribute>();
			if (verifyNumberAttr != null)
			{
				this.Text += $" {verifyNumberAttr.ToString()}";
			}
			var verifyStringLengthAttr = parameter.GetCustomAttribute<VerifyStringLengthAttribute>();
			if (verifyStringLengthAttr != null)
			{
				this.Text += $" {verifyStringLengthAttr.ToString()}";
			}
			if (this.Type.IsGenericType && this.Type.GetGenericTypeDefinition() == typeof(CustomArguments<>))
			{
				if (!this.IsRemainder)
				{
					throw new ArgumentException($"{this.Type.Name} requires {nameof(RemainderAttribute)}.");
				}

				var result = this.Type.GetProperty(nameof(CustomArguments<object>.ArgNames)).GetValue(null);
				var argNames = ((IEnumerable)result).Cast<string>().Select(x => CapitalizeFirstLetter(x));
				this.Text += $" ({String.Join("|", argNames)})";
			}
		}

		private string CapitalizeFirstLetter(string n) => n[0].ToString().ToUpper() + n.Substring(1, n.Length - 1);

		public void SetDeepness(int deepness) => this.Deepness = deepness;
		public void SetOccurences(int occurences) => this.Occurences = occurences;

		public override string ToString()
		{
			if (this.Type.IsEnum)
			{
				var names = Enum.GetNames(this.Type);
				if (names.Length <= 7)
				{
					return $"{this.TypeName}: {String.Join("|", names)}{this.Text}";
				}
			}
			return $"{this.TypeName}: {this.Name}{this.Text}";
		}
	}
}