using ExplorerExtender.Models;
using System.Collections.Generic;

namespace ExplorerExtender.Commands {

  internal interface ICommand {
    IEnumerable<BaseMenuItem> BuildMenu(List<string> files, List<string> folders, bool isClickOnEmptyArea);
  }

}
