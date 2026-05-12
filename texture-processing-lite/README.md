# Texture Processing Lite 1.0.0

**GPU-accelerated texture operations for Unity — flip, rotate, resize, crop, blend, and encode with a single line of code.**

Texture Processing Lite brings a full suite of GPU compute-shader-based texture operations to your Unity project. No native plugin required for processing — everything runs on the GPU via Unity's compute shader pipeline, making it fast and cross-platform from day one.

---

## Key Features

### Core Operations
- **Flip** — horizontal and vertical
- **Rotate** — any angle, with `Clip` or `Expand` canvas modes
- **Resize** — `Stretch`, `Fit`, and `Fill` modes with `Bilinear` or other filter modes
- **Scale** — uniform scale by float factor

### Crop & Mask
- **Rect crop** — crop to any `Rect`
- **Circle crop** — circular mask with optional custom radius
- **Rounded corners** — uniform or per-corner radii (`Vector4`)
- **Polygon crop** — arbitrary convex/concave polygon in normalised coordinates
- **Mask apply** — use any texture as an alpha mask (with invert option)
- **Aspect-ratio crop** — crop to target aspect ratio with 9 anchor positions

### Blend & Composite
- **13 blend modes**: Alpha, Additive, Multiply, Screen, Overlay, Darken, Lighten, Color Dodge, Color Burn, Hard Light, Soft Light, Difference, Exclusion
- **Overlay composite** — place a texture over a base with position, scale, and opacity control

### Image Adjustments
- Brightness, Contrast, Saturation, Grayscale

### Image Effects
- Blur (Gaussian separable, Box), Sharpen
- Edge detection: Sobel, Canny (full multi-pass pipeline)
- Morphology: Dilate, Erode
- Threshold, Adaptive Threshold
- Vignette, Color Matrix
- Hue Shift, Hue Remap, Selective HSV
- Chroma Key, In-Range mask
- Median Blur, Bilateral Filter
- CLAHE (Contrast Limited Adaptive Histogram Equalization)
- Channel Separation, Texture Subtract, Texture Add

### Geometric Transforms
- **Perspective Warp** — 4-point homography, normalised coordinate space
- **Affine Warp** — 3-point affine transform

### Fast Encoding (Native Plugin — optional)

- **FastEncodePNG** / **FastEncodeJPEG** — 2–5× faster than Unity's built-in `EncodeToPNG`, runs on CPU using multi-threaded Rust (Rayon thread pool)
- Works on `Texture2D` or raw `NativeArray<byte>` RGBA data
- Fully async variants available (`Task<byte[]>`)
- Save directly to file: `FastSavePNG`, `FastSaveJPEG`

### Pipeline System (TextureFlow)
- `TextureFlow` MonoBehaviour — drag-and-drop visual pipeline in the Inspector
- Stack multiple modules: Flip → Resize → Crop → Blend → …
- Auto-execute on selection change (Editor), or call `Execute()` from code
- Extensible: subclass `TextureProcessModule` to add custom steps

---

## Three Ways to Use It

### 1. Static API
```csharp
using Draft.TextureProcessing;

// One-liners
Texture2D flipped = TextureProcessor.FlipX(source);
Texture2D resized = TextureProcessor.Resize(source, 512, 512, TextureProcessor.ResizeMode.Fit);
Texture2D cropped = TextureProcessor.CropCircle(source);
Texture2D rounded = TextureProcessor.CropRoundedCorners(source, 0.1f);
Texture2D blended = TextureProcessor.Blend(src1, src2, TextureProcessor.BlendMode.Multiply, 0.8f);
```

### 2. Extension Methods
```csharp
using Draft.TextureProcessing;

Texture2D result = myTexture
    .FastResize(256, 256)
    .FastCropCircle()
    .FastBlend(overlayTex, TextureProcessor.BlendMode.Screen, 0.5f);

// Save to disk
myTexture.FastSavePNG("/path/to/output.png");

// Async
byte[] pngBytes = await myTexture.FastEncodePNGAsync();
```

### 3. Fluent Chain Builder
```csharp
using Draft.TextureProcessing;

Texture2D result = TextureProcessor
    .BeginChain(source)
    .FlipX()
    .Rotate(45f)
    .ResizeFit(512, 512)
    .RoundedCorners(0.05f)
    .Blend(watermark, TextureProcessor.BlendMode.Alpha, 0.7f)
    .Execute();

// Or async
Texture2D result = await TextureProcessor
    .BeginChain(source)
    .Resize(1024, 1024)
    .CropCircle()
    .ExecuteAsync();
```

