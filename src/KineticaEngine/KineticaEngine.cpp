#include "stdafx.h"
#include "KineticaEngine.h"

struct Pixel { // bgra
  BYTE Blue;
  BYTE Green;
  BYTE Red;
  BYTE Alpha;

  bool operator==(const Pixel& other)
  {
    return Blue == other.Blue && Red == other.Red && Green == other.Green && Alpha == other.Alpha;
  }

  bool operator!=(const Pixel& other)
  {
    return !(*this == other);
  }
};

void RenderFrame(wchar_t* sourceCode, BYTE* bitmap, int bw, int bh, int stride, int lineCount, int maxColCount)
{
  auto hr = S_OK;

  CComPtr<IWICImagingFactory> wicFactory;
  CComPtr<ID2D1Factory> d2dFactory;
  CComPtr<IDWriteFactory> dwriteFactory;
  CComPtr<IWICBitmap> wicBitmap;
  CComPtr<ID2D1RenderTarget> rt;
  CComPtr<IDWriteTextFormat> textFormat;
  CComPtr<ID2D1SolidColorBrush> blackBrush;
  CComPtr<IDWriteTextLayout> textLayout;
  CComPtr<IDWriteTypography> typography;

  bool italic = false;
  wstring fontFamily{ L"Consolas" };
  int fontSizeH = bh / lineCount / 1.2;
  int fontSizeW = bw / maxColCount * 1.5;
  int fontSize = min(fontSizeW, fontSizeH);

  hr = CoCreateInstance(CLSID_WICImagingFactory, nullptr, CLSCTX_INPROC_SERVER, 
    IID_IWICImagingFactory, reinterpret_cast<void**>(&wicFactory));

  if (SUCCEEDED(hr))
  {
    hr = D2D1CreateFactory(D2D1_FACTORY_TYPE_SINGLE_THREADED, &d2dFactory);
  }

  if (SUCCEEDED(hr))
  {
    hr = DWriteCreateFactory(
      DWRITE_FACTORY_TYPE_SHARED,
      __uuidof(dwriteFactory),
      reinterpret_cast<IUnknown **>(&dwriteFactory));
  }

  if (SUCCEEDED(hr))
  {
    hr = wicFactory->CreateBitmap(bw, bh,
      GUID_WICPixelFormat32bppBGR,
      WICBitmapCacheOnLoad, &wicBitmap);
  }

  if (SUCCEEDED(hr))
  {
    hr = d2dFactory->CreateWicBitmapRenderTarget(
      wicBitmap, D2D1::RenderTargetProperties(), &rt);
  }

  if (SUCCEEDED(hr))
  {
    hr = dwriteFactory->CreateTextFormat(
      fontFamily.c_str(),
      nullptr,
      DWRITE_FONT_WEIGHT_NORMAL,
      italic ? DWRITE_FONT_STYLE_ITALIC : DWRITE_FONT_STYLE_NORMAL,
      DWRITE_FONT_STRETCH_NORMAL,
      fontSize,
      L"", //locale
      &textFormat);
  }

  if (SUCCEEDED(hr))
  {
    hr = dwriteFactory->CreateTextLayout(sourceCode, 
      wcslen(sourceCode), textFormat, bw, bh, &textLayout);
  }

  if (SUCCEEDED(hr))
  {
    hr = rt->CreateSolidColorBrush(D2D1::ColorF(D2D1::ColorF::Black),
      &blackBrush);
  }

  if (SUCCEEDED(hr))
  {
    rt->BeginDraw();
    rt->Clear(D2D1::ColorF(D2D1::ColorF::White));
    D2D1_SIZE_F rtSize = rt->GetSize();
    rt->DrawTextLayout(D2D1::Point2F(), textLayout, blackBrush);
    hr = rt->EndDraw();
  }

  if (SUCCEEDED(hr))
  {
    WICRect r;
    r.X = r.Y = 0;
    r.Width = bw;
    r.Height = bh;
    hr = wicBitmap->CopyPixels(&r, stride, sizeof(Pixel)*bw*bh, bitmap);
  }
}