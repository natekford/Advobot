using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Advobot.Classes
{
	/// <summary>
	/// Arguments used for creating invites.
	/// </summary>
	[NamedArgumentType]
	public sealed class CreateInviteArguments
	{
		/// <summary>
		/// How long to make the invite last for.
		/// </summary>
		[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
		public int? Time { get; set; } = 86400;
		/// <summary>
		/// How many uses to let the invite last for.
		/// </summary>
		[OverrideTypeReader(typeof(PositiveNullableIntTypeReader))]
		public int? Uses { get; set; }
		/// <summary>
		/// Whether the user only receives temporary membership from the invite.
		/// </summary>
		public bool IsTemporary { get; set; }
		/// <summary>
		/// Whether the invite should be unique.
		/// </summary>
		public bool IsUnique { get; set; }

		/// <summary>
		/// Creates an invite with the supplied parameters.
		/// </summary>
		/// <param name="channel"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public Task<IInviteMetadata> CreateInviteAsync(INestedChannel channel, RequestOptions? options = null)
			=> channel.CreateInviteAsync(Time, Uses, IsTemporary, IsUnique, options);

		private sealed class PositiveNullableIntTypeReader : TypeReader
		{
			/// <inheritdoc />
			public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
			{
				if (input == null)
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(null));
				}
				else if (!int.TryParse(input, out var value))
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"Failed to parse {typeof(int?).Name}."));
				}
				else if (value < 1)
				{
					return Task.FromResult(TypeReaderResult.FromError(CommandError.UnmetPrecondition, "Value must be positive."));
				}
				else
				{
					return Task.FromResult(TypeReaderResult.FromSuccess(value));
				}
			}
		}
	}
}