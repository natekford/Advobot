using System.ComponentModel;

using Advobot.Services.LogCounters;

namespace Advobot.UI.ViewModels
{
	public sealed class LogServiceViewModel : INotifyPropertyChanged
	{
		private readonly ILogCounterService _LogService;

		public int Animated => _LogService.Animated.Count;
		public int AttemptedCommands => _LogService.AttemptedCommands.Count;
		public int FailedCommands => _LogService.FailedCommands.Count;
		public int Files => _LogService.Files.Count;
		public int Images => _LogService.Images.Count;
		public int MessageDeletes => _LogService.MessageDeletes.Count;
		public int MessageEdits => _LogService.MessageEdits.Count;
		public int Messages => _LogService.Messages.Count;
		public int SuccessfulCommands => _LogService.SuccessfulCommands.Count;
		public int TotalGuilds => _LogService.TotalGuilds.Count;
		public int TotalUsers => _LogService.TotalUsers.Count;
		public int UserChanges => _LogService.UserChanges.Count;
		public int UserJoins => _LogService.UserJoins.Count;
		public int UserLeaves => _LogService.UserLeaves.Count;

		public event PropertyChangedEventHandler? PropertyChanged
		{
			add => ((INotifyPropertyChanged)_LogService).PropertyChanged += value;
			remove => ((INotifyPropertyChanged)_LogService).PropertyChanged -= value;
		}

		public LogServiceViewModel(ILogCounterService logService)
		{
			_LogService = logService;
		}
	}
}