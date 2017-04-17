#pragma once

#ifdef KINETICAENGINE_EXPORTS
#define MYAPI extern "C" __declspec(dllexport)
#else
#define MYAPI __declspec(dllimport)
#endif

MYAPI inline void RenderFrame(wchar_t* sourceCode, BYTE* bitmap, int bw, int bh, int stride, int lineCount,
  int maxColCount);