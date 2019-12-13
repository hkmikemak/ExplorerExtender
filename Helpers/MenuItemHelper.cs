using ExplorerExtender.Models;
using System.Collections.Generic;

namespace ExplorerExtender.Helpers {
  internal static class MenuItemHelper {
    public static IEnumerable<CommandMenuItem> GetAllCommands(this IEnumerable<BaseMenuItem> source) {
      foreach (BaseMenuItem item in source) {
        if (item is CommandMenuItem) {
          yield return (CommandMenuItem)item;
        } else if (item is SubmenuMenuItem) {
          foreach (CommandMenuItem i in ((SubmenuMenuItem)item).Items.GetAllCommands()) {
            yield return i;
          }
        }
      }
    }
  }
}
