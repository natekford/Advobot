using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

using Advobot.Settings;
using Advobot.UI.AbstractUI.Colors;
using Advobot.Utilities;

using AdvorangesUtils;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Styling;

using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Highlighting.Xshd;

namespace Advobot.UI.Colors
{
	/// <summary>
	/// Color settings for Advobot's .Net Core UI.
	/// </summary>
	public sealed class NetCoreColorSettings : ColorSettings<ISolidColorBrush, NetCoreBrushFactory>
	{
		private static readonly string _AssemblyName = Assembly.GetExecutingAssembly()
			.GetName().Name ?? "";

		private static readonly Dictionary<string, string[]> _ColorNameMappings = new()
		{
			{ ColorTargets.BaseBackground, new[] { "ThemeBackgroundBrush" } },
			//{ BaseForeground, "ThemeAccentBrush" },
			{ ColorTargets.BaseBorder, new[] { "ThemeBorderDarkBrush" } },
			{ ColorTargets.ButtonBackground, new[] { "ThemeControlMidBrush" } },
			{ ColorTargets.ButtonForeground, new[] { "ThemeForegroundBrush" } },
			{ ColorTargets.ButtonBorder, new[] { "ThemeBorderLightBrush" } },
			{ ColorTargets.ButtonMouseOverBackground, new[] { "ThemeBorderMidBrush" } },
		};

		private static readonly Lazy<IResourceDictionary> _Resources = new(() =>
		{
			var styles = Application.Current.Styles.OfType<StyleInclude>();
			var colors = styles.Single(x =>
			{
				var src = x.Source;
				return src?.ToString().CaseInsContains("BaseLight.xaml") == true;
			});
			return ((Style)colors.Loaded).Resources;
		});

		private IBotDirectoryAccessor? _DirectoryAccessor;

		static NetCoreColorSettings()
		{
			LoadSyntaxHighlighting($"{_AssemblyName}.Resources.JsonSyntaxHighlighting.xshd", "Json", new[] { ".json" });
		}

		/// <summary>
		/// Creates an instance of <see cref="NetCoreColorSettings"/>.
		/// </summary>
		private NetCoreColorSettings()
		{
			PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(ActiveTheme))
				{
					SetSyntaxHighlightingColors("Json");
				}
			};
		}

		/// <summary>
		/// Loads the UI settings from file.
		/// </summary>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public static NetCoreColorSettings CreateOrLoad(IBotDirectoryAccessor accessor)
		{
			var path = StaticGetFile(accessor);
			var instance = IOUtils.DeserializeFromFile<NetCoreColorSettings>(path);
			if (instance == null)
			{
				instance = new NetCoreColorSettings();
				instance.Save();
			}

			instance._DirectoryAccessor = accessor;
			return instance;
		}

		/// <inheritdoc />
		public override void Save()
		{
			if (_DirectoryAccessor == null)
			{
				throw new InvalidOperationException("Unable to save.");
			}

			var path = StaticGetFile(_DirectoryAccessor);
			IOUtils.SafeWriteAllText(path, IOUtils.Serialize(this));
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

		private static void LoadSyntaxHighlighting(string loc, string name, string[] extensions)
		{
			using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(loc);
			if (s is null)
			{
				return;
			}
			using var r = new XmlTextReader(s);

			var highlighting = HighlightingLoader.Load(r, HighlightingManager.Instance);
			HighlightingManager.Inst‌​ance.RegisterHighlighting(name, extensions, highlighting);
		}

		private static void SetSyntaxHighlightingColors(params string[] names)
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
					var appColor = ((ISolidColorBrush?)Application.Current.Resources[s])?.Color;
					var color = appColor ?? LightMode[s].Color;
					namedColor.Foreground = new SimpleHighlightingBrush(color);
				}
			}
		}

		private static FileInfo StaticGetFile(IBotDirectoryAccessor accessor)
			=> accessor.GetBaseBotDirectoryFile("UISettings.json");

		private static void UpdateSyntaxHighlightingColor(string target, ISolidColorBrush value)
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
	}
}