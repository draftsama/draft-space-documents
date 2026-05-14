# Screen Recorder — API Reference

**Namespace:** `ScreenRecorder`

---

## Components

### `GameViewRecorder`

Records the full game view (entire screen output). Add to any GameObject.

```
Component Menu: Screen Recorder / Game View Recorder
```

Inherits all properties and methods from [`ScreenRecorderBase`](#screenrecorderbase).

---

### `CameraRecorder`

Records output from a specific Camera. Uses `OnRenderImage` for correct orientation on all platforms.

```
Component Menu: Screen Recorder / Camera Recorder
```

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `targetCamera` | `Camera` | `Camera.main` | Camera to record. Falls back to `Camera.main` if null. |

Inherits all properties and methods from [`ScreenRecorderBase`](#screenrecorderbase).

---

## ScreenRecorderBase

Abstract `MonoBehaviour` base class. Do not add this directly — use `GameViewRecorder` or `CameraRecorder`.

### Inspector Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `outputWidth` | `int` | `0` | Output width in pixels. `0` = use source resolution. |
| `outputHeight` | `int` | `0` | Output height in pixels. `0` = use source resolution. |
| `fps` | `int` | `30` | Target frames per second. Range: 15–120. |
| `quality` | `int` | `5` | Encoder quality preset. Range: 0 (lowest) – 10 (highest). |
| `bitrate` | `int` | `0` | Video bitrate in kbps. `0` = auto based on resolution. |
| `useHardwareEncoder` | `bool` | `true` | Prefer hardware encoder (H.264) when available. |
| `outputFolder` | `string` | `""` | Output directory. Empty = platform default (see below). |
| `filenamePrefix` | `string` | `"recording"` | Filename prefix. Output: `{prefix}_{yyyyMMdd_HHmmss}.mp4`. |
| `enableAudio` | `bool` | `false` | Enable audio recording. |
| `audioSource` | `AudioSourceType` | `Game` | Audio source selection. |
| `audioSampleRate` | `int` | `48000` | Audio sample rate. Recommended: `44100` or `48000`. |
| `audioChannels` | `int` | `2` | Audio channel count. `1` = mono, `2` = stereo. |
| `audioBitrate` | `int` | `128` | Audio bitrate in kbps. |
| `enableOverlay` | `bool` | `false` | Enable overlay/watermark compositing. |
| `overlayTexture` | `Texture2D` | `null` | Overlay texture. Alpha channel used for blending. |
| `overlayRect` | `Rect` | `(0,0,0,0)` | Overlay position and size in pixels. `(0,0,0,0)` = fullscreen. |
| `overlayOpacity` | `float` | `1.0` | Overlay opacity. Range: 0–1. |
| `debugMode` | `bool` | `false` | Log verbose output to console. |

### Properties (Read-only)

| Property | Type | Description |
|----------|------|-------------|
| `State` | `RecordingState` | Current recording state: `Idle`, `Recording`, or `Stopping`. |
| `IsRecording` | `bool` | `true` when `State == Recording`. |
| `FrameCount` | `int` | Number of frames captured in the current session. |
| `CurrentOutputPath` | `string` | Full path of the output `.mp4` file. Set after `StartRecording()`. |
| `ElapsedTime` | `float` | Elapsed recording time in seconds. |
| `ElapsedTimeFormatted` | `string` | Elapsed time as `"mm:ss"` string. |
| `RecordingFPS` | `float` | Measured capture rate (updated every 0.5 s). |

### Methods

#### `bool StartRecording()`

Initializes the native encoder and begins capturing frames.

Returns `true` on success, `false` if already recording or initialization failed.

```csharp
if (!recorder.StartRecording())
    Debug.LogError("Failed to start");
```

#### `void StopRecording()`

Signals the encoder to finish and flush remaining frames. Encoding completes asynchronously — `OnRecordingComplete` fires when the file is written.

```csharp
recorder.StopRecording();
```

### Events

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnRecordingComplete` | `Action<string>` | Fired on the main thread when the file is saved. Parameter: output file path. |
| `OnRecordingError` | `Action<string>` | Fired when an error occurs. Parameter: error description. |
| `OnFrameCaptured` | `Action<int>` | Fired each time a frame is pushed to the encoder. Parameter: current frame count. |

```csharp
recorder.OnRecordingComplete += path => Debug.Log($"Saved: {path}");
recorder.OnRecordingError   += err  => Debug.LogError(err);
```

### Default Output Paths

| Platform | Default folder |
|----------|---------------|
| Windows | `%USERPROFILE%\Videos\ScreenRecorder` |
| macOS | `~/Movies/ScreenRecorder` |
| iOS / Android | `Application.persistentDataPath/ScreenRecorder` |

---

## Enums

### `RecordingState`

| Value | Description |
|-------|-------------|
| `Idle` | No active recording. |
| `Recording` | Frames are being captured and encoded. |
| `Stopping` | Stop has been requested; encoder is flushing. |

---

### `AudioSourceType`

| Value | Description |
|-------|-------------|
| `None` | No audio track. |
| `Game` | Capture game audio via `AudioListener`. |
| `Microphone` | Capture from device microphone. |
| `Both` | Mix game audio and microphone. |

---

### `PixelFormat`

Used internally by the FFI layer. Not required for normal usage.

| Value | Description |
|-------|-------------|
| `RGBA32` | 8 bits per channel, R first. |
| `BGRA32` | 8 bits per channel, B first. |
| `RGB24` | 24-bit packed RGB. |
| `ARGB32` | 8 bits per channel, A first. |

---

## RecordingSettings (Struct)

Low-level settings struct passed to the native encoder. Use the convenience factories for normal workflows.

```csharp
// Video only
var settings = RecordingSettings.Default(1920, 1080, outputPath);

// With audio
var settings = RecordingSettings.WithAudio(
    1920, 1080, outputPath,
    source: AudioSourceType.Game,
    sampleRate: 48000,
    channels: 2
);
```

| Factory | Description |
|---------|-------------|
| `RecordingSettings.Default(width, height, path)` | 30 fps, quality 5, hardware encoder, no audio. |
| `RecordingSettings.WithAudio(width, height, path, source, sampleRate, channels)` | Same defaults with audio enabled. |

---

## ScreenRecorderPlugin (Static)

Low-level FFI bindings. Use `ScreenRecorderBase` subclasses for all normal recording workflows. Only access this class when building a custom recorder.

| Method | Returns | Description |
|--------|---------|-------------|
| `IsHardwareEncodingAvailable()` | `bool` | Check if hardware H.264 is available on this device. |
| `GetAvailableEncodersArray()` | `string[]` | List available encoder names. |
| `GetErrorString(IntPtr handle)` | `string` | Get last error from a recorder handle. |

---

## Quick Start

### Record the Game View

```csharp
using ScreenRecorder;
using UnityEngine;

public class RecordButton : MonoBehaviour
{
    [SerializeField] GameViewRecorder recorder;

    void Start()
    {
        recorder.OnRecordingComplete += path => Debug.Log($"Saved to: {path}");
        recorder.OnRecordingError   += err  => Debug.LogError(err);
    }

    public void ToggleRecording()
    {
        if (recorder.IsRecording)
            recorder.StopRecording();
        else
            recorder.StartRecording();
    }
}
```

### Record a Specific Camera with Audio

```csharp
recorder.targetCamera   = myCamera;
recorder.fps            = 60;
recorder.enableAudio    = true;
recorder.audioSource    = AudioSourceType.Game;
recorder.outputFolder   = Application.persistentDataPath;
recorder.filenamePrefix = "gameplay";
recorder.StartRecording();
```

### Check Hardware Encoding Support

```csharp
bool hwAvailable = ScreenRecorderPlugin.IsHardwareEncodingAvailable();
string[] encoders = ScreenRecorderPlugin.GetAvailableEncodersArray();
```

---

## Platform Notes

| Platform | Encoder | Output |
|----------|---------|--------|
| macOS | AVFoundation (VideoToolbox) | H.264 MP4 |
| iOS | AVFoundation (VideoToolbox) | H.264 MP4 |
| Android | MediaCodec | H.264 MP4 |
| Windows | Media Foundation | H.264 MP4 |

- **IL2CPP:** Fully supported. Static callback delegates with `[MonoPInvokeCallback]` are used throughout.
- **AsyncGPUReadback:** Frames are read from the GPU asynchronously — no main-thread stall.
- **Dimensions:** Width and height are automatically rounded to even numbers (H.264 requirement).
