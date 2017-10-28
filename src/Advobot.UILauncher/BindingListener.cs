using System;
using System.Diagnostics;

namespace Advobot.UILauncher
{
	//Source: http://www.jasonbock.net/jb/News/Item/0f221e047de740ee90722b248933a28d
	internal sealed class BindingListener : DefaultTraceListener
	{
		private int InformationPropertyCount { get; set; }
		private bool IsFirstWrite { get; set; }
		private string Message { get; set; }

		public BindingListener(TraceOptions options) : base()
		{
			PresentationTraceSources.Refresh();
			PresentationTraceSources.DataBindingSource.Listeners.Add(this);
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

			IsFirstWrite = true;
			TraceOutputOptions = options;
			foreach (TraceOptions traceOptionValue in Enum.GetValues(typeof(TraceOptions)))
			{
				if (traceOptionValue == TraceOptions.None)
				{
					continue;
				}
				InformationPropertyCount += (TraceOutputOptions.HasFlag(traceOptionValue) ? 1 : 0);
			}
		}

		public override void WriteLine(string message)
		{
			if (IsFirstWrite)
			{
				Message = message;
				IsFirstWrite = false;
			}
			else
			{
				InformationPropertyCount--;
			}

			Flush();
			if (InformationPropertyCount == 0)
			{
				PresentationTraceSources.DataBindingSource.Listeners.Remove(this);
				throw new Exception(Message);
			}
		}
	}
}
