# Texture Processing Lite — API Reference

Namespace: `Draft.TextureProcessing`  
Assembly: `TextureProcessing.Runtime`  
Unity: 2021.3+

---

## TextureProcessor

The primary static entry point. All GPU operations are synchronous (main-thread GPU dispatch). Encoding is handled via extension methods (`EncodePNGAsync`, `EncodeJPEGAsync`) on `Texture2D` — see `TextureExtensions`. Chain execution supports async via `TextureOperationChain.ExecuteAsync()`.

### Lifecycle

| Method | Description |
|--------|-------------|
| `Initialize()` | Eagerly load all compute shaders. Safe to call multiple times. |
| `ReleaseShaders()` | Release compute shader resources. |
| `CloneTexture(source)` | Deep copy of a `Texture2D`. |
| `ToTexture2D(texture)` | Convert any `Texture` to `Texture2D`. |

---

### Flip

```csharp
Texture2D FlipX(Texture2D source)
Texture2D FlipY(Texture2D source)
```

---

### Rotate

```csharp
Texture2D Rotate(Texture2D source, float angleDegrees,
    RotationClipMode clipMode = RotationClipMode.Clip)
```

**RotationClipMode**

| Value | Behaviour |
|-------|-----------|
| `Clip` | Output canvas stays the same size; corners are clipped. |
| `Expand` | Output canvas expands to fit the rotated image fully. |

---

### Resize & Scale

```csharp
Texture2D Resize(Texture2D source, int width, int height,
    ResizeMode mode = ResizeMode.Stretch,
    FilterMode filterMode = FilterMode.Bilinear)

Texture2D Scale(Texture2D source, float scale,
    FilterMode filterMode = FilterMode.Bilinear)
```

**ResizeMode**

| Value | Behaviour |
|-------|-----------|
| `Stretch` | Stretches to exact `width × height`, ignoring aspect ratio. |
| `Fit` | Scales uniformly to fit inside `width × height`; may add letterbox. |
| `Fill` | Scales uniformly to fill `width × height`; may crop edges. |

---

### Crop

```csharp
// Rectangular crop — Rect in pixel coordinates
Texture2D CropRect(Texture2D source, Rect cropRect)

// Circular crop — optional radius in normalised [0,1] units
Texture2D CropCircle(Texture2D source, float? radius = null)

// Rounded corners — uniform radius
Texture2D CropRoundedCorners(Texture2D source, float cornerRadius)

// Rounded corners — per-corner (x=TL, y=TR, z=BR, w=BL)
Texture2D CropRoundedCorners(Texture2D source, Vector4 cornerRadii)

// Polygon crop — points in normalised [0,1] x-right y-down space
Texture2D CropPolygon(Texture2D source, Vector2[] points)

// Texture mask — alpha channel of mask drives transparency
Texture2D ApplyMask(Texture2D source, Texture2D mask, bool invert = false)

// Aspect-ratio crop
Texture2D CropAspectRatio(Texture2D source, float aspectRatio,
    CropAnchor anchor = CropAnchor.Center)
```

**CropAnchor**

`Center`, `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight`, `TopCenter`, `BottomCenter`, `LeftCenter`, `RightCenter`

---

### Blend & Composite

```csharp
Texture2D Blend(Texture2D source1, Texture2D source2,
    BlendMode blendMode, float opacity = 1.0f)

Texture2D Overlay(Texture2D baseTexture, Texture2D overlayTexture,
    Vector2? position = null,   // normalised [0,1], default center
    Vector2? scale = null,      // normalised [0,1], default fit
    float opacity = 1.0f)
```

**BlendMode**

`Alpha` · `Additive` · `Multiply` · `Screen` · `Overlay` · `Darken` · `Lighten` · `ColorDodge` · `ColorBurn` · `HardLight` · `SoftLight` · `Difference` · `Exclusion`

---

### Async Encoding

Encoding is exposed as extension methods on `Texture2D` (see `TextureExtensions`), not as static methods on `TextureProcessor`. Encoding runs on a background thread via `Task.Run` using Unity's `ImageConversion` API — the main thread is never blocked, unlike Unity's built-in `EncodeToPNG` / `EncodeToJPG`. No native plugin required.

