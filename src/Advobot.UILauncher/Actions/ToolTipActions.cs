using Advobot.UILauncher.Enums;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Advobot.UILauncher.Actions
{
	internal static class ToolTipActions
	{
		private static Dictionary<ToolTipReason, string> _ToolTipReasons = new Dictionary<ToolTipReason, string>
		{
			{ ToolTipReason.FileSavingFailure, "Failed to save the file." },
			{ ToolTipReason.FileSavingSuccess, "Successfully saved the file." },
			{ ToolTipReason.InvalidFilePath, "Unable to gather the path for this file." },
		};
		private static CancellationTokenSource _ToolTipCancellationTokenSource;

		public static async Task EnableTimedToolTip(FrameworkElement element, string text, int timeInMS = 2500)
		{
			if (!(element.ToolTip is ToolTip tt))
			{
				element.ToolTip = tt = new ToolTip { Placement = PlacementMode.Relative };
			}

			tt.Content = text;
			ToggleToolTip(tt);

			_ToolTipCancellationTokenSource?.Cancel();
			_ToolTipCancellationTokenSource = new CancellationTokenSource();

			await element.Dispatcher.InvokeAsync(async () =>
			{
				try
				{
					await Task.Delay(timeInMS, _ToolTipCancellationTokenSource.Token);
				}
				catch (TaskCanceledException)
				{
					return;
				}

				ToggleToolTip(tt);
			});
		}
		public static void ToggleToolTip(ToolTip ttip)
		{
			if (ttip.IsOpen)
			{
				ttip.IsOpen = false;
				ttip.Visibility = Visibility.Collapsed;
			}
			else
			{
				ttip.IsOpen = true;
				ttip.Visibility = Visibility.Visible;
			}
		}
		public static string GetReason(this ToolTipReason reason)
		{
			return _ToolTipReasons[reason];
		}
	}
}
