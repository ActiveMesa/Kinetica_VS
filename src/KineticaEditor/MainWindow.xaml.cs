using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using AviFile;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace KineticaEditor
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public const int FrameWidth = 1280;
    public const int FrameHeight = 720;

    public ObservableCollection<Clip> Clips { get; set; }

    public MainWindow()
    {
      Clips = new ObservableCollection<Clip>();
      InitializeComponent();
    }

    private void ExportToAvi(object sender, RoutedEventArgs e)
    {
      var clips = Clips.OrderBy(c => c.StartTime).ToList();

      if (clips.Count < 2) return;
      const float fps = 15;

      var sfd = new SaveFileDialog();
      if (sfd.ShowDialog() ?? false)
      {
        var mgr = new AviManager(sfd.FileName, false);
        Bitmap clipToRender = RenderClip(clips[0]);
        clipToRender.RotateFlip(RotateFlipType.RotateNoneFlipY);
        var stream = mgr.AddVideoStream(true, fps, RenderClip(clips[0]));

        // estimate overall duration in msec
        long totalMsec = clips[clips.Count - 1].StartTime + 5000;
        const int msecPerFrame = (int) (1.0f/fps*1000.0f);
        int clipIndex = 0;
        for (long msec = clips[0].StartTime; msec < totalMsec; msec += msecPerFrame)
        {
          // check if we need to change the clip
          if (clipIndex != clips.Count - 1 && clips[clipIndex + 1].StartTime <= msec)
          {
            clipIndex++;
            clipToRender.Dispose();
            clipToRender = RenderClip(clips[clipIndex]);

            clipToRender.Save(Path.Combine(@"C:\users\dmitri\desktop\temp\", clipIndex.ToString()+".png"), ImageFormat.Png);

            clipToRender.RotateFlip(RotateFlipType.RotateNoneFlipY);
          }
          stream.AddFrame(clipToRender);
        }

        clipToRender.Dispose();
        mgr.Close(); // we're done
      }
    }

    private Bitmap RenderClip(Clip clip)
    {
      var bmp = new Bitmap(FrameWidth, FrameHeight);
      var gfx = Graphics.FromImage(bmp);
      gfx.FillRectangle(Brushes.White, 0, 0, FrameWidth, FrameHeight);
      var data = bmp.LockBits(new Rectangle(0, 0, FrameWidth, FrameHeight), 
        ImageLockMode.ReadWrite, bmp.PixelFormat);

      var lines = clip.SourceCode.Split('\n');

      NativeMethods.RenderFrame(clip.SourceCode, data.Scan0, data.Width, data.Height, data.Stride,
        lines.Length, lines.Max(l => l.Length));
      bmp.UnlockBits(data);
      return bmp;
    }

    private void ImportSequence(object sender, RoutedEventArgs e)
    {
      var fbd = new FolderBrowserDialog();
      
      if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
      {
        var files = Directory.GetFiles(fbd.SelectedPath);
        foreach (var file in files)
        {
          // turn it into a clip
          var clip = new Clip();
          var lines = File.ReadAllLines(file);
          if (lines.Length > 0)
          {
            clip.StartLine = 1;
            clip.EndLine = lines.Length;
          }
          clip.SourceCode = string.Join(Environment.NewLine, lines);
          long l;
          if (long.TryParse(Path.GetFileNameWithoutExtension(file), out l))
          {
            clip.StartTime = l;
            Clips.Add(clip);
          }
        }
      }
    }
  }
}
