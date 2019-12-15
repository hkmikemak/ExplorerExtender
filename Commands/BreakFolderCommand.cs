using ExplorerExtender.Models;
using Microsoft.VisualBasic.FileIO;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExplorerExtender.Commands {
  internal class BreakFolderCommand : ICommand {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IEnumerable<BaseMenuItem> BuildMenu(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      yield return new CommandMenuItem {
        Enabled = (files.Count == 0 && folders.Any()),
        CommandMethod = BreakFolderCommand.InvokeCommand_BreakFolder,
        HelpText = "Move content with the directory up one level, then remove the directory",
        Name = "Break Folder"
      };
    }

    public static void InvokeCommand_BreakFolder(List<string> files, List<string> folders, bool isClickOnEmptyArea) {

      BreakFolderCommand.Logger.Trace("BreakFolder started");

      folders.ForEach(i => {
        Logger.Trace("Start processing folder - {0}", i);
        try {
          DirectoryInfo currentFolder = new DirectoryInfo(i);
          DirectoryInfo parentFolder = currentFolder.Parent;

          foreach (DirectoryInfo subFolder in currentFolder.EnumerateDirectories()) {
            try {
              string newFolderPath = Path.Combine(parentFolder.FullName, subFolder.Name);
              Logger.Trace("Trying to move folder from \"{0}\" to \"{1}\"", subFolder.FullName, newFolderPath);
              FileSystem.MoveDirectory(subFolder.FullName, newFolderPath);
            } catch {
              Logger.Error("Failed to move file - {0}", i);
            }
          }

          foreach (FileInfo subFile in currentFolder.EnumerateFiles()) {
            try {
              string newFilePath = Path.Combine(parentFolder.FullName, subFile.Name);
              Logger.Trace("Trying to move file from \"{0}\" to \"{1}\"", subFile.FullName, newFilePath);
              FileSystem.MoveFile(subFile.FullName, newFilePath);
            } catch {
              Logger.Error("Failed to move folder - {0}", i);
            }
          }

          if (currentFolder.EnumerateDirectories().Count() == 0 && currentFolder.EnumerateFiles().Count() == 0) {
            try {
              FileSystem.DeleteDirectory(currentFolder.FullName, UIOption.AllDialogs, RecycleOption.DeletePermanently);
            } catch {
              Logger.Error("Failed to delete folder - {0}", currentFolder.FullName);
            }
          }
        } catch (Exception ex) {
          Logger.Error(ex, "Failed to Break Folder - {0}", i);
          Logger.Error("Error Message - {0}", ex.Message);
          Logger.Error("Error StackTrace - {0}", ex.StackTrace);
        }
      });
    }


  }
}
