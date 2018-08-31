using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using Advobot.SharedUI.Colors;
using AdvorangesUtils;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

namespace Advobot.NetFrameworkUI.Classes.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Framework UI.
	/// </summary>
	public sealed class NetFrameworkColorSettings : ColorSettings<SolidColorBrush, NetFrameworkBrushFactory>
	{
		private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		static NetFrameworkColorSettings()
		{
			LoadSyntaxHighlighting($"{AssemblyName}.Resources.JsonSyntaxHighlighting.xshd", "Json", new[] { ".json" });
		}

		/// <summary>
		/// Creates an instance of <see cref="NetFrameworkColorSettings"/> and sets the default theme and colors to light.
		/// </summary>
		public NetFrameworkColorSettings() : base() { }

		/// <inheritdoc />
		protected override void UpdateResource(string target, SolidColorBrush value)
		{
			Application.Current.Resources[target] = value;
			UpdateSyntaxHighlightingColor(target, value);
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
				var highlighting = HighlightingManager.Instance.GetDefinition(name)
					?? throw new ArgumentException($"{name} is not a valid highlighting.", name);

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
		private void UpdateSyntaxHighlightingColor(string target, SolidColorBrush value)
		{
			foreach (var highlighting in HighlightingManager.Instance.HighlightingDefinitions)
			{
				if (!target.CaseInsStartsWith(highlighting.Name))
				{
					continue;
				}
				foreach (var color in highlighting.NamedHighlightingColors)
				{
					if (!target.CaseInsEquals(highlighting.Name + color.Name))
					{
						continue;
					}
					color.Foreground = new SimpleHighlightingBrush(value.Color);
				}
			}
		}
		private static void LoadSyntaxHighlighting(string loc, string name, string[] extensions)
		{
			using (var r = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(loc))
				?? throw new InvalidOperationException($"{loc} is missing."))
			{
				var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
				HighlightingManager.Inst‌​ance.RegisterHighlighting(name, extensions, highlighting);
			}
		}
	}
}