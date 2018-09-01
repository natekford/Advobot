using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Xml;
using Advobot.SharedUI.Colors;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;
using Newtonsoft.Json;

namespace Advobot.NetCoreUI.Classes.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Core UI.
	/// </summary>
	public sealed class NetCoreColorSettings : ColorSettings<ISolidColorBrush, NetCoreBrushFactory>
	{
		private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

		static NetCoreColorSettings()
		{
			LoadSyntaxHighlighting($"{AssemblyName}.Resources.JsonSyntaxHighlighting.xshd", "Json", new[] { ".json" });
		}

		[JsonIgnore]
		private IResourceDictionary _Resources;
		[JsonIgnore]
		private readonly ImmutableDictionary<string, string[]> _ColorNameMappings = new Dictionary<string, string[]>
		{
			{ ColorTargets.BaseBackground, new[] { "ThemeBackgroundBrush" } },
			//{ BaseForeground, "ThemeAccentBrush" },
			{ ColorTargets.BaseBorder, new[] { "ThemeBorderMidBrush", "ThemeBorderDarkBrush" } },
			{ ColorTargets.ButtonBackground, new[] { "ThemeControlMidBrush" } },
			{ ColorTargets.ButtonForeground, new[] { "ThemeForegroundBrush" } },
			{ ColorTargets.ButtonBorder, new[] { "ThemeBorderLightBrush" } },
			{ ColorTargets.ButtonMouseOverBackground, new[] { "ThemeBorderMidBrush" } },
		}.ToImmutableDictionary();

		/// <summary>
		/// Creates an instance of <see cref="NetCoreColorSettings"/> and sets the default theme and colors to light.
		/// </summary>
		public NetCoreColorSettings() : base() { }

		/// <inheritdoc />
		protected override void UpdateResource(string target, ISolidColorBrush value)
		{
			if (_Resources == null)
			{
				var styles = Application.Current.Styles.OfType<StyleInclude>();
				var colors = styles.Single(x => x.Source.ToString().CaseInsContains("BaseLight.xaml"));
				_Resources = ((Style)colors.Loaded).Resources;
			}

			//If this is remapped to take advantage of how it's easy to recolor already defined background, etc then do that
			if (_ColorNameMappings.TryGetValue(target, out var names))
			{
				foreach (var name in names)
				{
					_Resources[name] = value;
				}
			}

			//Still set it in the global resource dictionary if we want to access it easily with our names
			Application.Current.Resources[target] = value;
			UpdateSyntaxHighlightingColor(target, value);
		}
		/// <inheritdoc />
		protected override void AfterThemeUpdated()
			=> SetSyntaxHighlightingColors("Json");
		//TODO: remove returns after avaloniaedit is uploaded onto nuget past 0.6.0
		private void SetSyntaxHighlightingColors(params string[] names)
		{
			return;
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
					var color = ((ISolidColorBrush)Application.Current.Resources[str])?.Color ?? LightMode[str].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}
		private void UpdateSyntaxHighlightingColor(string target, ISolidColorBrush value)
		{
			return;
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
			return;
			using (var r = new XmlTextReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(loc))
				?? throw new InvalidOperationException($"{loc} is missing."))
			{
				var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
				HighlightingManager.Inst‌​ance.RegisterHighlighting(name, extensions, highlighting);
			}
		}
	}
}