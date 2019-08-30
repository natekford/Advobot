using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using ImageMagick;

namespace Advobot.Services.ImageResizing
{
	/// <summary>
	/// How to use and resize an image.
	/// </summary>
	public abstract class ImageContextBase : IImageContext
	{
		private IUserMessage? _Message;

		/// <inheritdoc />
		public UserProvidedImageArgs Args { get; }

		/// <inheritdoc />
		public ulong GuildId => Context.Guild.Id;

		/// <inheritdoc />
		public abstract long MaxAllowedLengthInBytes { get; }

		/// <inheritdoc />
		public abstract string Type { get; }

		/// <inheritdoc />
		public Uri Url { get; }

		/// <summary>
		/// The command context for this image context.
		/// </summary>
		protected ICommandContext Context { get; }

		/// <summary>
		/// Creates an instance of <see cref="ImageContextBase"/>.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="url"></param>
		/// <param name="userArgs"></param>
		protected ImageContextBase(ICommandContext context, Uri url, UserProvidedImageArgs userArgs)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			Url = url ?? throw new ArgumentNullException(nameof(url));
			Args = userArgs ?? new UserProvidedImageArgs();
		}

		/// <inheritdoc />
		public abstract IResult CanUseFormat(MagickFormat format);

		/// <inheritdoc />
		public async Task ReportAsync(string value)
		{
			if (_Message != null)
			{
				await _Message.ModifyAsync(x => x.Content = value).CAF();
			}
			else
			{
				_Message = await MessageUtils.SendMessageAsync(Context.Channel, value).CAF();
			}
		}

		/// <inheritdoc />
		public async Task SendFinalResponseAsync(IResult result)
		{
			if (_Message != null)
			{
				await _Message.DeleteAsync().CAF();
			}

			var t = Type.ToLower();
			var text = result.IsSuccess
				? $"Successfully created the {t}."
				: $"Failed to create the {t}. Reason: {result.ErrorReason}.";
			await MessageUtils.SendMessageAsync(Context.Channel, text).CAF();
		}

		/// <inheritdoc />
		public abstract Task<IResult> UseStream(MemoryStream stream);
	}
}