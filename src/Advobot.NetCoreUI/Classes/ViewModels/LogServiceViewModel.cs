using System.ComponentModel;
using Advobot.Interfaces;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class LogServiceViewModel : INotifyPropertyChanged
	{
		public int TotalUsers => _LogService.TotalUsers.Count;
		public int TotalGuilds => _LogService.TotalGuilds.Count;
		public int AttemptedCommands => _LogService.AttemptedCommands.Count;
		public int SuccessfulCommands => _LogService.SuccessfulCommands.Count;
		public int FailedCommands => _LogService.FailedCommands.Count;
		public int UserJoins => _LogService.UserJoins.Count;
		public int UserLeaves => _LogService.UserLeaves.Count;
		public int UserChanges => _LogService.UserChanges.Count;
		public int MessageEdits => _LogService.MessageEdits.Count;
		public int MessageDeletes => _LogService.MessageDeletes.Count;
		public int Messages => _LogService.Messages.Count;
		public int Images => _LogService.Images.Count;
		public int Animated => _LogService.Animated.Count;
		public int Files => _LogService.Files.Count;

		private readonly ILogService _LogService;

		public LogServiceViewModel(ILogService logService)
		{
			_LogService = logService;
		}

		public event PropertyChangedEventHandler PropertyChanged
		{
			add => _LogService.PropertyChanged += value;
			remove => _LogService.PropertyChanged -= value;
		}
	}
}
