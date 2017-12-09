using System;
using System.Diagnostics;
using System.Reflection;

namespace Advobot.UILauncher.Classes
{
	//Source: http://www.jasonbock.net/jb/News/Item/0f221e047de740ee90722b248933a28d
	internal sealed class BindingListener : DefaultTraceListener
	{
		private const TraceOptions OPTIONS = 0
			| TraceOptions.Callstack
			| TraceOptions.DateTime
			| TraceOptions.LogicalOperationStack
			| TraceOptions.ProcessId
			| TraceOptions.ThreadId
			| TraceOptions.Timestamp;

		private int InformationPropertyCount { get; set; }
		private string Callstack { get; set; }
		private string DateTime { get; set; }
		private string LogicalOperationStack { get; set; }
		private string Message { get; set; }
		private string ProcessId { get; set; }
		private string ThreadId { get; set; }
		private string Timestamp { get; set; }

		public BindingListener() : base()
		{
			PresentationTraceSources.Refresh();
			PresentationTraceSources.DataBindingSource.Listeners.Add(this);
			PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

			this.TraceOutputOptions = OPTIONS;
			foreach (TraceOptions traceOption in Enum.GetValues(typeof(TraceOptions)))
			{
				if (traceOption == TraceOptions.None)
				{
					continue;
				}
				this.InformationPropertyCount += (this.TraceOutputOptions.HasFlag(traceOption) ? 1 : 0);
			}
		}

		public override void WriteLine(string message)
		{
			if (this.Message == null)
			{
				this.Message = message;
			}
			else
			{
				var propertyInformation = message.Split(new string[] { "=" }, StringSplitOptions.None);

				if (propertyInformation.Length == 1)
				{
					this.LogicalOperationStack = propertyInformation[0];
				}
				else
				{
					var flags = BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance;
					GetType().GetProperty(propertyInformation[0], flags).SetValue(this, propertyInformation[1], null);
				}

				--this.InformationPropertyCount;
			}

			Flush();
			if (this.InformationPropertyCount == 0)
			{
				PresentationTraceSources.DataBindingSource.Listeners.Remove(this);

				var exceptionMessage = $"{this.Message}\n" +
					$"Time: {this.DateTime}\n" +
					$"Logical Operation Stack: {this.LogicalOperationStack}\n" +
					$"Process Id: {this.ProcessId}\n" +
					$"Thread Id: {this.ThreadId}\n" +
					$"Timestamp: {this.Timestamp}\n" +
					$"Callstack: {this.Callstack}";
				throw new Exception(exceptionMessage);
			}
		}
	}
}
