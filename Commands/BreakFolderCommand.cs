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
      folders.ForEach(i => {
        Logger.Info("Start processing - {0}", i);
        try {
          DirectoryInfo currentFolder = new DirectoryInfo(i);
          DirectoryInfo parentFolder = currentFolder.Parent;

          foreach (DirectoryInfo subFolder in currentFolder.EnumerateDirectories()) {
            try {
              string newFolderPath = Path.Combine(parentFolder.FullName, subFolder.Name);
              Logger.Info("Trying to move folder - {0} - {1}", subFolder.FullName, newFolderPath);
              FileSystem.MoveDirectory(subFolder.FullName, newFolderPath);
            } catch {
            }
          }

          foreach (FileInfo subFile in currentFolder.EnumerateFiles()) {
            try {
              string newFilePath = Path.Combine(parentFolder.FullName, subFile.Name);
              Logger.Info("Trying to move file - {0} - {1}", subFile.FullName, newFilePath);
              FileSystem.MoveFile(subFile.FullName, newFilePath);
            } catch {
            }
          }

          if (currentFolder.EnumerateDirectories().Count() == 0 && currentFolder.EnumerateFiles().Count() == 0) {
            currentFolder.Delete();
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
