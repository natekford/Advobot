using Advobot.Classes;
using System.Collections.Generic;

namespace Advobot.Interfaces
{
	/// <summary>
	/// Abstraction for a log module. Handles counts of actions, and which commands have been ran. 
	/// </summary>
	public interface ILogModule
	{
		List<LoggedCommand> RanCommands { get; }

		uint TotalUsers { get; }
		uint TotalGuilds { get; }
		uint AttemptedCommands { get; }
		uint SuccessfulCommands { get; }
		uint FailedCommands { get; }
		uint LoggedJoins { get; }
		uint LoggedLeaves { get; }
		uint LoggedUserChanges { get; }
		uint LoggedEdits { get; }
		uint LoggedDeletes { get; }
		uint LoggedMessages { get; }
		uint LoggedImages { get; }
		uint LoggedGifs { get; }
		uint LoggedFiles { get; }

		void AddUsers(int users);
		void RemoveUsers(int users);
		void IncrementUsers();
		void DecrementUsers();
		void IncrementGuilds();
		void DecrementGuilds();
		void IncrementSuccessfulCommands();
		void IncrementFailedCommands();
		void IncrementJoins();
		void IncrementLeaves();
		void IncrementUserChanges();
		void IncrementEdits();
		void IncrementDeletes();
		void IncrementMessages();
		void IncrementImages();
		void IncrementGifs();
		void IncrementFiles();

		string FormatLoggedCommands();
		string FormatLoggedActions();
	}
}
