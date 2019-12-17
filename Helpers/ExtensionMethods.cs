using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExplorerExtender.Helpers {
  internal static class ExtensionMethods {
    public static string ToDetailedString(this Exception exception) {
      StringBuilder result = new StringBuilder();
      result.AppendLine(string.Join(Environment.NewLine, exception.GetAllExceptions().Select(i => i.Message)));
      StackTrace st = new StackTrace(exception, true);
      StackFrame frame = st.GetFrame(0);
      int line = frame.GetFileLineNumber();
      result.AppendLine(line.ToString());
      return result.ToString();
    }

    public static IEnumerable<Exception> GetAllExceptions(this Exception exception) {
      yield return exception;

      if (exception is AggregateException aggEx) {
        foreach (Exception innerEx in aggEx.InnerExceptions.SelectMany(i => i.GetAllExceptions())) {
          yield return innerEx;
        }
      } else if (exception.InnerException != null) {
        foreach (Exception innerEx in exception.InnerException.GetAllExceptions()) {
          yield return innerEx;
        }
      }
    }
  }
}
