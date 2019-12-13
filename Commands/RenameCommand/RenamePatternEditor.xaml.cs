using NLog;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ExplorerExtender.Commands.RenameCommand {
  /// <summary>
  /// Interaction logic for RenamePatternEditor.xaml
  /// </summary>
  public partial class RenamePatternEditor : Window {


    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ObservableCollection<RenamePaternModel> Models { get; set; }


    public RenamePatternEditor() {
      this.Models = new ObservableCollection<RenamePaternModel>(RenamePatternStorage.Read());

      Logger.Debug("Before - " + this.Models.Count);

      this.InitializeComponent();

      this.DataGrid.ItemsSource = this.Models;
    }

    public void Button_Save_Click(object sender, EventArgs e) => RenamePatternStorage.Save(this.Models.ToList());

    private void Button_Add_Click(object sender, RoutedEventArgs e) {
      this.Models.Add(new RenamePaternModel());
      Logger.Debug(this.Models.Count);
    }

    private void Button_MoveUp_Click(object sender, RoutedEventArgs e) {
      if (((Button)sender).DataContext is RenamePaternModel selectedItem && this.Models.Contains(selectedItem)) {
        int index = this.Models.IndexOf(selectedItem);
        if (index != 0) {
          this.Models.Move(index, index - 1);
        }
      }
    }

    private void Button_MoveDown_Click(object sender, RoutedEventArgs e) {
      if (((Button)sender).DataContext is RenamePaternModel selectedItem && this.Models.Contains(selectedItem)) {
        int index = this.Models.IndexOf(selectedItem);
        if (index != this.Models.Count - 1) {
          this.Models.Move(index, index + 1);
        }
      }
    }

    private void Button_Delete_Row_Click(object sender, RoutedEventArgs e) {
      if (((Button)sender).DataContext is RenamePaternModel selectedItem) {
        this.Models.Remove(selectedItem);
      }
    }
  }
}
