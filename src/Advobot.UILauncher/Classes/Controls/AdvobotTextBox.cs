using Advobot.Core.Utilities;
using Advobot.Core.Utilities.Formatting;
using Advobot.UILauncher.Utilities;
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
				ElementUtils.SetFontResizeProperty(this, _FRV);
			}
		}
		private string _S;
		public string Summary
		{
			get => _S;
			set
			{
				_S = value;
				ToolTip = new ToolTip { Content = _S, };
				MouseEnter += (sender, e) => ((ToolTip)ToolTip).EnableToolTip();
				MouseLeave += (sender, e) => ((ToolTip)ToolTip).DisableToolTip();
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
			switch (_T)
			{
				case TBType.Title:
				{
					IsReadOnly = true;
					BorderThickness = new Thickness(0);
					VerticalContentAlignment = VerticalAlignment.Center;
					TextWrapping = TextWrapping.WrapWithOverflow;
					return;
				}
				case TBType.RightCentered:
				{
					IsReadOnly = true;
					HorizontalContentAlignment = HorizontalAlignment.Right;
					VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.LeftCentered:
				{
					IsReadOnly = true;
					HorizontalContentAlignment = HorizontalAlignment.Left;
					VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.CenterCentered:
				{
					IsReadOnly = true;
					HorizontalContentAlignment = HorizontalAlignment.Center;
					VerticalContentAlignment = VerticalAlignment.Center;
					return;
				}
				case TBType.Background:
				{
					IsReadOnly = true;
					BorderThickness = new Thickness(1, 1, 1, 1);
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
					Text = String.IsNullOrWhiteSpace(Text)
						? Name.FormatTitle().CaseInsReplace("title", "").Trim() + ":"
						: Text;
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
			SetResourceReference(Control.BackgroundProperty, ColorTarget.BaseBackground);
			SetResourceReference(Control.ForegroundProperty, ColorTarget.BaseForeground);
			SetResourceReference(Control.BorderBrushProperty, ColorTarget.BaseBorder);
		}
	}
}
