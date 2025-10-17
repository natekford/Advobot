namespace Advobot.Services;

/// <summary>
/// A service which can be started and stopped.
/// </summary>
public abstract class StartableService : IConfigurableService
{
	/// <summary>
	/// Token for cancelling active service items.
	/// </summary>
	protected CancellationToken CancellationToken => CancellationTokenSource?.Token ?? CancellationToken.None;
	/// <summary>
	/// Source for cancelling active service items.
	/// </summary>
	protected CancellationTokenSource? CancellationTokenSource { get; set; }
	/// <summary>
	/// Whether or not the service is active.
	/// </summary>
	protected bool IsRunning => CancellationTokenSource?.IsCancellationRequested == false;

	/// <summary>
	/// Start the service.
	/// </summary>
	public Task StartAsync()
	{
		if (IsRunning)
		{
			return Task.CompletedTask;
		}

		CancellationTokenSource = new CancellationTokenSource();
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

		CancellationTokenSource?.Cancel();
		return StopAsyncImpl();
	}

	Task IConfigurableService.ConfigureAsync()
		=> StartAsync();

	/// <summary>
	/// Repeats <paramref name="func"/> in the background.
	/// </summary>
	/// <param name="func"></param>
	protected virtual void RepeatInBackground(Func<Task> func)
	{
		_ = Task.Run(async () =>
		{
			while (IsRunning)
			{
				try
				{
					await func().ConfigureAwait(false);
				}
				catch (OperationCanceledException)
				{
				}
			}
		});
	}

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