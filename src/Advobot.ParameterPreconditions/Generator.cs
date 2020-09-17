﻿using System;
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
				.AddType("Discord.ITextChannel")
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
#pragma warning disable RCS1197 // Optimize StringBuilder.Append/AppendLine call.
			return sb.Append($@"
namespace Advobot.GeneratedParameterPreconditions
{{
	/// <summary>
	/// Automatically generated parameter precondition attribute for <see cref=""global::{fullName}""/>.
	/// Ensures type safety of the parameter.
	/// </summary>
	public abstract class {typeName}ParameterPreconditionAttribute
		: global::Advobot.Attributes.ParameterPreconditions.AdvobotParameterPreconditionAttribute
	{{
		/// <inheritdoc />
		protected override global::System.Threading.Tasks.Task<global::Discord.Commands.PreconditionResult> CheckPermissionsAsync(
			global::Discord.Commands.ICommandContext context,
			global::Discord.Commands.ParameterInfo parameter,
			global::Discord.IGuildUser invoker,
			global::System.Object value,
			global::System.IServiceProvider services)
		{{
			if (!(value is global::{fullName} cast))
			{{
				var result = global::Advobot.Utilities.PreconditionUtils.FromOnlySupports(this, value, typeof(global::{fullName}));
				return global::Advobot.Utilities.PreconditionUtils.AsTask(result);
			}}
			return CheckPermissionsAsync(context, parameter, invoker, cast, services);
		}}

		/// <summary>
		/// Checks whether the condition for the <see cref=""global::{fullName}""/> is met before execution of the command.
		/// </summary>
		/// <param name=""context""></param>
		/// <param name=""parameter""></param>
		/// <param name=""value""></param>
		/// <param name=""invoker""></param>
		/// <param name=""services""></param>
		/// <returns></returns>
		protected abstract global::System.Threading.Tasks.Task<global::Discord.Commands.PreconditionResult> CheckPermissionsAsync(
			global::Discord.Commands.ICommandContext context,
			global::Discord.Commands.ParameterInfo parameter,
			global::Discord.IGuildUser invoker,
			global::{fullName} value,
			global::System.IServiceProvider services);
	}}
}}
");
#pragma warning restore RCS1197 // Optimize StringBuilder.Append/AppendLine call.
		}

		public static StringBuilder AddType(this StringBuilder sb, Type type)
		{
			var typeName = type.Name;
			var fullName = type.FullName;
			return sb.AddType(typeName, fullName);
		}
	}
}