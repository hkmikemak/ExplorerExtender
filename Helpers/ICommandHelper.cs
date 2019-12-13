using ExplorerExtender.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExplorerExtender.Helpers {
  internal static class ICommandHelper {
    public static List<ICommand> CreateAllCommandInstances() => typeof(Main).Assembly.GetTypes().Where(i => typeof(ICommand).IsAssignableFrom(i) && !i.IsAbstract && !i.IsInterface).Select(i => (ICommand)Activator.CreateInstance(i)).ToList();
  }
}
