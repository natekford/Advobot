using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Advobot.UI.Colors;
using Newtonsoft.Json;

namespace Advobot.UI.AbstractUI.Colors
{
	/// <summary>
	/// Indicates what colors to use in the UI.
	/// </summary>
	public abstract class ColorSettings<TBrush, TBrushFactory>
		: IColorSettings<TBrush>, INotifyPropertyChanged
			where TBrushFactory : BrushFactory<TBrush>, new()
	{
		/// <summary>
		/// A light color UI theme.
		/// </summary>
		public static ITheme<TBrush> LightMode { get; } = new Theme<TBrush, TBrushFactory>
		{
			{ ColorTargets.BaseBackground,            "#FFFFFF" },
			{ ColorTargets.BaseForeground,            "#000000" },
			{ ColorTargets.BaseBorder,                "#ABADB3" },
			{ ColorTargets.ButtonBackground,          "#DDDDDD" },
			{ ColorTargets.ButtonForeground,          "#0E0E0E" },
			{ ColorTargets.ButtonBorder,              "#707070" },
			{ ColorTargets.ButtonDisabledBackground,  "#F4F4F4" },
			{ ColorTargets.ButtonDisabledForeground,  "#888888" },
			{ ColorTargets.ButtonDisabledBorder,      "#ADB2B5" },
			{ ColorTargets.ButtonMouseOverBackground, "#BEE6FD" },
			{ ColorTargets.JsonDigits,                "#8700FF" },
			{ ColorTargets.JsonValue,                 "#000CFF" },
			{ ColorTargets.JsonParamName,             "#057500" },
		};
		/// <summary>
		/// A dark color UI theme.
		/// </summary>
		public static ITheme<TBrush> DarkMode { get; } = new Theme<TBrush, TBrushFactory>
		{
			{ ColorTargets.BaseBackground,            "#1C1C1C" },
			{ ColorTargets.BaseForeground,            "#E1E1E1" },
			{ ColorTargets.BaseBorder,                "#ABADB3" },
			{ ColorTargets.ButtonBackground,          "#151515" },
			{ ColorTargets.ButtonForeground,          "#E1E1E1" },
			{ ColorTargets.ButtonBorder,              "#ABADB3" },
			{ ColorTargets.ButtonDisabledBackground,  "#343434" },
			{ ColorTargets.ButtonDisabledForeground,  "#A0A0A0" },
			{ ColorTargets.ButtonDisabledBorder,      "#ADB2B5" },
			{ ColorTargets.ButtonMouseOverBackground, "#303333" },
			{ ColorTargets.JsonDigits,                "#8700FF" },
			{ ColorTargets.JsonValue,                 "#0051FF" },
			{ ColorTargets.JsonParamName,             "#057500" },
		};
		/// <summary>
		/// Static instance of the brush factory.
		/// </summary>
		public static BrushFactory<TBrush> Factory { get; } = new TBrushFactory();

		static ColorSettings()
		{
			LightMode.Freeze();
			DarkMode.Freeze();
		}

		/// <inheritdoc />
		[JsonProperty("ColorTargets", Order = 1)] //Deserialize this first so when Theme gets set it will update the UI
		public ITheme<TBrush> UserDefinedColors { get; } = new Theme<TBrush, TBrushFactory>();
		/// <inheritdoc />
		[JsonProperty("Theme", Order = 2)]
		public ColorTheme ActiveTheme
		{
			get => _ActiveTheme;
			set
			{
				if (_ActiveTheme == value) //Don't bother reloading the theme if it's the same value
				{
					return;
				}

				var themeBrushes = (_ActiveTheme = value) switch
				{
					ColorTheme.LightMode => LightMode,
					ColorTheme.DarkMode => DarkMode,
					ColorTheme.UserMade => UserDefinedColors,
					_ => throw new ArgumentException(nameof(value)),
				};

				foreach (var kvp in themeBrushes)
				{
					UpdateResource(kvp.Key, kvp.Value);
				}
				RaisePropertyChanged();
			}
		}
		[JsonIgnore]
		private ColorTheme _ActiveTheme = ColorTheme.LightMode;

		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Creates an instance of <see cref="ColorSettings{TBrush, TBrushFactory}"/> and sets the default theme and colors to light.
		/// </summary>
		public ColorSettings()
		{
			foreach (var key in LightMode.Keys)
			{
				if (!UserDefinedColors.TryGetValue(key, out var val))
				{
					UserDefinedColors.Add(key, LightMode[key]);
				}
			}
			UserDefinedColors.PropertyChanged += (sender, e) =>
			{
				if (ActiveTheme == ColorTheme.UserMade)
				{
					UpdateResource(e.PropertyName, ((ITheme<TBrush>)sender)[e.PropertyName]);
				}
			};
		}

		/// <inheritdoc />
		public abstract void Save();
		/// <summary>
		/// Updates a resource dictionary with the specified value.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="value"></param>
		protected abstract void UpdateResource(string target, TBrush value);
		/// <summary>
		/// Raises the property changed event.
		/// </summary>
		/// <param name="caller"></param>
		protected void RaisePropertyChanged([CallerMemberName] string caller = "")
			=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
	}
}