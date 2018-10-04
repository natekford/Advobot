using System.Threading.Tasks;
using Advobot.Classes.Attributes.Preconditions;
using Advobot.Interfaces;
using AdvorangesUtils;

namespace Advobot.Classes
{
	/// <summary>
	/// Module which is used to resize and upload images.
	/// </summary>
	[RequireImageNotProcessing]
	public abstract class ImageResizerModule : AdvobotModuleBase
	{
		/// <summary>
		/// The resizer to use.
		/// </summary>
		public IImageResizer Resizer { get; set; }

		/// <summary>
		/// Queues the arguments and sends a response message.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public async Task ProcessAsync(IImageArgs args)
		{
			Resizer.Process(args);
			await ReplyAsync($"Position in image modification queue: {Resizer.QueueCount}.").CAF();
		}
	}
}