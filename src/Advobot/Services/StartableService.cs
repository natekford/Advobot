namespace Advobot.Services;

/// <summary>
/// A service which can be started and stopped.
/// </summary>
public abstract class StartableService : IConfigurableService
{
	/// <summary>
	/// Whether or not the service is active.
	/// </summary>
	protected bool IsRunning { get; private set; }

	/// <summary>
	/// Start the service.
	/// </summary>
	public Task StartAsync()
	{
		if (IsRunning)
		{
			return Task.CompletedTask;
		}

		IsRunning = true;
		return StartAsyncImpl();
	}

	/// <summary>
	/// Stop the service.
	/// </summary>
	public Task StopAsync()
	{
		if (!IsRunning)
		{
			return Task.CompletedTask;
		}

		IsRunning = false;
		return StopAsyncImpl();
	}

	Task IConfigurableService.ConfigureAsync()
		=> StartAsync();

	/// <summary>
	/// Start the service after <see cref="IsRunning"/> has been checked.
	/// </summary>
	/// <returns></returns>
	protected abstract Task StartAsyncImpl();

	/// <summary>
	/// Stop the service after <see cref="IsRunning"/> has been checked.
	/// </summary>
	/// <returns></returns>
	protected virtual Task StopAsyncImpl()
		=> Task.CompletedTask;
}