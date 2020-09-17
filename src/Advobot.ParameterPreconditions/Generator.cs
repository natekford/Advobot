using System;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Advobot.ParameterPreconditions
{
	[Generator]
	public sealed class Generator : ISourceGenerator
	{
		public void Execute(SourceGeneratorContext context)
		{
			var sb = new StringBuilder()
				.AddType(typeof(int))
				.AddType(typeof(ulong))
				.AddType(typeof(string))
				.AddType("Discord.GuildEmote")
				.AddType("Discord.IGuildChannel")
				.AddType("Discord.IInviteMetadata")
				.AddType("Discord.IRole")
				.AddType("Discord.IGuildUser");

			var sourceText = SourceText.From(sb.ToString(), Encoding.UTF8);
			context.AddSource("Parameter_Preconditions__", sourceText);
		}

		public void Initialize(InitializationContext context)
		{
		}
	}

	internal static class Utils
	{
		public static StringBuilder AddType(this StringBuilder sb, string fullName)
		{
			var typeName = fullName.Split('.').Last();
			return sb.AddType(typeName, fullName);
		}

		public static StringBuilder AddType(this StringBuilder sb, string typeName, string fullName)
		{
			return sb.AppendLine($@"
namespace Advobot.GeneratedParameterPreconditions
{{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;

	using Advobot.Attributes.ParameterPreconditions;
	using Advobot.Utilities;

	using Discord.Commands;
	using Discord;

	/// <summary>
	/// Automatically generated parameter precondition attribute for {typeName}.
	/// </summary>
	public abstract class {typeName}ParameterPreconditionAttribute
		: AdvobotParameterPreconditionAttribute
	{{
		/// <inheritdoc />
		public override IEnumerable<Type> SupportedTypes {{ get; }} = new[]
		{{
			typeof({fullName}),
		}};

		/// <inheritdoc />
		protected override Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			object value,
			IServiceProvider services)
		{{
			if (!(context.User is IGuildUser user))
			{{
				return this.FromInvalidInvoker().AsTask();
			}}
			if (!(value is {fullName} cast))
			{{
				return this.FromOnlySupports(value).AsTask();
			}}
			return SingularCheckPermissionsAsync(context, parameter, user, cast, services);
		}}

		/// <summary>
		/// Checks whether the condition for the <see cref=""{typeName}""/> is met before execution of the command.
		/// </summary>
		/// <param name=""context""></param>
		/// <param name=""parameter""></param>
		/// <param name=""value""></param>
		/// <param name=""invoker""></param>
		/// <param name=""services""></param>
		/// <returns></returns>
		protected abstract Task<PreconditionResult> SingularCheckPermissionsAsync(
			ICommandContext context,
			ParameterInfo parameter,
			IGuildUser invoker,
			{fullName} value,
			IServiceProvider services);
	}}
}}
");
		}

		public static StringBuilder AddType(this StringBuilder sb, Type type)
		{
			var typeName = type.Name;
			var fullName = type.FullName;
			return sb.AddType(typeName, fullName);
		}
	}
}