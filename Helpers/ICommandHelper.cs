using ExplorerExtender.Commands;
using ExplorerExtender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerExtender.Helpers {
  internal static class ICommandHelper {
    public static List<BaseMenuItem> BuildMenu(List<string> files, List<string> folders, bool IsClickOnEmptyArea) {
      return AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(i => i.GetTypes())
                        .Where(i => typeof(ICommand).IsAssignableFrom(i) && !i.IsAbstract && !i.IsInterface)
                        .Select(i => Activator.CreateInstance(i))
                        .SelectMany(i => ((ICommand)i).BuildMenu(files, folders, IsClickOnEmptyArea))
                        .ToList();
    }
  }
}
