using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Avalonia.Controls;

using ReactiveUI;

namespace Advobot.UI.ViewModels
{
	public sealed class MessageBoxViewModel : ReactiveObject
	{
		private string? _ButtonText;

		private bool _CanClose;

		private string? _CurrentOption;

		private bool _DropDownVisible;

		private IEnumerable<string> _Options = Enumerable.Empty<string>();

		private string? _Text;

		private string? _WindowTitle;

		public MessageBoxViewModel()
		{
			CloseCommand = ReactiveCommand.Create<Window>(window => window.Close(CurrentOption), this.WhenAnyValue(x => x.CanClose));
		}

		public string? ButtonText
		{
			get => _ButtonText;
			set => this.RaiseAndSetIfChanged(ref _ButtonText, value);
		}

		public ICommand CloseCommand { get; }

		public string? CurrentOption
		{
			get => _CurrentOption;
			set
			{
				this.RaiseAndSetIfChanged(ref _CurrentOption, value);
				CanClose = !string.IsNullOrWhiteSpace(value);
			}
		}

		public bool DropDownVisible
		{
			get => _DropDownVisible;
			set => this.RaiseAndSetIfChanged(ref _DropDownVisible, value);
		}

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

		public string? Text
		{
			get => _Text;
			set => this.RaiseAndSetIfChanged(ref _Text, value);
		}

		public string? WindowTitle
		{
			get => _WindowTitle;
			set => this.RaiseAndSetIfChanged(ref _WindowTitle, value);
		}

		private bool CanClose
		{
			get => _CanClose;
			set => this.RaiseAndSetIfChanged(ref _CanClose, value);
		}
	}
}