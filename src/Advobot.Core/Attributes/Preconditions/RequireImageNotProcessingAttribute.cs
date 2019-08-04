using System;
using System.Threading.Tasks;
using Advobot.Services.ImageResizing;
using Advobot.Utilities;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Attributes.Preconditions
{
	/// <summary>
	/// Disallows the command from running if an image is currently being resized.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
	public sealed class RequireImageNotProcessingAttribute : PreconditionAttribute
	{
		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var resizer = services.GetRequiredService<IImageResizer>();
			if (!resizer.IsGuildAlreadyProcessing(context.Guild.Id))
			{
				return this.FromSuccessAsync();
			}
			return this.FromErrorAsync("Guild already has an image processing.");
		}
		/// <inheritdoc />
		public override string ToString()
			=> "Not currently processing another image";
	}
}