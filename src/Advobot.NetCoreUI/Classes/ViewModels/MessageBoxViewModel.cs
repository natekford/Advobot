using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia.Controls;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class MessageBoxViewModel : ReactiveObject
	{
		public string Text
		{
			get => _Text;
			set => this.RaiseAndSetIfChanged(ref _Text, value);
		}
		private string _Text;

		public string WindowTitle
		{
			get => _WindowTitle;
			set => this.RaiseAndSetIfChanged(ref _WindowTitle, value);
		}
		private string _WindowTitle;

		public string CurrentOption
		{
			get => _CurrentOption;
			set
			{
				this.RaiseAndSetIfChanged(ref _CurrentOption, value);
				CanClose = !string.IsNullOrWhiteSpace(value);
			}
		}
		private string _CurrentOption;

		private bool CanClose
		{
			get => _CanClose;
			set => this.RaiseAndSetIfChanged(ref _CanClose, value);
		}
		private bool _CanClose;

		public IEnumerable<string> Options
		{
			get => _Options;
			set
			{
				this.RaiseAndSetIfChanged(ref _Options, value);
				var requiresOption = Options?.Any() ?? false;
				DropDownVisible = requiresOption;
				ButtonText = requiresOption ? "Confirm" : "OK";
				CanClose = !requiresOption; //If no options are required, can simply click OK to close the dialog
			}
		}
		private IEnumerable<string> _Options;

		public string ButtonText
		{
			get => _ButtonText;
			set => this.RaiseAndSetIfChanged(ref _ButtonText, value);
		}
		private string _ButtonText;

		public bool DropDownVisible
		{
			get => _DropDownVisible;
			set => this.RaiseAndSetIfChanged(ref _DropDownVisible, value);
		}
		private bool _DropDownVisible;

		private ICommand CloseCommand { get; }

		public MessageBoxViewModel()
		{
			CloseCommand = ReactiveCommand.Create<Window>(window => window.Close(CurrentOption), this.WhenAnyValue(x => x.CanClose));
		}
	}
}