using ExplorerExtender.Commands;
using ExplorerExtender.Helpers;
using ExplorerExtender.Interop;
using ExplorerExtender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ExplorerExtender {

  [ClassInterface(ClassInterfaceType.None)]
  [ComVisible(true)]
  [Guid("23a8b362-4cce-454a-b9d9-7490bfde4d83")]
  public class Main : IContextMenu, IShellExtInit {
    internal List<string> Files = new List<string>();
    internal List<string> Folders = new List<string>();
    internal bool IsClickOnEmptyArea = false;
    internal List<CommandMenuItem> Menu = new List<CommandMenuItem>();

    #region ComRegister

    [ComRegisterFunction]
    public static void Register(Type t) {
      ShellExtReg.RegisterShellExtContextMenuHandler(t.GUID, "DIRECTORY", "ExplorerExtender.Main");
      ShellExtReg.RegisterShellExtContextMenuHandler(t.GUID, @"DIRECTORY\Background", "ExplorerExtender.Main");
      ShellExtReg.RegisterShellExtContextMenuHandler(t.GUID, "*", "ExplorerExtender.Main");
    }

    [ComUnregisterFunction]
    public static void Unregister(Type t) {
      ShellExtReg.UnregisterShellExtContextMenuHandler(t.GUID, "DIRECTORY");
      ShellExtReg.UnregisterShellExtContextMenuHandler(t.GUID, @"DIRECTORY\Background");
      ShellExtReg.UnregisterShellExtContextMenuHandler(t.GUID, "*");
    }

    #endregion
    
    #region IShellInit
    /// <summary>
    /// Step 1 - Get Selected Items (Files or Folder), also determine if user is click on the empty area
    /// </summary>
    /// <param name="pidlFolder"></param>
    /// <param name="pDataObj"></param>
    /// <param name="hKeyProgID"></param>
    public void Initialize(IntPtr pidlFolder, IntPtr pDataObj, IntPtr hKeyProgID) {
      (this.Files, this.Folders, this.IsClickOnEmptyArea) = NativeHelpers.ProcessSelectedItems(pidlFolder, pDataObj);
    }
    #endregion

    #region IContextMenu
    /// <summary>
    /// Step 2 - Build Menu
    /// </summary>
    /// <param name="hMenu"></param>
    /// <param name="iMenu"></param>
    /// <param name="idCmdFirst"></param>
    /// <param name="idCmdLast"></param>
    /// <param name="uFlags"></param>
    /// <returns></returns>
    public int QueryContextMenu(IntPtr hMenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags) {

      if (((uint)CMF.CMF_DEFAULTONLY & uFlags) != 0) {
        return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 0);
      }
      
      SubmenuMenuItem mainMenu = new SubmenuMenuItem {
        Enabled = true,
        Items = ICommandHelper.BuildMenu(this.Files, this.Folders, this.IsClickOnEmptyArea),
        Name = "Explorer Extender"
      };

      this.Menu = mainMenu.Items.GetAllCommands().ToList();

      uint commandCounter = 0;
      foreach (var item in this.Menu) {
        item.PositionId = idCmdFirst + commandCounter;
        item.CommandID = commandCounter++;
      }
;
      MENUITEMINFO miiMainMenu = mainMenu.ToMENUITEMINFO();

      NativeMethods.InsertMenuItem(hMenu, iMenu, true, ref miiMainMenu);
      return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, commandCounter);
    }

    /// <summary>
    /// Step 3 - Trigger when user mouse over a item for a while to display tooltips
    /// </summary>
    /// <param name="idCmd">Local Command ID (Starts from zero)</param>
    /// <param name="uFlags">Flags showing wherever user want us to show Command Name or Help Text</param>
    /// <param name="pReserved"></param>
    /// <param name="pszName">Use to put our result</param>
    /// <param name="cchMax">Maxinum number of size</param>
    public void GetCommandString(UIntPtr idCmd, uint uFlags, IntPtr pReserved, StringBuilder pszName, uint cchMax) {
      var command = this.Menu.FirstOrDefault(i => i.CommandID == idCmd.ToUInt32());

      if (command != null) {
        switch ((GCS)uFlags) {
          case GCS.GCS_VERBW:
            if (command.Name.Length > cchMax - 1) {
              Marshal.ThrowExceptionForHR(WinError.STRSAFE_E_INSUFFICIENT_BUFFER);
            } else {
              pszName.Clear();
              pszName.Append(command.Name);
            }
            break;
          case GCS.GCS_HELPTEXTW:
            if (command.HelpText.Length > cchMax - 1) {
              Marshal.ThrowExceptionForHR(WinError.STRSAFE_E_INSUFFICIENT_BUFFER);
            } else {
              pszName.Clear();
              pszName.Append(command.HelpText);
            }
            break;
        }
      }
    }

    /// <summary>
    /// Step 4 - Final step, trigger when user actuall click on a item
    /// </summary>
    /// <param name="pici">Local command ID</param>
    public void InvokeCommand(IntPtr pici) {
      uint idCmd = (uint)NativeHelpers.GetCommandOffsetId(pici);
      var command = this.Menu.FirstOrDefault(i => i.CommandID == idCmd);

      if (command != null) {
        command.CommandMethod(this.Files, this.Folders, this.IsClickOnEmptyArea);
      }
    }
    #endregion
  }
}
