using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Advobot.NetFrameworkUI.Enums;

namespace Advobot.NetFrameworkUI.Utilities
{
	internal static class ToolTipUtils
	{
		private static readonly ImmutableDictionary<ToolTipReason, string> _ToolTipReasons = new Dictionary<ToolTipReason, string>
		{
			{ ToolTipReason.FileSavingFailure, "Failed to save the file." },
			{ ToolTipReason.FileSavingSuccess, "Successfully saved the file." },
			{ ToolTipReason.InvalidFilePath, "Unable to gather the path for this file." }
		}.ToImmutableDictionary();
		private static CancellationTokenSource _ToolTipCancellationTokenSource;

		public static void EnableTimedToolTip(FrameworkElement element, string text, int timeInMS = 2500)
		{
			if (!(element.ToolTip is ToolTip tt))
			{
				element.ToolTip = tt = new ToolTip
				{
					IsOpen = false,
					Visibility = Visibility.Collapsed,
					Placement = PlacementMode.Relative
				};
			}

			tt.Content = text;
			tt.EnableToolTip();

			_ToolTipCancellationTokenSource?.Cancel();
			_ToolTipCancellationTokenSource = new CancellationTokenSource();

			Task.Run(async () =>
			{
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

					tt.DisableToolTip();
				});
			});
		}
		/// <summary>
		/// Opens the tooltip and makes it visible.
		/// </summary>
		/// <param name="tt"></param>
		public static void EnableToolTip(this ToolTip tt)
		{
			tt.IsOpen = true;
			tt.Visibility = Visibility.Visible;
		}
		/// <summary>
		/// Closes the tooltip and makes it collapsed.
		/// </summary>
		/// <param name="tt"></param>
		public static void DisableToolTip(this ToolTip tt)
		{
			tt.IsOpen = false;
			tt.Visibility = Visibility.Collapsed;
		}
		/// <summary>
		/// Gets a more verbose explanation.
		/// </summary>
		/// <param name="reason"></param>
		/// <returns></returns>
		public static string GetReason(this ToolTipReason reason)
		{
			return _ToolTipReasons[reason];
		}
	}
}
