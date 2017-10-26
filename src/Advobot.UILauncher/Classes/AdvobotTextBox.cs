using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
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
		private TBType _T;
		public TBType TBType
		{
			get => _T;
			set
			{
				_T = value;
				ActivateTBType();
			}
		}
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				_FRV = value;
				UIModification.SetFontResizeProperty(this, _FRV);
			}
		}
		private string _S;
		public string Summary
		{
			get => _S;
			set
			{
				_S = value;
				this.ToolTip = new ToolTip { Content = _S, };
				this.MouseEnter += (sender, e) => UIModification.ToggleToolTip((ToolTip)this.ToolTip);
				this.MouseLeave += (sender, e) => UIModification.ToggleToolTip((ToolTip)this.ToolTip);
			}
		}

		public AdvobotTextBox()
		{
			this.Background = null;
			this.Foreground = null;
			this.BorderBrush = null;
		}

		public override void EndInit()
		{
			ActivateTitle();
			base.EndInit();
		}
		private void ActivateTBType()
		{
			switch (_T)
			{
				case TBType.Title:
				{
					this.IsReadOnly = true;
					this.BorderThickness = new Thickness(0);
					this.VerticalContentAlignment = VerticalAlignment.Center;
					this.TextWrapping = TextWrapping.WrapWithOverflow;
					return;
				}
				case TBType.RightCentered:
				{
					this.IsReadOnly = true;
					this.HorizontalContentAlignment = HorizontalAlignment.Right;
					this.VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.LeftCentered:
				{
					this.IsReadOnly = true;
					this.HorizontalContentAlignment = HorizontalAlignment.Left;
					this.VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.CenterCentered:
				{
					this.IsReadOnly = true;
					this.HorizontalContentAlignment = HorizontalAlignment.Center;
					this.VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.Background:
				{
					this.IsReadOnly = true;
					this.BorderThickness = new Thickness(1, 1, 1, 1);
					return;
				}
				case TBType.Nothing:
				default:
				{
					return;
				}
			}
		}
		private void ActivateTitle()
		{
			switch (_T)
			{
				case TBType.Title:
				{
					this.Text = String.IsNullOrWhiteSpace(this.Text)
						? this.Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":"
						: this.Text;
					return;
				}
				case TBType.RightCentered:
				case TBType.LeftCentered:
				case TBType.Nothing:
				default:
				{
					return;
				}
			}
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
