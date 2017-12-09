using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.UILauncher.Actions;
using Advobot.UILauncher.Enums;
using Advobot.UILauncher.Interfaces;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Advobot.UILauncher.Classes.Controls
{
	/// <summary>
	/// A <see cref="TextBox"/> which implements some other useful properties and accepts custom colors easily.
	/// </summary>
	internal class AdvobotTextBox : TextBox, IFontResizeValue, IAdvobotControl
	{
		private TBType _T;
		public TBType TBType
		{
			get => _T;
			set
			{
				this._T = value;
				ActivateTBType();
			}
		}
		private double _FRV;
		public double FontResizeValue
		{
			get => _FRV;
			set
			{
				this._FRV = value;
				EntityActions.SetFontResizeProperty(this, this._FRV);
			}
		}
		private string _S;
		public string Summary
		{
			get => _S;
			set
			{
				this._S = value;
				this.ToolTip = new ToolTip { Content = this._S, };
				this.MouseEnter += (sender, e) => ((ToolTip)this.ToolTip).EnableToolTip();
				this.MouseLeave += (sender, e) => ((ToolTip)this.ToolTip).DisableToolTip();
			}
		}

		public AdvobotTextBox()
		{
			SetResourceReferences();
		}

		public override void EndInit()
		{
			base.EndInit();
			ActivateTitle();
		}
		private void ActivateTBType()
		{
			switch (this._T)
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
			switch (this._T)
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
		public void SetResourceReferences()
		{
			this.SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			this.SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			this.SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}
	}
}
