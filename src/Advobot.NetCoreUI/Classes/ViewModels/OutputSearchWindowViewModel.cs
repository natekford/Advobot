using System;
using System.Collections.Generic;
using System.Windows.Input;
using Advobot.Interfaces;
using Advobot.NetCoreUI.Utils;
using AdvorangesUtils;
using Avalonia.Controls;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public sealed class OutputSearchWindowViewModel : ReactiveObject
	{
		public string SearchTerm
		{
			get => _SearchTerm;
			set
			{
				this.RaiseAndSetIfChanged(ref _SearchTerm, value);
				CanSearch = !string.IsNullOrWhiteSpace(value);
			}
		}
		private string _SearchTerm;

		private bool CanSearch
		{
			get => _CanSearch;
			set => this.RaiseAndSetIfChanged(ref _CanSearch, value);
		}
		private bool _CanSearch;

		public string Output
		{
			get => _Output;
			set
			{
				this.RaiseAndSetIfChanged(ref _Output, value);
				CanSaveOutput = value.Length > 0;
			}
		}
		private string _Output;

		private bool CanSaveOutput
		{
			get => _CanSaveOutput;
			set => this.RaiseAndSetIfChanged(ref _CanSaveOutput, value);
		}
		private bool _CanSaveOutput;

		public string SaveResponse
		{
			get => _SaveResponse;
			set => this.RaiseAndSetIfChanged(ref _SaveResponse, value);
		}
		private string _SaveResponse;

		public IEnumerable<string> Keys => ConsoleUtils.WrittenLines.Keys;

		public ICommand SearchCommand { get; }
		public ICommand SaveCommand { get; }
		public ICommand CloseCommand { get; }

		private readonly IBotDirectoryAccessor _Accessor;

		public OutputSearchWindowViewModel(IBotDirectoryAccessor accessor)
		{
			_Accessor = accessor;

			SearchCommand = ReactiveCommand.Create(() =>
			{
				Output = "";
				foreach (var line in ConsoleUtils.WrittenLines[SearchTerm])
				{
					Output += $"{line}{Environment.NewLine}";
				}
			}, this.WhenAnyValue(x => x.CanSearch));
			SaveCommand = ReactiveCommand.Create(() =>
			{
				if (Output.Length != 0)
				{
					var response = _Accessor.GenerateFileName("Output_Search").SaveAndGetResponse(Output);
					ConsoleUtils.WriteLine(response, name: "Saving Output Search");
				}
			}, this.WhenAnyValue(x => x.CanSaveOutput));
			CloseCommand = ReactiveCommand.Create<Window>(window => window?.Close());
		}
	}
}