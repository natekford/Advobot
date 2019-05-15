using Advobot.Classes.Attributes;
using Advobot.Classes.Modules;
using Advobot.Classes.Settings;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes.TypeReaders
{
	public delegate Task<(bool, T)> AsyncTryConverter<T, TContext>(TContext context, string input, IServiceProvider services) where TContext : ICommandContext;

	/// <summary>
	/// Type reader for converting the passed in string to a basic type then using that basic type to find an object.
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	/// <typeparam name="TContext"></typeparam>
	public abstract class TypeReader<TBase, TContext> : TypeReader<TContext> where TContext : ICommandContext
	{
		/// <summary>
		/// Converts a string into an object of type <typeparamref name="TBase"/> asynchronously.
		/// </summary>
		public abstract AsyncTryConverter<TBase, TContext> TryConverter { get; }

		/// <inheritdoc />
		public override async Task<TypeReaderResult> ReadAsync(TContext context, string input, IServiceProvider services)
		{
			var (success, value) = await TryConverter.Invoke(context, input, services).CAF();
			if (!success)
			{
				return TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(TBase).Name}.");
			}
			return await ReadAsync(context, value, services).CAF();
		}
		/// <summary>
		/// Finds an object with a converted input.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public abstract Task<TypeReaderResult> ReadAsync(TContext context, TBase input, IServiceProvider services);
	}

	public static class AsyncTryConverters
	{
		/// <summary>
		/// Converts 
		/// </summary>
		/// <param name="_1"></param>
		/// <param name="input"></param>
		/// <param name="_2"></param>
		/// <returns></returns>
		public static Task<(bool, int)> TryConvertIntAsync<T>(T _1, string input, IServiceProvider _2) where T : ICommandContext
			=> Task.FromResult((int.TryParse(input, out var num), num));
	}

	/// <summary>
	/// Attempst to find a self assignable role group in the guild settings.
	/// </summary>
	[TypeReaderTargetType(typeof(SelfAssignableRoles))]
	public sealed class SelfAssignableRolesTypeReader : TypeReader<int, AdvobotCommandContext>
	{
		/// <inheritdoc />
		public override AsyncTryConverter<int, AdvobotCommandContext> TryConverter
			=> AsyncTryConverters.TryConvertIntAsync;

		/// <inheritdoc />
		public override Task<TypeReaderResult> ReadAsync(AdvobotCommandContext context, int input, IServiceProvider services)
		{
			if (context.GuildSettings.SelfAssignableGroups.TryGetSingle(x => x.Group == input, out var group))
			{
				return Task.FromResult(TypeReaderResult.FromSuccess(group));
			}
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ObjectNotFound, $"There is no group with the group number `{input}`"));
		}
	}
}
