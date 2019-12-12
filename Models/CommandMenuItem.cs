using ExplorerExtender.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ExplorerExtender.Models {
  internal class CommandMenuItem : BaseMenuItem {
    public string Name { get; set; }
    public uint PositionId { get; set; }
    public uint CommandID { get; set; }
    public string HelpText { get; set; }
    public Action<List<string>, List<string>, bool> CommandMethod { get; set; }
    public override MENUITEMINFO ToMENUITEMINFO() {
      MENUITEMINFO result = new MENUITEMINFO();
      result.cbSize = (uint)Marshal.SizeOf(result);
      result.fMask = MIIM.MIIM_STRING | MIIM.MIIM_FTYPE | MIIM.MIIM_ID | MIIM.MIIM_STATE; ;
      result.fType = MFT.MFT_STRING;
      result.wID = this.PositionId;
      result.dwTypeData = this.Name;
      result.fState = this.Enabled ? MFS.MFS_ENABLED : MFS.MFS_DISABLED;
      return result;
    }
  }
}
