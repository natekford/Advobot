﻿using Advobot.Attributes.Preconditions;
using Advobot.Services.ImageResizing;

namespace Advobot.Modules;

/// <summary>
/// Module which is used to resize and upload images.
/// </summary>
[RequireImageNotProcessing]
public abstract class ImageResizerModule : AdvobotModuleBase
{
	/// <summary>
	/// The resizer to use.
	/// </summary>
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
	public IImageResizer Resizer { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.

	/// <summary>
	/// Queues the arguments and returns the position the arguments have been queued in.
	/// </summary>
	/// <param name="args"></param>
	/// <returns></returns>
	protected int Enqueue(IImageContext args)
	{
		Resizer.Enqueue(args);
		return Resizer.QueueCount;
	}
}