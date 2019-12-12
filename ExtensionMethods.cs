using ExplorerExtender.Models;
using System.Collections.Generic;

namespace ExplorerExtender {
  internal static class ExtensionMethods {

    public static IEnumerable<CommandMenuItem> GetAllCommands(this IEnumerable<BaseMenuItem> source) {
      foreach (var item in source) {
        if (item is CommandMenuItem) {
          yield return (CommandMenuItem)item;
        } else if (item is SubmenuMenuItem) {
          foreach (var i in ((SubmenuMenuItem)item).Items.GetAllCommands()) {
            yield return i;
          }
        }
      }
    }


  }
}
