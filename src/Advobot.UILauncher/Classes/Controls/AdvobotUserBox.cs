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
			Text = $"'{name}#{user.Discriminator}' ({user.Id})";
			Tag = user.Id;
			IsReadOnly = true;
			IsHitTestVisible = false;
			BorderThickness = new Thickness(0);
			Background = Brushes.Transparent;
			Foreground = Brushes.Black;
		}
	}
}