### 4. Inspector Pipeline (TextureFlow)
Add a `TextureFlow` component to any GameObject. Use the custom Inspector to:
- Assign a source texture
- Add and reorder processing modules (Flip, Rotate, Resize, Crop, Blend, Overlay, Mask, …)
- Preview the result live in the Editor
- Call `textureFlow.Execute()` to process at runtime

---

## Async Support

All major operations have async counterparts returning `Task<Texture2D>` (or `Task<byte[]>` for encoding). Operations run off the main thread where possible.

```csharp
Texture2D result = await TextureProcessor.ResizeAsync(source, 1024, 1024);
byte[] jpeg = await TextureProcessor.FastEncodeJPEGAsync(result, quality: 90);
```

---

## Platform Support

| Platform | GPU Operations | Fast Encoding |
|----------|----------------|---------------|
| Windows (DX11/DX12) | ✅ | ✅ |
| macOS (Metal) | ✅ | ✅ |
| iOS (Metal) | ✅ | ✅ |
| Android (Vulkan / OpenGL ES 3.1+) | ✅ | ✅ |
| Linux (Vulkan) | ✅ | — |
| WebGL | ❌ | ❌ |

> GPU operations require compute shader support. WebGL does not support compute shaders.

---

## Requirements

- **Unity**: 2021.3 LTS or newer
- **Render Pipeline**: Built-in, URP, or HDRP (compute shaders are render-pipeline agnostic)
- **GPU**: Compute shader support (DX11+, Metal, Vulkan, OpenGL ES 3.1+)

Fast Encoding uses an optional native Rust plugin. Pre-built binaries for Windows, macOS, iOS, and Android are included. The plugin is only required for `FastEncodePNG` / `FastEncodeJPEG`; all other operations work without it.

---

## Installation

1. Import the package via Unity Package Manager or by placing the `Draft/TextureProcessing/` folder in your `Assets/`.
2. Add `using Draft.TextureProcessing;` to your scripts.
3. No setup required — compute shaders are loaded lazily on first use.

> **Optional**: Call `TextureProcessor.Initialize()` at startup to warm the GPU shaders and avoid a one-time load stall on first operation.

---

## Color Space

All intermediate `RenderTexture`s use **Linear** color space. The final `Texture2D` output is correctly converted to sRGB via a dedicated blit pass, ensuring consistent results across Metal, DirectX, and Vulkan.

---

## Architecture Overview

```
TextureProcessor          ← static API
TextureExtensions         ← Texture2D extension methods
TextureOperationChain     ← fluent builder
TextureFlow (MonoBehaviour) ← Inspector pipeline
    └── TextureProcessModule subclasses (one per operation)

Processors/               ← implementation classes
    TextureFlipProcessor, TextureRotateProcessor,
    TextureResizeProcessor, TextureCropProcessor,
    TextureBlendProcessor, TextureAdjustmentProcessor,
    TextureEffectProcessor, TextureWarpProcessor, …

Resources/                ← compute shaders (GPU kernels)
    TextureOperations.compute   (flip, rotate)
    TextureCrop.compute         (rect, circle, rounded corners)
    TextureAdvancedCrop.compute (polygon, mask, aspect-ratio)
    TextureCombine.compute      (13 blend modes + overlay)
    TextureAdjustments.compute  (brightness, contrast, saturation, grayscale)
    TextureEffects.compute      (blur, sharpen, edge, morph, color, CLAHE, …)
    TextureWarp.compute         (perspective, affine, homography)
    TextureMath.compute         (add, subtract)
    TextureBitwise.compute      (bitwise operations)
```

---

## FAQ

**Does it work with URP / HDRP?**
Yes. Compute shaders are independent of the render pipeline.

**Is it thread-safe?**
GPU dispatch must happen on the main thread. Async methods use `Task.Run` for CPU work and marshal GPU calls to the main thread automatically.

**Can I use it in the Editor (Edit mode)?**
Yes. `TextureFlow` supports Editor execution. The package uses `SafeDestroy` internally so there are no Play/Edit mode cleanup issues.

**Can I add my own processing steps to TextureFlow?**
Yes — subclass `TextureProcessModule`, implement `Process(Texture2D input)`, and add a matching `CustomPropertyDrawer` in the `Editor/` folder.

**What is the coordinate convention?**
All normalised coordinates use **x-right, y-down**, origin at **top-left**, matching OpenCV / image convention (not Unity's bottom-up convention).

---

## Support

- Report issues or feature requests via the publisher contact page.
- Namespace: `Draft.TextureProcessing`
- Assembly: `TextureProcessing.Runtime`

---

*Texture Processing Lite — clean API, GPU speed, zero boilerplate.*
