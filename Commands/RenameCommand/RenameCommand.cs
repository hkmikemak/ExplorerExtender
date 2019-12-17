using ExplorerExtender.Helpers;
using ExplorerExtender.Models;
using Microsoft.VisualBasic.FileIO;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Threading;

namespace ExplorerExtender.Commands.RenameCommand {

  internal class RenameCommand : ICommand {

    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public IEnumerable<BaseMenuItem> BuildMenu(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      yield return new SubmenuMenuItem {
        Enabled = true,
        Name = "Rename",
        Items = {
            new CommandMenuItem {
              CommandMethod = RenameCommand.InvokeCommand_HtmlDecodeCommand,
              Enabled = (files.Any() || folders.Any()) && !isClickOnEmptyArea,
              HelpText = "HTML Decode",
              Name = "HTML Decode"
            },
            new CommandMenuItem {
              CommandMethod = RenameCommand.InvokeCommand_UrlDecodeCommand,
              Enabled = (files.Any() || folders.Any()) && !isClickOnEmptyArea,
              HelpText = "Url Decode",
              Name = "Url Decode"
            },
            new SeparatorMenuItem { Enabled = true },
            new CommandMenuItem {
              CommandMethod = RenameCommand.InvokeCommand_RenameCommand,
              Enabled = (files.Any() || folders.Any()) && !isClickOnEmptyArea,
              HelpText = "Rename with Pattern",
              Name = "Rename with Pattern"
            },
            new CommandMenuItem {
              CommandMethod = RenameCommand.InvokeCommand_RenameEditorCommand,
              Enabled = true,
              HelpText = "Edit Rename Patterns",
              Name = "Edit Rename Patterns"
            }
          }
      };
    }

    public static void InvokeCommand_RenameEditorCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {

      Logger.Trace("Start RenaneEditor");

      Thread thread = new Thread(() => {
        RenamePatternEditor newWindow = new RenamePatternEditor();
        newWindow.Show();
        newWindow.Closed += (s, e) => newWindow.Dispatcher.InvokeShutdown();
        Dispatcher.Run();
      });
      thread.SetApartmentState(ApartmentState.STA);
      thread.Start();
    }

    public static void InvokeCommand_RenameCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {

      RenameCommand.Logger.Trace("RenameCommand started");

      List<RenamePaternModel> patterns = RenamePatternStorage.Read();

      RenameCommand.Logger.Trace("Rename Patterns: {0}", JsonHelper.SerializeObject(patterns));

      foreach (FileInfo i in files.Select(i => new FileInfo(i))) {
        try {
          string newName = patterns.Aggregate(Path.GetFileNameWithoutExtension(i.Name), (agg, p) => p.IsRegex ? Regex.Replace(agg, p.Search, p.Replace) : agg.Replace(p.Search, p.Replace)) + Path.GetExtension(i.Name);
          RenameCommand.Logger.Trace("Try to rename file from \"{0}\" to \"{1}\"", i.Name, newName);
          FileSystem.MoveFile(i.FullName, Path.Combine(i.DirectoryName, newName), UIOption.AllDialogs);
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename file: {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }

      foreach (DirectoryInfo i in folders.Select(i => new DirectoryInfo(i))) {
        try {
          string newName = patterns.Aggregate(i.Name, (agg, p) => p.IsRegex ? Regex.Replace(agg, p.Search, p.Replace) : agg.Replace(p.Search, p.Replace));
          RenameCommand.Logger.Trace("Try to rename folder from \"{0}\" to \"{1}\"", i.Name, newName);
          FileSystem.MoveDirectory(i.FullName, Path.Combine(Path.GetDirectoryName(i.FullName), newName), UIOption.AllDialogs);
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename folder: {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }
    }

    public static void InvokeCommand_HtmlDecodeCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      Logger.Trace("HTML Decode Command Started");

      foreach (FileInfo i in files.Select(i => new FileInfo(i))) {
        try {
          string newName = Path.Combine(i.DirectoryName, HttpUtility.HtmlDecode(i.Name));
          if (!File.Exists(newName)) {
            FileSystem.MoveFile(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename file - {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }

      foreach (DirectoryInfo i in folders.Select(i => new DirectoryInfo(i))) {
        try {
          string newName = Path.Combine(Path.GetDirectoryName(i.FullName), HttpUtility.HtmlDecode(i.Name));
          if (!Directory.Exists(newName)) {
            FileSystem.MoveDirectory(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename folder - {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }
    }

    public static void InvokeCommand_UrlDecodeCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      Logger.Trace("URL Decode Command Started");

      foreach (FileInfo i in files.Select(i => new FileInfo(i))) {
        try {
          string newName = Path.Combine(i.DirectoryName, HttpUtility.UrlDecode(i.Name));
          if (!File.Exists(newName)) {
            FileSystem.MoveFile(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename file - {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }

      foreach (DirectoryInfo i in folders.Select(i => new DirectoryInfo(i))) {
        try {
          string newName = Path.Combine(Path.GetDirectoryName(i.FullName), HttpUtility.UrlDecode(i.Name));
          if (!Directory.Exists(newName)) {
            FileSystem.MoveDirectory(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch (Exception ex) {
          RenameCommand.Logger.Error("Failed to rename folder - {0}\r\n{1}", i.FullName, ex.ToDetailedString());
        }
      }
    }
  }
}
