using ExplorerExtender.Interop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ExplorerExtender.Models {
  internal class SubmenuMenuItem : BaseMenuItem {
    public override MENUITEMINFO ToMENUITEMINFO() {
      IntPtr ptrResult = NativeMethods.CreatePopupMenu();
      MENUITEMINFO result = new MENUITEMINFO();
      result.cbSize = (uint)Marshal.SizeOf(result);
      result.fMask = MIIM.MIIM_STRING | MIIM.MIIM_ID | MIIM.MIIM_SUBMENU;
      result.hSubMenu = ptrResult;
      result.fType = MFT.MFT_STRING;
      result.dwTypeData = this.Name;
      result.fState = this.Enabled ? MFS.MFS_ENABLED : MFS.MFS_DISABLED;

      for (uint i = 0; i < this.Items.Count; i++) {
        MENUITEMINFO subItem = this.Items[(int)i].ToMENUITEMINFO();
        NativeMethods.InsertMenuItem(ptrResult, i, true, ref subItem);
      }

      return result;
    }

    public string Name { get; set; }
    public List<BaseMenuItem> Items { get; set; }
  }
}
