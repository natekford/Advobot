using System;
using System.Threading.Tasks;
using AdvorangesUtils;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to see if the input matches <see cref="PRUNE_STRING"/>.
	/// </summary>
	public sealed class PruneTypeReader : TypeReader
	{
		internal const string PRUNE_STRING = "ActualPrune";

		/// <summary>
		/// Returns true if the input is equal to <see cref="PRUNE_STRING"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
			=> Task.FromResult(TypeReaderResult.FromSuccess(PRUNE_STRING.CaseInsEquals(input)));
	}
}
