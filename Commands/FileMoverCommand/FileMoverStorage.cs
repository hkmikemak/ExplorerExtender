using ExplorerExtender.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace ExplorerExtender.Commands.FileMoverCommand {
  internal static class FileMoverStorage {

    private static string GetSettingFile() => Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location), "FileMover.json");

    public static List<FileMoverModel> Read() {
      if (!File.Exists(FileMoverStorage.GetSettingFile())) {
        return new List<FileMoverModel>();
      } else {
        return JsonConvert.DeserializeObject<List<FileMoverModel>>(File.ReadAllText(GetSettingFile()));
      }
    }

    public static void Save(List<FileMoverModel> items) {
      if (!File.Exists(GetSettingFile())) {
        try { File.Delete(GetSettingFile()); } catch { }
      }

      using (StreamWriter streamWriter = new StreamWriter(GetSettingFile(), true)) {
        streamWriter.Write(JsonHelper.SerializeObject(items));
      }
    }

  }
}