```csharp
// Available as extension methods on Texture2D:
Task<byte[]> EncodePNGAsync(this Texture2D texture)
Task<byte[]> EncodeJPEGAsync(this Texture2D texture, int quality = 85)
Task<bool>   SavePNGAsync(this Texture2D texture, string filePath)
Task<bool>   SaveJPEGAsync(this Texture2D texture, string filePath, int quality = 85)
```

---

### Chain Builder

```csharp
TextureOperationChain BeginChain(Texture2D source)
```

Returns a `TextureOperationChain`. See below.

---

## TextureOperationChain

Fluent builder for sequencing multiple operations. Call `Execute()` or `ExecuteAsync()` to produce the final `Texture2D`.

```csharp
// Transform
TextureOperationChain FlipX()
TextureOperationChain FlipY()
TextureOperationChain Rotate(float angleDegrees, RotationClipMode clipMode = RotationClipMode.Clip)

// Resize
TextureOperationChain Resize(int width, int height,
    ResizeMode mode = ResizeMode.Stretch,
    FilterMode filterMode = FilterMode.Bilinear)
TextureOperationChain Scale(float scale, FilterMode filterMode = FilterMode.Bilinear)
TextureOperationChain ResizeFit(int maxWidth, int maxHeight,
    FilterMode filterMode = FilterMode.Bilinear)

// Crop
TextureOperationChain CropRect(Rect cropRect)
TextureOperationChain CropCircle(float? radius = null)
TextureOperationChain RoundedCorners(float cornerRadius)
TextureOperationChain RoundedCorners(Vector4 cornerRadii)
TextureOperationChain CropPolygon(Vector2[] points)
TextureOperationChain CropMask(Texture2D mask)
TextureOperationChain CropAspectRatio(float aspectRatio,
    CropAnchor anchor = CropAnchor.Center)

// Combine
TextureOperationChain Blend(Texture2D blendTexture, BlendMode blendMode, float opacity = 1.0f)
TextureOperationChain Overlay(Texture2D overlayTexture,
    Vector2? position = null, Vector2? scale = null, float opacity = 1.0f)

// Terminal
Texture2D Execute()
Task<Texture2D> ExecuteAsync()
```

**Example**

```csharp
Texture2D avatar = await TextureProcessor
    .BeginChain(rawPhoto)
    .Resize(256, 256, ResizeMode.Fill)
    .CropCircle()
    .RoundedCorners(0f)   // already circle, skip
    .Blend(frame, BlendMode.Alpha, 1f)
    .ExecuteAsync();
```

---

## TextureExtensions

Extension methods on `Texture2D`. Transform/crop/blend methods use a `Fast` prefix; encoding/saving/load methods do not.

```csharp
// Transform
Texture2D  FastFlipX(this Texture2D)
Texture2D  FastFlipY(this Texture2D)
Texture2D  FastRotate(this Texture2D, float angle)

// Resize
Texture2D  FastResize(this Texture2D, int width, int height, ResizeMode mode, FilterMode filterMode)
Texture2D  FastResizeFit(this Texture2D, int maxWidth, int maxHeight)
Texture2D  FastResizeFill(this Texture2D, int width, int height)
Texture2D  FastScale(this Texture2D, float scale)

// Crop
Texture2D  FastCropRect(this Texture2D, Rect cropRect)
Texture2D  FastCropCircle(this Texture2D, float? radius = null)
Texture2D  FastCropRounded(this Texture2D, float cornerRadius)
Texture2D  FastCropPolygon(this Texture2D, Vector2[] points)
Texture2D  FastCropMask(this Texture2D, Texture2D mask, bool invert = false)
Texture2D  FastCropAspectRatio(this Texture2D, float aspectRatio, CropAnchor anchor)

// Combine
Texture2D  FastBlend(this Texture2D, Texture2D other, BlendMode blendMode, float opacity)
Texture2D  FastOverlay(this Texture2D, Texture2D overlayTexture, Vector2? position, Vector2? scale, float opacity)

// Encoding & saving (background thread via Task.Run, no native plugin required)
Task<byte[]>  EncodePNGAsync(this Texture2D)
Task<byte[]>  EncodeJPEGAsync(this Texture2D, int quality = 85)
Task<bool>    SavePNGAsync(this Texture2D, string filePath)
Task<bool>    SaveJPEGAsync(this Texture2D, string filePath, int quality = 85)

// Load
Task<Texture2D> LoadAsTextureAsync(this string pathOrUrl)

// Convert
Texture2D  ToTexture2D(this Texture texture)
```

