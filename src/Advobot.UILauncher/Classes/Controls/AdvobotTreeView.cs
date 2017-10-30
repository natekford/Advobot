using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Enums;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes.Controls
{
	internal class AdvobotTreeView : TreeView
	{
		public AdvobotTreeView()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}

		public static IEnumerable<TreeViewItem> MakeGuildTreeViewItemsSource(IEnumerable<IGuild> guilds)
		{
			var directoryInfo = GetActions.GetBaseBotDirectory();
			if (directoryInfo == null || !directoryInfo.Exists)
			{
				return null;
			}

			var r = Application.Current.Resources;
			return directoryInfo.GetDirectories().Select(dir =>
			{
				//Make sure the id leads to a valid non null guild
				if (!ulong.TryParse(dir.Name, out ulong Id) || !(guilds.SingleOrDefault(x => x.Id == Id) is SocketGuild guild))
				{
					return null;
				}

				var items = dir.GetFiles().Select(file =>
				{
					return new TreeViewItem
					{
						Header = file.Name,
						Tag = file,
						Background = (Brush)r[ColorTarget.BaseBackground],
						Foreground = (Brush)r[ColorTarget.BaseForeground],
						//No idea why these are needed, but the bindinglistener throws exceptions when they're not
						HorizontalContentAlignment = HorizontalAlignment.Left,
						VerticalContentAlignment = VerticalAlignment.Center,
					};
				}).Where(x => x.Tag != null);

				return !items.Any() ? null : new TreeViewItem
				{
					Header = guild.FormatGuild(),
					Tag = guild,
					Background = (Brush)r[ColorTarget.BaseBackground],
					Foreground = (Brush)r[ColorTarget.BaseForeground],
					ItemsSource = items,
				};
			}).Where(x => x != null).OrderByDescending(x => x.Tag is SocketGuild g ? g.MemberCount : 0);
		}
	}
}
