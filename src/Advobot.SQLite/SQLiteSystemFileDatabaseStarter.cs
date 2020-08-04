using System.IO;
using System.Threading.Tasks;

using Advobot.Settings;

namespace Advobot.SQLite
{
	/// <summary>
	/// Used for starting a SQLite database from a system file.
	/// </summary>
	public abstract class SQLiteSystemFileDatabaseStarter : IDatabaseStarter
	{
		/// <summary>
		/// The bot's directory.
		/// </summary>
		protected IBotDirectoryAccessor Accessor { get; }

		/// <summary>
		/// Creates an instance of <see cref="SQLiteSystemFileDatabaseStarter"/>.
		/// </summary>
		/// <param name="accessor"></param>
		protected SQLiteSystemFileDatabaseStarter(IBotDirectoryAccessor accessor)
		{
			Accessor = accessor;
		}

		/// <inheritdoc />
		public virtual Task EnsureCreatedAsync()
		{
			var location = GetLocation();
			if (!File.Exists(location))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(location));
				using (File.Create(location)) { }
			}
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public virtual string GetConnectionString()
			=> $"Data Source={GetLocation()}";

		/// <summary>
		/// Gets the location of the system file being used as a database.
		/// </summary>
		/// <returns></returns>
		public abstract string GetLocation();
	}
}