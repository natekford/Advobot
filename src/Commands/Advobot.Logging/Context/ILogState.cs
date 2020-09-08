using System.Threading.Tasks;

using Advobot.Logging.Service;

using Discord;

namespace Advobot.Logging.Context
{
	public interface ILogState
	{
		IGuild? Guild { get; }
		bool IsValid { get; }

		Task<bool> CanLog(ILoggingService service, ILogContext context);
	}
}