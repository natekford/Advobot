using Advobot.Preconditions;
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
	public IImageResizer Resizer { get; set; } = null!;

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