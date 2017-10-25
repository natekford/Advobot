using Advobot.Core.Actions;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Interfaces;
using Discord;
using System;
using System.Windows;
using System.Windows.Controls;
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
				UIModification.SetFontResizeProperty(this, value);
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
