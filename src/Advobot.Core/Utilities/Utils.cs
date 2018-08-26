using Advobot.Interfaces;
using System.IO;

namespace Advobot.Utilities
{
    /// <summary>
    /// Random utilities.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Gets the file inside the bot directory.
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static FileInfo GetBaseBotDirectoryFile(this IBotDirectoryAccessor accessor, string fileName)
        {
            return new FileInfo(Path.Combine(accessor.BaseBotDirectory.FullName, fileName));
        }
    }
}