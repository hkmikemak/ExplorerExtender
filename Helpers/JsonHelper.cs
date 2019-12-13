using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace ExplorerExtender.Helpers {
  internal static class JsonHelper {
    public static string SerializeObject<T>(T value) {
      StringBuilder sb = new StringBuilder(256);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault();
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw)) {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.IndentChar = ' ';
        jsonWriter.Indentation = 2;

        jsonSerializer.Serialize(jsonWriter, value, typeof(T));
      }

      return sw.ToString();
    }

  }
}
