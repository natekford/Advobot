using System;
using System.Windows.Input;
using Avalonia.Input;
using ReactiveUI;

namespace Advobot.NetCoreUI.Classes.ViewModels
{
	public class AdvobotNetCoreWindowViewModel : ReactiveObject
	{
		private string _OutputValue = "";
		public string OutputValue
		{
			get => _OutputValue;
			set => this.RaiseAndSetIfChanged(ref _OutputValue, value);
		}

		private string _InputValue = "";
		public string InputValue
		{
			get => _InputValue;
			set => this.RaiseAndSetIfChanged(ref _InputValue, value);
		}

		private string _UptimeString = "Uptime";
		public string UptimeString
		{
			get => _UptimeString;
			set => this.RaiseAndSetIfChanged(ref _UptimeString, value);
		}

		private string _LatencyString = "Latency";
		public string LatencyString
		{
			get => _LatencyString;
			set => this.RaiseAndSetIfChanged(ref _LatencyString, value);
		}

		private string _MemoryString = "Memory";
		public string MemoryString
		{
			get => _MemoryString;
			set => this.RaiseAndSetIfChanged(ref _MemoryString, value);
		}

		private string _ThreadCountString = "Thread Count";
		public string ThreadCountString
		{
			get => _ThreadCountString;
			set => this.RaiseAndSetIfChanged(ref _ThreadCountString, value);
		}

		public ReactiveCommand OutputValueCommand { get; }
		public ReactiveCommand InputValueCommand { get; }

		public AdvobotNetCoreWindowViewModel()
		{
			OutputValueCommand = ReactiveCommand.Create<string>(x =>
			{
				OutputValue += x;
			});
			InputValueCommand = ReactiveCommand.Create<object>(x =>
			{
				OutputValue += Environment.NewLine + InputValue;
				InputValue = "";
			});

			System.Console.SetOut(new TextBoxStreamWriter(OutputValueCommand));
		}

		public void InputButtonPressed(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Return)
			{
				((ICommand)InputValueCommand).Execute(e.Key);
			}
		}
	}
}