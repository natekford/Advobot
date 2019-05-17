using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace Advobot.Utilities
{
	/// <summary>
	/// Attempts to convert a string to a specified type asynchronously.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="TContext"></typeparam>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <param name="services"></param>
	/// <returns></returns>

	public delegate Task<(bool, T)> AsyncTryConverter<T, TContext>(TContext context, string input, IServiceProvider services) where TContext : ICommandContext;

	/// <summary>
	/// Some default <see cref="AsyncTryConverter{T, TContext}"/>s.
	/// </summary>
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
}
