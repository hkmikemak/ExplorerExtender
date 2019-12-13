namespace ExplorerExtender.Commands.FileMoverCommand {


  public enum FileMoverType {
    FILE,
    FOLDER
  }

  public class FileMoverModel {
    public string FullName { get; set; }
    public FileMoverType Type { get; set; }
  }
}
