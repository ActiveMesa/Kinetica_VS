namespace KineticaEditor
{
  /// <summary>
  /// A single clip that presents some source code.
  /// </summary>
  public class Clip
  {
    public long StartTime { get;set; }
    /// <summary>
    /// Computed from the next clip's end time or, if there is no end clip, oops!
    /// </summary>
    public string SourceCode;
    public int StartLine;
    public int EndLine;
  }
}