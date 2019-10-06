using System.IO;
using System.Threading.Tasks;

using Advobot.Settings;

namespace Advobot.Databases.AbstractSQL
{
	/// <summary>
	/// Used for starting a SQLite database from a system file.
	/// </summary>
	public abstract class SQLiteSystemFileDatabaseStarter : IDatabaseStarter
	{
		private readonly IBotDirectoryAccessor _Accessor;

		/// <summary>
		/// Creates an instance of <see cref="SQLiteSystemFileDatabaseStarter"/>.
		/// </summary>
		/// <param name="accessor"></param>
		protected SQLiteSystemFileDatabaseStarter(IBotDirectoryAccessor accessor)
		{
			_Accessor = accessor;
		}

		/// <inheritdoc />
		public virtual Task EnsureCreatedAsync()
		{
			var location = GetLocation(_Accessor);
			if (!File.Exists(location))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(location));
				using (File.Create(location)) { }
			}
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public virtual string GetConnectionString()
			=> $"Data Source={GetLocation(_Accessor)}";

		/// <summary>
		/// Gets the location of the system file being used as a database.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public abstract string GetLocation(IBotDirectoryAccessor accessor);
	}
}