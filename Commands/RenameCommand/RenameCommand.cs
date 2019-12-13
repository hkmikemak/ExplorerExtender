using ExplorerExtender.Models;
using Microsoft.VisualBasic.FileIO;
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
      List<RenamePaternModel> patterns = RenamePatternStorage.Read();

      foreach (FileInfo i in files.Select(i => new FileInfo(i))) {
        string newName = patterns.Aggregate(i.Name, (agg, p) => p.IsRegex ? Regex.Replace(agg, p.Search, p.Replace) : agg.Replace(p.Search, p.Replace));
        FileSystem.MoveFile(i.FullName, Path.Combine(i.DirectoryName, newName), UIOption.AllDialogs);
      }

      foreach (DirectoryInfo i in folders.Select(i => new DirectoryInfo(i))) {
        string newName = patterns.Aggregate(i.Name, (agg, p) => p.IsRegex ? Regex.Replace(agg, p.Search, p.Replace) : agg.Replace(p.Search, p.Replace));
        FileSystem.MoveDirectory(i.FullName, Path.Combine(Path.GetDirectoryName(i.FullName), newName), UIOption.AllDialogs);
      }
    }

    public static void InvokeCommand_HtmlDecodeCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      files.AsParallel().Select(i => new FileInfo(i)).ForAll(i => {
        try {
          string newFileNmae = Path.Combine(i.DirectoryName, HttpUtility.HtmlDecode(i.Name));
          if (!File.Exists(newFileNmae)) {
            FileSystem.MoveFile(i.FullName, newFileNmae, UIOption.AllDialogs);
          }
        } catch (Exception) {

        }
      });

      folders.AsParallel().Select(i => new DirectoryInfo(i)).ForAll(i => {
        try {
          string newName = Path.Combine(Path.GetDirectoryName(i.FullName), HttpUtility.HtmlDecode(i.Name));
          if (!Directory.Exists(newName)) {
            FileSystem.MoveDirectory(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch { }
      });

    }

    public static void InvokeCommand_UrlDecodeCommand(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      files.AsParallel().Select(i => new FileInfo(i)).ForAll(i => {
        try {
          string newFileNmae = Path.Combine(i.DirectoryName, HttpUtility.UrlDecode(i.Name));
          if (!File.Exists(newFileNmae)) {
            FileSystem.MoveFile(i.FullName, newFileNmae, UIOption.AllDialogs);
          }
        } catch (Exception) {

        }
      });

      folders.AsParallel().Select(i => new DirectoryInfo(i)).ForAll(i => {
        try {
          string newName = Path.Combine(Path.GetDirectoryName(i.FullName), HttpUtility.UrlDecode(i.Name));
          if (!Directory.Exists(newName)) {
            FileSystem.MoveDirectory(i.FullName, newName, UIOption.AllDialogs);
          }
        } catch { }
      });

    }
  }
}
