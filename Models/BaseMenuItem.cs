using ExplorerExtender.Interop;

namespace ExplorerExtender.Models {
  internal abstract class BaseMenuItem {
    public bool Enabled { get; set; } = true;
    public abstract MENUITEMINFO ToMENUITEMINFO();
  }
}
