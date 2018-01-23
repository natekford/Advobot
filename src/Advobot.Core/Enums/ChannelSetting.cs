using System;

namespace Advobot.Core.Enums
{
	/// <summary>
	/// Specifies what external settings to use on a channel.
	/// </summary>
	[Flags]
	public enum ChannelSetting : uint
	{
		ImageOnly = (1U << 0)
	}
}
