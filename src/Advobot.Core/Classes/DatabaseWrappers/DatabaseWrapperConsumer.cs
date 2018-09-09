using System;
using Advobot.Interfaces;
using AdvorangesUtils;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Classes.DatabaseWrappers
{
	/// <summary>
	/// This class is the base of a service which uses a database.
	/// </summary>
	public abstract class DatabaseWrapperConsumer : IUsesDatabase, IDisposable
	{
		/// <summary>
		/// The name of the database.
		/// </summary>
		public abstract string DatabaseName { get; }
		/// <summary>
		/// The database being used. This can be any database type, or even just a simple dictionary.
		/// </summary>
		protected IDatabaseWrapper DatabaseWrapper { get; set; }
		/// <summary>
		/// The factory for creating <see cref="DatabaseWrapper"/>.
		/// </summary>
		protected IDatabaseWrapperFactory DatabaseFactory { get; }

		/// <summary>
		/// Creates an instance of <see cref="DatabaseWrapperConsumer"/>.
		/// </summary>
		/// <param name="provider"></param>
		public DatabaseWrapperConsumer(IServiceProvider provider)
		{
			DatabaseFactory = provider.GetRequiredService<IDatabaseWrapperFactory>();
		}

		/// <inheritdoc />
		public void Start()
		{
			DatabaseWrapper = DatabaseFactory.CreateWrapper(DatabaseName);
			ConsoleUtils.DebugWrite($"Started the database connection for {DatabaseName}.");
			AfterStart();
		}
		/// <summary>
		/// Actions to do after the database connection has started.
		/// </summary>
		protected virtual void AfterStart()
		{
			return;
		}
		/// <inheritdoc />
		public void Dispose()
		{
			BeforeDispose();
			DatabaseWrapper.Dispose();
		}
		/// <summary>
		/// Actions to do before the database connection has been disposed.
		/// </summary>
		protected virtual void BeforeDispose()
		{
			return;
		}
	}
}