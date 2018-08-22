using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Advobot.SharedUI.Colors;
using AdvorangesUtils;
using ICSharpCode.AvalonEdit.Highlighting;

namespace Advobot.NetFrameworkUI.Classes.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Framework UI.
	/// </summary>
	public sealed class NetFrameworkColorSettings : ColorSettings<SolidColorBrush, NetFrameworkBrushFactory>
	{
		/// <summary>
		/// Creates an instance of <see cref="NetFrameworkColorSettings"/> and sets the default theme and colors to light.
		/// </summary>
		public NetFrameworkColorSettings() : base() { }

		/// <inheritdoc />
		protected override void UpdateResource(string target, SolidColorBrush value)
		{
			Application.Current.Resources[target] = value;
		}
		/// <inheritdoc />
		protected override void AfterThemeUpdated()
		{
			SetSyntaxHighlightingColors("Json");
		}
		private void SetSyntaxHighlightingColors(params string[] names)
		{
			foreach (var name in names)
			{
				var highlighting = HighlightingManager.Instance.GetDefinition(name) ?? throw new ArgumentException("not a valid highlighting.", name);

				foreach (var namedColor in highlighting.NamedHighlightingColors)
				{
					//E.G.: Highlighting name is json, color name is Param, searches for jsonParam
					var colorName = highlighting.Name + namedColor.Name;
					if (!(LightMode.Keys.SingleOrDefault(x => x.CaseInsEquals(colorName)) is string str))
					{
						continue;
					}

					//Get the set color, if one doesn't exist, use the default light mode
					var color = ((SolidColorBrush)Application.Current.Resources[str])?.Color ?? LightMode[str].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}
	}
}