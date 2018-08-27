using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Advobot.SharedUI.Colors;
using AdvorangesUtils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;
using Newtonsoft.Json;

namespace Advobot.NetCoreUI.Classes.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Core UI.
	/// </summary>
	public sealed class NetCoreColorSettings : ColorSettings<ISolidColorBrush, NetCoreBrushFactory>
	{
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
		}
	}
}