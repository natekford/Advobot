using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using Discord;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes
{
	internal class AdvobotTextBox : TextBox, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				(this as Control).SetBinding(Control.FontSizeProperty, new Binding
				{
					Path = new PropertyPath("ActualHeight"),
					RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(Grid), 1),
					Converter = new FontResizer(value),
				});
				_FRV = value;
			}
		}

		public AdvobotTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
			this.TextWrapping = TextWrapping.Wrap;
		}

		public static AdvobotTextBox CreateTitleBox(string text, string summary)
		{
			var tb = new AdvobotTextBox
			{
				Text = text,
				IsReadOnly = true,
				BorderThickness = new Thickness(0),
				VerticalAlignment = VerticalAlignment.Center,
				HorizontalAlignment = HorizontalAlignment.Left,
				TextWrapping = TextWrapping.WrapWithOverflow,
			};

			if (!String.IsNullOrWhiteSpace(summary))
			{
				var tt = new ToolTip { Content = summary, };
				tb.MouseEnter += (sender, e) => UIModification.ToggleToolTip(tt);
				tb.MouseLeave += (sender, e) => UIModification.ToggleToolTip(tt);
			}

			return tb;
		}
		public static AdvobotTextBox CreateSettingBox(string settingName, int length)
		{
			return new AdvobotTextBox
			{
				VerticalContentAlignment = VerticalAlignment.Center,
				Tag = settingName,
				MaxLength = length
			};
		}
		public static AdvobotTextBox CreateSystemInfoBox()
		{
			return new AdvobotTextBox
			{
				IsReadOnly = true,
				BorderThickness = new Thickness(0, .5, 0, .5),
				Background = null,
			};
		}
		public static AdvobotTextBox CreateUserBox(IUser user)
		{
			return user == null ? null : new AdvobotTextBox
			{
				Text = String.Format("'{0}#{1}' ({2})",
					user.Username.AllCharactersAreWithinUpperLimit() ? user.Username : "Non-Standard Name",
					user.Discriminator,
					user.Id),
				Tag = user.Id,
				IsReadOnly = true,
				IsHitTestVisible = false,
				BorderThickness = new Thickness(0),
				Background = Brushes.Transparent,
				Foreground = Brushes.Black,
			};
		}
	}
}
