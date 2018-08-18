using System.IO;
using Advobot.Interfaces;
using Advobot.Utilities;
using AdvorangesUtils;
using LiteDB;

namespace Advobot.Services
{
	internal static class ServiceUtils
	{
		public static LiteDatabase GetDatabase(this IBotDirectoryAccessor accessor, string fileName, BsonMapper mapper = null)
		{
			var file = accessor.GetBaseBotDirectoryFile(fileName);
			//Make sure the file is not currently being used if it exists
			if (file.Exists)
			{
				using (var fs = file.Open(System.IO.FileMode.Open, FileAccess.Read, FileShare.None)) { }
			}
			ConsoleUtils.DebugWrite($"Started the database connection for {Path.GetFileNameWithoutExtension(fileName)}.");
			return new LiteDatabase(new ConnectionString
			{
				Filename = file.FullName,
				Mode = LiteDB.FileMode.Exclusive, //One of my computer's will throw exceptions if this is shared
			}, mapper);
		}
	}
}