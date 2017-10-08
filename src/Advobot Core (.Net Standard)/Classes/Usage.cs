using Advobot.Actions.Formatting;
using Advobot.Classes.TypeReaders;
using Advobot.Interfaces;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Advobot.Classes
{
	public class Usage
	{
		public readonly string Text;

		public Usage(Type classType)
		{
			if (classType.IsNested)
			{
				throw new ArgumentException("Only use this method on a non nested class.");
			}

			/* Example:
			 * public class Top
			 * {
			 *		[Group("a")]
			 *		public class Nested
			 *		{
			 *			[Command("q")]
			 *			{
			 *				...
			 *			}
			 *		}
			 *		[Command("b")]
			 *		public async Task B(string text, uint dog)
			 *		{
			 *			...
			 *		}
			 *		[Command()]
			 *		public async Task C(int cat)
			 *		{
			 *			...
			 *		}
			 * }
			 * Gets formatted to [A|B|cat] <q|text> <dog>
			 */

			var sb = new StringBuilder();

			var classes = new List<ClassDetails>();
			var methods = new List<MethodDetails>();
			var parameters = new List<ParameterDetails>();
			GetAllNestedClassesAndMethods(-1, classType, classes, methods, parameters);

			classes = classes.GroupBy(x => x.Name).Select(x => x.First()).ToList();
			methods = methods.GroupBy(x => x.Name).Select(x => x.First()).ToList();
			parameters = parameters.GroupBy(x => x.Name).Select(x => x.First()).ToList();

			var deepest = new[]
			{
				methods.DefaultIfEmpty().Max(x => x.Deepness),
				classes.DefaultIfEmpty().Max(x => x.Deepness),
				parameters.DefaultIfEmpty().Max(x => x.Deepness),
			}.Max();

			for (int i = 0; i <= deepest; ++i)
			{
				var iDeepMethods = methods.Where(x => x.Deepness == i);
				var iDeepClasses = classes.Where(x => x.Deepness == i);
				var iDeepParams = parameters.Where(x => x.Deepness == i);

				var optional =
					(classes.Any() && !iDeepClasses.Any()) ||
					(methods.Any() && !iDeepMethods.Any()) ||
					(iDeepParams.All(x => x.Optional));

				StartArgument(sb, optional);
				AddOptions(sb, iDeepClasses);
				AddOptions(sb, iDeepMethods);
				AddOptions(sb, iDeepParams);
				CloseArgument(sb, optional);
			}

			Text = sb.ToString().Trim();
		}

		private void GetAllNestedClassesAndMethods(int deepness, Type classType, List<ClassDetails> classes, List<MethodDetails> methods, List<ParameterDetails> parameters)
		{
			++deepness;
			foreach (var type in GetNestedCommandClasses(classType))
			{
				GetAllNestedClassesAndMethods(deepness, type, classes, methods, parameters);
				classes.Add(new ClassDetails(deepness, type));
			}

			foreach (var method in GetCommands(classType))
			{
				var m = new MethodDetails(deepness, method);

				var extraDeep = 0;
				if (m.Name != null)
				{
					methods.Add(m);
					extraDeep = 1;
				}

				var parmesans = method.GetParameters();
				for (int i = 0; i < parmesans.Length; ++i)
				{
					parameters.Add(new ParameterDetails(extraDeep + deepness + i, parmesans[i]));
				}
			}
		}
		private IEnumerable<Type> GetNestedCommandClasses(Type classType)
		{
			return classType.GetNestedTypes(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<GroupAttribute>() != null);
		}
		private IEnumerable<MethodInfo> GetCommands(Type classType)
		{
			return classType.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.GetCustomAttribute<CommandAttribute>() != null);
		}

		private void StartArgument(StringBuilder sb, bool optional)
		{
			while (sb.Length > 0 && sb[0] == '|')
			{
				sb.Remove(1, sb.Length - 2);
			}

			sb.Append(optional ? "<" : "[");
		}
		private void CloseArgument(StringBuilder sb, bool optional)
		{
			while (sb[sb.Length - 1] == '|')
			{
				sb.Remove(sb.Length - 1, 1);
			}

			sb.Append(optional ? "> " : "] ");
		}
		private void AddOptions<T>(StringBuilder sb, IEnumerable<T> options) where T : IArgument
		{
			var converted = options.Where(x => !String.IsNullOrWhiteSpace(x?.Name)).Select(x => x.ToString());
			var addOrToEnd = converted.Any(x => !String.IsNullOrWhiteSpace(x?.ToString())) ? "|" : "";
			sb.Append(GeneralFormatting.JoinNonNullStrings("|", converted.ToArray()) + addOrToEnd);
		}
	}

	public struct ClassDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }

		public ClassDetails(int deepness, Type classType)
		{
			Deepness = deepness;
			Name = classType.GetCustomAttribute<GroupAttribute>()?.Prefix;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public struct MethodDetails : IArgument
	{
		public int Deepness { get; }
		public string Name { get; }

		public MethodDetails(int deepness, MethodInfo method)
		{
			Deepness = deepness;
			Name = method.GetCustomAttribute<CommandAttribute>()?.Text;
		}

		public override string ToString()
		{
			return Name;
		}
	}

	public struct ParameterDetails : IArgument
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
			{ typeof(SettingTypeReader.GuildSettingTypeReader), typeof(PropertyInfo) },
			{ typeof(SettingTypeReader.BotSettingTypeReader), typeof(PropertyInfo) },
			{ typeof(SettingTypeReader.BotSettingNonIEnumerableTypeReader), typeof(PropertyInfo) },
			{ typeof(UserIdTypeReader), typeof(IGuildUser) },
		};

		public int Deepness { get; }
		public string Name { get; }
		public Type Type { get; }
		public bool Optional { get; }

		public ParameterDetails(int deepness, System.Reflection.ParameterInfo parameter)
		{
			Deepness = deepness;
			Name = parameter.Name;
			Type = parameter.ParameterType;
			Optional = parameter.GetCustomAttribute<OptionalAttribute>() != null;

			var overriddenTypeReadersAttr = parameter.GetCustomAttribute<OverrideTypeReaderAttribute>();
			if (overriddenTypeReadersAttr != null)
			{
				Type = _TypeSwitcher[overriddenTypeReadersAttr.TypeReader];
			}
		}

		private IEnumerable<string> ConvertEnumToListOfNames(Type enumType)
		{
			if (!enumType.IsEnum)
			{
				throw new ArgumentException("Not an enum.");
			}

			return Enum.GetNames(enumType);
		}

		public override string ToString()
		{
			if (Type.IsEnum)
			{
				return $"{Type.Name}: {String.Join("|", ConvertEnumToListOfNames(Type))}";
			}
			else
			{
				return $"{Type.Name}: {Name}";
			}
		}
	}
}
