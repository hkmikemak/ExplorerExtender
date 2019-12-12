
using ExplorerExtender.Interop;
using System.Runtime.InteropServices;

namespace ExplorerExtender.Models {
  internal class SeparatorMenuItem : BaseMenuItem {
    public override MENUITEMINFO ToMENUITEMINFO() {
      MENUITEMINFO result = new MENUITEMINFO();
      result.cbSize = (uint)Marshal.SizeOf(result);
      result.fMask = MIIM.MIIM_TYPE;
      result.fType = MFT.MFT_SEPARATOR;
      result.fState = this.Enabled ? MFS.MFS_ENABLED : MFS.MFS_DISABLED;
      return result;
    }
  }
}
