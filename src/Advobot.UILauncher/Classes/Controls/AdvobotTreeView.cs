using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="TreeView"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotTreeView : TreeView, IFontResizeValue
	{
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				EntityActions.SetFontResizeProperty(this, value);
				_FRV = value;
			}
		}

		public AdvobotTreeView()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}

		public static TreeViewItem CreateGuildHeader(SocketGuild guild, DirectoryInfo dirInfo)
		{
			var header = new TreeViewItem
			{
				Header = guild.FormatGuild(),
				Tag = new GuildHeaderInfo(guild, dirInfo),
				HorizontalContentAlignment = HorizontalAlignment.Left,
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			header.SetResourceReference(TreeViewItem.BackgroundProperty, ColorTarget.BaseBackground);
			header.SetResourceReference(TreeViewItem.ForegroundProperty, ColorTarget.BaseForeground);
			return header;
		}
		public static TreeViewItem CreateGuildFile(SocketGuild guild, FileInfo fileInfo)
		{
			var file = new TreeViewItem
			{
				Header = fileInfo.Name,
				Tag = fileInfo,
				HorizontalContentAlignment = HorizontalAlignment.Left,
				VerticalContentAlignment = VerticalAlignment.Center,
			};
			file.SetResourceReference(TreeViewItem.BackgroundProperty, ColorTarget.BaseBackground);
			file.SetResourceReference(TreeViewItem.ForegroundProperty, ColorTarget.BaseForeground);
			return file;
		}

		public static IEnumerable<TreeViewItem> MakeGuildTreeViewItemsSource(IEnumerable<IGuild> guilds)
		{
			var r = Application.Current.Resources;
			return GetActions.GetBaseBotDirectory().GetDirectories().Select(dir =>
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
				}).Where(x => x?.Tag != null);

				return !items.Any() ? null : new TreeViewItem
				{
					Header = guild.FormatGuild(),
					Tag = guild,
					Background = (Brush)r[ColorTarget.BaseBackground],
					Foreground = (Brush)r[ColorTarget.BaseForeground],
					ItemsSource = items,
				};
			}).Where(x => x?.Tag != null).OrderByDescending(x => x.Tag is SocketGuild g ? g.MemberCount : 0);
		}
	}

	internal struct GuildHeaderInfo
	{
		public readonly SocketGuild Guild;
		public readonly DirectoryInfo DirectoryInfo;

		public GuildHeaderInfo(SocketGuild guild, DirectoryInfo directoryInfo)
		{
			Guild = guild;
			DirectoryInfo = directoryInfo;
		}
	}
}
