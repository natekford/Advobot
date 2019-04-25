using System.IO;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Indicates the object can be saved to a file.
	/// </summary>
	public interface ISavable
	{
		/// <summary>
		/// Gets the file associated with the settings.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		FileInfo GetFile(IBotDirectoryAccessor accessor);
		/// <summary>
		/// Serializes this object and then overwrites the file.
		/// </summary>
		/// <param name="accessor">Where to save the bot files.</param>
		void Save(IBotDirectoryAccessor accessor);
	}
}
