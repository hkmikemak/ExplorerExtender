using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace ExplorerExtender.Interop {
  internal static class NativeHelpers {


    internal static int GetCommandOffsetId(IntPtr pici) => NativeMethods.LowWord(((CMINVOKECOMMANDINFO)Marshal.PtrToStructure(pici, typeof(CMINVOKECOMMANDINFO))).verb.ToInt32());

    internal static (List<string>, List<string>, bool) ProcessSelectedItems(IntPtr pidlFolder, IntPtr pDataObj) {
      if (pDataObj == IntPtr.Zero && pidlFolder == IntPtr.Zero) {
        throw new ArgumentException();
      } else if (pDataObj == IntPtr.Zero) {
        //User Click on Empty Area of a Folder, pDataObj is empty while pidlFolder is the current path
        StringBuilder sb = new StringBuilder(260);
        if (!NativeMethods.SHGetPathFromIDListW(pidlFolder, sb)) {
          Marshal.ThrowExceptionForHR(WinError.E_FAIL);
          throw new ArgumentException();
        } else {
          return (new List<string>(), new List<string> { sb.ToString() }, true);
        }
      } else {
        //User actually select some item, pDataObj is the list while pidlFolder is empty

        FORMATETC fe = new FORMATETC {
          cfFormat = (short)CLIPFORMAT.CF_HDROP,
          ptd = IntPtr.Zero,
          dwAspect = DVASPECT.DVASPECT_CONTENT,
          lindex = -1,
          tymed = TYMED.TYMED_HGLOBAL
        };

        IDataObject dataObject = (IDataObject)Marshal.GetObjectForIUnknown(pDataObj);
        dataObject.GetData(ref fe, out STGMEDIUM stm);

        try {
          IntPtr hDrop = stm.unionmember;
          if (hDrop == IntPtr.Zero) {
            throw new ArgumentException();
          }

          uint nFiles = NativeMethods.DragQueryFile(hDrop, uint.MaxValue, null, 0);

          if (nFiles == 0) {
            Marshal.ThrowExceptionForHR(WinError.E_FAIL);
          }

          List<string> files = new List<string>();
          List<string> folders = new List<string>();

          for (int i = 0; i < nFiles; i++) {
            StringBuilder sb = new StringBuilder(260);
            if (NativeMethods.DragQueryFile(hDrop, (uint)i, sb, sb.Capacity) == 0) {
              Marshal.ThrowExceptionForHR(WinError.E_FAIL);
            } else {
              string item = sb.ToString();
              if (File.Exists(item)) {
                files.Add(item);
              } else if (Directory.Exists(item)) {
                folders.Add(item);
              }
            }
          }

          return (files, folders, false);
        } finally {
          NativeMethods.ReleaseStgMedium(ref stm);
        }
      }
    }



  }
}
