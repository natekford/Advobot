using System;
using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Discord.Commands;

namespace Advobot.Classes.TypeReaders
{
	/// <summary>
	/// Attempts to create a moderation reason with a time from a string.
	/// </summary>
	[TypeReaderTargetType(typeof(ModerationReason))]
	public sealed class ModerationReasonTypeReader : TypeReader
	{
		/// <summary>
		/// Creates a moderation reason from a string.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="input"></param>
		/// <param name="services"></param>
		/// <returns></returns>
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
			=> Task.FromResult(TypeReaderResult.FromSuccess(new ModerationReason(input)));
	}
}
