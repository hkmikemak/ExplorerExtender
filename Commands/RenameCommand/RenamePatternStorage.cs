using ExplorerExtender.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ExplorerExtender.Commands.RenameCommand {
  internal static class RenamePatternStorage {
    private static string GetSettingFile() => Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location), "RenamePattern.json");

    public static List<RenamePaternModel> Read() {
      if (!File.Exists(RenamePatternStorage.GetSettingFile())) {
        return new List<RenamePaternModel>();
      } else {
        return JsonConvert.DeserializeObject<List<RenamePaternModel>>(File.ReadAllText(GetSettingFile()));
      }
    }

    public static void Save(List<RenamePaternModel> items) {
      if (File.Exists(GetSettingFile())) {
        try { File.Delete(GetSettingFile()); } catch { }
      }

      using (StreamWriter streamWriter = new StreamWriter(GetSettingFile(), true)) {
        streamWriter.Write(JsonHelper.SerializeObject(items));
      }
    }
  }
}
