using ExplorerExtender.Commands;
using ExplorerExtender.Helpers;
using ExplorerExtender.Interop;
using ExplorerExtender.Models;
using NLog;
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
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    // All commands is stored as class rather than static class, we need to keep a reference at all time
    internal List<ICommand> AllAvailableCommands = new List<ICommand>();
    internal List<string> Files = new List<string>();
    internal List<string> Folders = new List<string>();
    internal bool IsClickOnEmptyArea = false;
    internal List<CommandMenuItem> GeneratedCommands = new List<CommandMenuItem>();

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
      NLogHelper.LoadNLog();

      Main.Logger.Trace("Explorer Extender initialized");

      Main.Logger.Trace("Searching and instancing classes with ICommand interface");
      this.AllAvailableCommands = ICommandHelper.CreateAllCommandInstances();
      Main.Logger.Trace("Total classes found: {0}", this.AllAvailableCommands.Count);

      Main.Logger.Trace("Process input from Explorer");
      (this.Files, this.Folders, this.IsClickOnEmptyArea) = NativeHelpers.ProcessSelectedItems(pidlFolder, pDataObj);
      Main.Logger.Trace("Is menu opened by clicking on empty area: {0}", this.IsClickOnEmptyArea);
      Main.Logger.Trace("Total # of selected files: {0}", this.Files.Count);
      Main.Logger.Trace("Total # of selected folders: {0}", this.Folders.Count);
    }
    #endregion

    #region IContextMenu
    /// <summary>
    /// Step 2 - Build Menu
    /// </summary>
    /// <param name="hMenu">Pointer of the main context menu</param>
    /// <param name="iMenu">Position of the main context ment to append to</param>
    /// <param name="idCmdFirst">Start of command ID (wID)</param>
    /// <param name="idCmdLast"></param>
    /// <param name="uFlags"></param>
    /// <returns></returns>
    public int QueryContextMenu(IntPtr hMenu, uint iMenu, uint idCmdFirst, uint idCmdLast, uint uFlags) {
      Main.Logger.Trace("Query Context Menu");

      if (((uint)CMF.CMF_DEFAULTONLY & uFlags) != 0) {
        return WinError.MAKE_HRESULT(WinError.SEVERITY_SUCCESS, 0, 0);
      }

      Main.Logger.Trace("Generating menu from all available commands");

      SubmenuMenuItem mainMenu = new SubmenuMenuItem {
        Enabled = true,
        Items = this.AllAvailableCommands.SelectMany(i => i.BuildMenu(this.Files, this.Folders, this.IsClickOnEmptyArea)).ToList(),
        Name = "Explorer Extender"
      };

      this.GeneratedCommands = mainMenu.Items.GetAllCommands().ToList();

      uint commandCounter = 0;
      foreach (CommandMenuItem item in this.GeneratedCommands) {
        item.PositionId = idCmdFirst + commandCounter;
        item.CommandID = commandCounter++;
      }

      Main.Logger.Trace("Generated menu: {0}", JsonHelper.SerializeObject(mainMenu));

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
      Logger.Trace("GetCommandString started");

      uint commandId = idCmd.ToUInt32();

      GCS requiredInfo = (GCS)uFlags;

      Logger.Trace("Command ID: {0}", commandId);
      Logger.Trace("Required info: {0}", Enum.GetName(typeof(GCS), requiredInfo));

      CommandMenuItem command = this.GeneratedCommands.FirstOrDefault(i => i.CommandID == commandId);

      if (command != null) {
        switch ((GCS)uFlags) {
          case GCS.GCS_VERBW:
            if (command.Name.Length > cchMax - 1) {
              Marshal.ThrowExceptionForHR(WinError.STRSAFE_E_INSUFFICIENT_BUFFER);
            } else {
              pszName.Clear();
              pszName.Append(command.Name);
              Logger.Trace("Return result: {0}", command.Name);
            }
            break;
          case GCS.GCS_HELPTEXTW:
            if (command.HelpText.Length > cchMax - 1) {
              Marshal.ThrowExceptionForHR(WinError.STRSAFE_E_INSUFFICIENT_BUFFER);
            } else {
              pszName.Clear();
              pszName.Append(command.HelpText);
              Logger.Trace("Return result: {0}", command.HelpText);
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
      Main.Logger.Trace("InvokeCommand started");
      uint idCmd = (uint)NativeHelpers.GetCommandOffsetId(pici);

      Main.Logger.Trace("Command Id: {0}", idCmd);

      CommandMenuItem command = this.GeneratedCommands.FirstOrDefault(i => i.CommandID == idCmd);

      if (command != null) {
        command.CommandMethod(this.Files, this.Folders, this.IsClickOnEmptyArea);
      }
    }
    #endregion
  }
}
