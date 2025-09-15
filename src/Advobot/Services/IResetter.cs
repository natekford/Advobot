using Advobot.Modules;

namespace Advobot.Services;

/// <summary>
/// Sets something in a command module to a recommended/default value.
/// </summary>
public interface IResetter
{
	/// <summary>
	/// Sets some option value.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	Task ResetAsync(IGuildContext context);
}