---

## TextureFlow (MonoBehaviour)

Visual pipeline component. Attach to any `GameObject`.

### Inspector Properties

| Property | Type | Description |
|----------|------|-------------|
| `SourceTexture` | `Texture2D` | Input texture. |
| `OutputTexture` | `Texture2D` (read-only) | Result after `Execute()`. |
| `AutoExecute` | `bool` | Re-run pipeline on Inspector selection change (Editor only). |

### Runtime API

```csharp
// Execute pipeline on SourceTexture
Texture2D Execute()

// Execute on an explicit input (does not modify SourceTexture)
Texture2D Execute(Texture2D input)

// Module management
void AddModule(TextureProcessModule module)
void InsertModule(int index, TextureProcessModule module)
void RemoveModule(TextureProcessModule module)
void RemoveModuleAt(int index)
void MoveModule(int fromIndex, int toIndex)
void ClearModules()
TextureProcessModule GetModule(int index)
IReadOnlyList<TextureProcessModule> GetModules()
int ModuleCount { get; }
```

---

## Built-in TextureFlow Modules

| Module | Key Parameters |
|--------|----------------|
| `FlipModule` | Axis (X / Y) |
| `RotateModule` | Angle, ClipMode (Clip / Expand) |
| `ResizeModule` | Width, Height, ResizeMode, FilterMode |
| `CropModule` | CropType, Rect / Radius / Points / AspectRatio, Anchor |
| `BlendModule` | BlendTexture, BlendMode, Opacity |
| `OverlayModule` | OverlayTexture, Position, Scale, Opacity |
| `MaskModule` | MaskTexture, Invert |

### Creating a Custom Module

```csharp
using Draft.TextureProcessing;
using UnityEngine;

[System.Serializable]
public class InvertModule : TextureProcessModule
{
    public override string ModuleName => "Invert Colors";

    public override Texture2D Process(Texture2D input)
    {
        // implement using TextureProcessor or custom compute shader
        return input; // replace with actual implementation
    }
}
```

Add a matching `CustomPropertyDrawer` in the `Editor/Drawers/` folder for Inspector controls.

---

## Enumerations

```csharp
namespace Draft.TextureProcessing
{
    // Blend modes (TextureProcessor.BlendMode)
    enum BlendMode
    {
        Alpha, Additive, Multiply, Screen, Overlay,
        Darken, Lighten, ColorDodge, ColorBurn,
        HardLight, SoftLight, Difference, Exclusion
    }

    // Resize behaviour (TextureProcessor.ResizeMode)
    enum ResizeMode { Stretch, Fit, Fill }

    // Crop anchor for aspect-ratio crop (TextureProcessor.CropAnchor)
    enum CropAnchor
    {
        Center,
        TopLeft, TopRight, BottomLeft, BottomRight,
        TopCenter, BottomCenter, LeftCenter, RightCenter
    }

    // Rotation canvas mode (TextureUtilityCore.RotationClipMode)
    enum RotationClipMode { Clip, Expand }
}
```

---

## Coordinate System

All normalised coordinate parameters use:
- **x** → right
- **y** → down (image convention, not Unity world space)
- **origin** → top-left
- **range** → `[0, 1]`

This matches OpenCV convention. Conversion example:
```csharp
// normalised [0,1] → pixel
int px = Mathf.RoundToInt(normX * (width  - 1));
int py = Mathf.RoundToInt(normY * (height - 1));  // y=0 is top
```

Polygon points, overlay positions, and warp source/destination points all follow this convention.

---

## Performance Notes

- **Lazy initialisation** — compute shaders are loaded on first use. Call `TextureProcessor.Initialize()` at startup to avoid stutter.
- **Async encoding** — use `EncodePNGAsync` / `EncodeJPEGAsync` (extension methods on `Texture2D`) to encode off the main thread. For chained operations use `TextureOperationChain.ExecuteAsync()`.
- **Intermediate textures** — each chained operation produces a new `Texture2D`. For very long chains, consider using the `TextureOperationChain` which minimises unnecessary allocations.
- **Thread groups** — all 2D dispatch kernels use 8×8 thread groups (`CeilToInt(dimension / 8)`), optimal for most modern GPUs.

---


*API Reference — Texture Processing Lite*
