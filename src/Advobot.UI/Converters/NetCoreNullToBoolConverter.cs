﻿using Advobot.UI.AbstractUI.Converters;

using Avalonia.Data.Converters;

namespace Advobot.UI.Converters;

/// <summary>
/// Returns true if the object is not null or whitespace.
/// </summary>
public sealed class NetCoreNullToBoolConverter : NullToBoolConverter, IValueConverter
{ }