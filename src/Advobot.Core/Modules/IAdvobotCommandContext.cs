namespace Advobot.Modules;

/// <summary>
/// Holds guild specific user/channel and guild settings.
/// </summary>
public interface IAdvobotCommandContext : IGuildCommandContext, IElapsed;