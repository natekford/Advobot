using System;
using System.Threading.Tasks;

using Advobot.Services.HelpEntries;
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
	public sealed class RequireImageNotProcessingAttribute
		: PreconditionAttribute, IPrecondition
	{
		/// <inheritdoc />
		public string Summary
			=> "Not currently processing another image";

		/// <inheritdoc />
		public override Task<PreconditionResult> CheckPermissionsAsync(
			ICommandContext context,
			CommandInfo command,
			IServiceProvider services)
		{
			var resizer = services.GetRequiredService<IImageResizer>();
			if (!resizer.IsGuildAlreadyProcessing(context.Guild.Id))
			{
				return PreconditionUtils.FromSuccess().Async();
			}
			return PreconditionUtils.FromError("Guild already has an image processing.").Async();
		}
	}
}