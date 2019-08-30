using System;
using System.Collections.Generic;
using System.Windows.Input;

using Advobot.Settings;
using Advobot.UI.Utils;

using AdvorangesUtils;

using Avalonia.Controls;

using ReactiveUI;

namespace Advobot.UI.ViewModels
{
	public sealed class OutputSearchWindowViewModel : ReactiveObject
	{
		private readonly IBotDirectoryAccessor _Accessor;

		private string? _Output;

		private string? _SearchTerm;

		public OutputSearchWindowViewModel(IBotDirectoryAccessor accessor)
		{
			_Accessor = accessor;

			SearchCommand = ReactiveCommand.Create(Search, this.WhenAnyValue(x => x.SearchTerm, x => !string.IsNullOrWhiteSpace(x)));
			SaveCommand = ReactiveCommand.Create(Save, this.WhenAnyValue(x => x.Output, x => x?.Length > 0));
			CloseCommand = ReactiveCommand.Create<Window>(window => window?.Close());
		}

		public ICommand CloseCommand { get; }

		public string? Output
		{
			get => _Output;
			set => this.RaiseAndSetIfChanged(ref _Output, value);
		}

		public ICommand SaveCommand { get; }

		public ICommand SearchCommand { get; }

		public string? SearchTerm
		{
			get => _SearchTerm;
			set => this.RaiseAndSetIfChanged(ref _SearchTerm, value);
		}

		public IEnumerable<string> Keys => ConsoleUtils.WrittenLines.Keys;

		private void Save()
		{
			var (text, _) = _Accessor.GenerateFileName("Output_Search").SaveAndGetResponse(Output ?? "");
			ConsoleUtils.WriteLine(text);
		}

		private void Search()
		{
			Output = "";
			foreach (var line in ConsoleUtils.WrittenLines[SearchTerm])
			{
				Output += $"{line}{Environment.NewLine}";
			}
		}
	}
}