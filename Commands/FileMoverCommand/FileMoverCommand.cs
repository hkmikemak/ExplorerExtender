using ExplorerExtender.Helpers;
using ExplorerExtender.Models;
using Microsoft.VisualBasic.FileIO;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExplorerExtender.Commands.FileMoverCommand {
  internal class FileMoverCommand : ICommand {


    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();


    public IEnumerable<BaseMenuItem> BuildMenu(List<string> files, List<string> folders, bool isClickOnEmptyArea) {

      List<FileMoverModel> currentList = FileMoverStorage.Read();

      yield return new SubmenuMenuItem {
        Enabled = true,
        Name = "File Mover",
        Items = {
          new CommandMenuItem {
            CommandMethod = InvokeCommand_Add,
            Enabled = (files.Any() || folders.Any()),
            HelpText = "Add selected items to the list",
            Name = "Add to List",
          },
          new CommandMenuItem {
            CommandMethod = InvokeCommand_Add,
            Enabled = (files.Any() || folders.Any()),
            HelpText = "Remove selected items from the list",
            Name = "Remove from List",
          },
          new SeparatorMenuItem {Enabled = true},
          new CommandMenuItem {
            CommandMethod = InvokeCommand_Clear,
            Enabled = currentList.Any(),
            HelpText = "Clear entire list",
            Name = "Clear List",
          },
          new SeparatorMenuItem {Enabled = true},
          new CommandMenuItem {
            CommandMethod = InvokeCommand_Move,
            Enabled = currentList.Any() && !files.Any() && folders.Count == 1,
            HelpText = "Move all items from list here",
            Name = "Move Here"
          }
        }
      };
    }

    public static void InvokeCommand_Add(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      List<FileMoverModel> currentList = FileMoverStorage.Read();
      currentList.AddRange(files.Select(i => new FileMoverModel { FullName = i, Type = FileMoverType.FILE }));
      currentList.AddRange(folders.Select(i => new FileMoverModel { FullName = i, Type = FileMoverType.FOLDER }));
      FileMoverStorage.Save(currentList.OrderBy(i => i.FullName).Distinct().ToList());
    }

    public static void InvokeCommand_Remove(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      List<FileMoverModel> currentList = FileMoverStorage.Read();
      currentList = currentList.Except(files.Select(i => new FileMoverModel { FullName = i, Type = FileMoverType.FILE })).ToList();
      currentList = currentList.Except(folders.Select(i => new FileMoverModel { FullName = i, Type = FileMoverType.FOLDER })).ToList();
      FileMoverStorage.Save(currentList.OrderBy(i => i.FullName).Distinct().ToList());
    }

    public static void InvokeCommand_Clear(List<string> files, List<string> folders, bool isClickOnEmptyArea) => FileMoverStorage.Save(new List<FileMoverModel> { });

    public static void InvokeCommand_Move(List<string> files, List<string> folders, bool isClickOnEmptyArea) {
      List<FileMoverModel> currentList = FileMoverStorage.Read();

      files.Select(i => new FileInfo(i))
        .Where(i => i.Exists)
        .AsParallel()
        .ForAll(i => {
          try {
            FileSystem.MoveFile(i.FullName, Path.Combine(folders.First(), i.Name), UIOption.AllDialogs);
          } catch (Exception ex) {
            Logger.Error("Failed to move file - {0}\r\n", i.FullName, ex.ToDetailedString());
          }
        });

      folders.Select(i => new DirectoryInfo(i))
        .Where(i => i.Exists)
        .AsParallel()
        .ForAll(i => {
          try {
            FileSystem.MoveDirectory(i.FullName, Path.Combine(folders.First(), i.Name), UIOption.AllDialogs);
          } catch (Exception ex) {
            Logger.Error("Failed to move file - {0}\r\n", i.FullName, ex.ToDetailedString());
          }
        });

      FileMoverStorage.Save(new List<FileMoverModel> { });
    }
  }
}
