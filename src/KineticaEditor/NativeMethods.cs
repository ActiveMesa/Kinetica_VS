using System;
using System.Runtime.InteropServices;

namespace KineticaEditor
{
  public class NativeMethods
  {
    [DllImport("KineticaEngine.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
    public static extern void RenderFrame(string sourceCode, IntPtr bitmap, int bw, int bh, int stride, int lineCount,
      int maxColCount);
  }
}