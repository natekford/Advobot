using System.Threading.Tasks;

using Advobot.Logging.Service;

using Discord;

namespace Advobot.Logging.Context
{
	public interface ILoggingState
	{
		IGuild? Guild { get; }
		bool IsValid { get; }

		Task<bool> CanLog(ILoggingService service, ILoggingContext context);
	}
}