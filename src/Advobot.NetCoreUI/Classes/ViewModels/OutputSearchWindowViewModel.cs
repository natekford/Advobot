using System;
using System.Collections.Generic;
using System.Windows.Input;
using Advobot.NetCoreUI.Utils;
using Advobot.Settings;
using AdvorangesUtils;
using Avalonia.Controls;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class OutputSearchWindowViewModel : ReactiveObject
	{
		public string? SearchTerm
		{
			get => _SearchTerm;
			set => this.RaiseAndSetIfChanged(ref _SearchTerm, value);
		}
		private string? _SearchTerm;

		public string? Output
		{
			get => _Output;
			set => this.RaiseAndSetIfChanged(ref _Output, value);
		}
		private string? _Output;

		public IEnumerable<string> Keys => ConsoleUtils.WrittenLines.Keys;

		public ICommand SearchCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand CloseCommand { get; }

		private readonly IBotDirectoryAccessor _Accessor;

		public OutputSearchWindowViewModel(IBotDirectoryAccessor accessor)
		{
			_Accessor = accessor;

			SearchCommand = ReactiveCommand.Create(Search, this.WhenAnyValue(x => x.SearchTerm, x => !string.IsNullOrWhiteSpace(x)));
			SaveCommand = ReactiveCommand.Create(Save, this.WhenAnyValue(x => x.Output, x => x?.Length > 0));
			CloseCommand = ReactiveCommand.Create<Window>(window => window?.Close());
		}

		private void Search()
		{
			Output = "";
			foreach (var line in ConsoleUtils.WrittenLines[SearchTerm])
			{
				Output += $"{line}{Environment.NewLine}";
			}
		}
		private void Save()
		{
			var (text, _) = _Accessor.GenerateFileName("Output_Search").SaveAndGetResponse(Output ?? "");
			ConsoleUtils.WriteLine(text);
		}
	}
}