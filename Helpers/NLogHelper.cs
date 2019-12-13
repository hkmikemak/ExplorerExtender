using NLog;
using System.Diagnostics;
using System.IO;

namespace ExplorerExtender.Helpers {
  internal static class NLogHelper {


    public static void LogToEvent(string message) {
      using (EventLog eventLog = new EventLog("Application")) {
        eventLog.Source = "Application";
        eventLog.WriteEntry(message, EventLogEntryType.Information, 101, 1);
      }
    }

    public static void LoadNLog() {
      string configuration = Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location), "NLog.config");
      LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(configuration, true);
      LogManager.ReconfigExistingLoggers();
    }


  }
}
