using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Advobot.NetCoreUI.Classes.AbstractUI.Colors;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Advobot.Utilities;

namespace Advobot.NetCoreUI.Classes.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Core UI.
	/// </summary>
	public sealed class NetCoreColorSettings : ColorSettings<ISolidColorBrush, NetCoreBrushFactory>
	{
		private static readonly string _AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
		private static readonly Dictionary<string, string[]> _ColorNameMappings = new Dictionary<string, string[]>
		{
			{ ColorTargets.BaseBackground, new[] { "ThemeBackgroundBrush" } },
			//{ BaseForeground, "ThemeAccentBrush" },
			{ ColorTargets.BaseBorder, new[] { "ThemeBorderDarkBrush" } },
			{ ColorTargets.ButtonBackground, new[] { "ThemeControlMidBrush" } },
			{ ColorTargets.ButtonForeground, new[] { "ThemeForegroundBrush" } },
			{ ColorTargets.ButtonBorder, new[] { "ThemeBorderLightBrush" } },
			{ ColorTargets.ButtonMouseOverBackground, new[] { "ThemeBorderMidBrush" } },
		};
		private static readonly Lazy<IResourceDictionary> _Resources = new Lazy<IResourceDictionary>(() =>
		{
			var styles = Application.Current.Styles.OfType<StyleInclude>();
			var colors = styles.Single(x => x.Source.ToString().CaseInsContains("BaseLight.xaml"));
			return ((Style)colors.Loaded).Resources;
		});

		static NetCoreColorSettings()
		{
			LoadSyntaxHighlighting($"{_AssemblyName}.Resources.JsonSyntaxHighlighting.xshd", "Json", new[] { ".json" });
		}

		/// <summary>
		/// Creates an instance of <see cref="NetCoreColorSettings"/>.
		/// </summary>
		public NetCoreColorSettings()
		{
			PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(ActiveTheme))
				{
					SetSyntaxHighlightingColors("Json");
				}
			};
		}

		/// <inheritdoc />
		protected override void UpdateResource(string target, ISolidColorBrush value)
		{
			//If this is remapped to take advantage of how it's easy to recolor already defined background, etc then do that
			if (_ColorNameMappings.TryGetValue(target, out var names))
			{
				foreach (var name in names)
				{
					_Resources.Value[name] = value;
				}
			}

			//Still set it in the global resource dictionary if we want to access it easily with our names
			Application.Current.Resources[target] = value;
			UpdateSyntaxHighlightingColor(target, value);
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
					if (!LightMode.Keys.TryGetSingle(x => x.CaseInsEquals(colorName), out var s))
					{
						continue;
					}

					//Get the set color, if one doesn't exist, use the default light mode
					var color = ((ISolidColorBrush)Application.Current.Resources[s])?.Color ?? LightMode[s].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}
		private void UpdateSyntaxHighlightingColor(string target, ISolidColorBrush value)
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