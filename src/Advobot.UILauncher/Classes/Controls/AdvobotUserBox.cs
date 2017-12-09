using Advobot.Core.Actions;
using Discord;
using System.Windows;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="AdvobotTextBox"/> which only accepts numbers as input.
	/// </summary>
	internal class AdvobotUserBox : AdvobotTextBox
	{
		public AdvobotUserBox(IUser user)
		{
			var name = user.Username.AllCharactersAreWithinUpperLimit() ? user.Username : "Non-Standard Name";
			this.Text = $"'{name}#{user.Discriminator}' ({user.Id})";
			this.Tag = user.Id;
			this.IsReadOnly = true;
			this.IsHitTestVisible = false;
			this.BorderThickness = new Thickness(0);
			this.Background = Brushes.Transparent;
			this.Foreground = Brushes.Black;
		}
	}
}
