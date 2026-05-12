using System.Diagnostics;
using System.Threading.Tasks;
using Draft.TextureProcessing;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class EncodingBenchmark : MonoBehaviour
{
    public Texture2D sourceTexture;
    public int iterations = 5;


    async void Start()
    {
        Debug.Log($"=== Benchmark {sourceTexture.width}x{sourceTexture.height} x{iterations} ===");

        //delay a bit to ensure any initialization overhead is out of the way
        await Task.Delay(100);

        // Unity sync
        var sw = Stopwatch.StartNew();

        // Async: measure only how long the main thread is stalled before background work takes over
        byte[] fastAsyncBytes = null;

        for (int i = 0; i < iterations; i++)
        {
            var task = TextureProcessor.FastEncodePNGAsync(sourceTexture, quality: 100);
            fastAsyncBytes = await task;
            Debug.Log($"  Iteration {i + 1}/{iterations} completed on background thread, main thread was not blocked.");
        }
        sw.Stop();

        Debug.Log($"[Fast] FastEncodePNGAsync main-thread stall: {sw.Elapsed.TotalMilliseconds / iterations:F2} ms avg (encoding runs on background thread), {fastAsyncBytes.Length / 1024} KB");

        sw.Restart();
        byte[] unityBytes = null;
        for (int i = 0; i < iterations; i++)
            unityBytes = sourceTexture.EncodeToPNG();
        sw.Stop();
        Debug.Log($"[Unity] EncodeToPNG: {sw.Elapsed.TotalMilliseconds / iterations:F1} ms avg, {unityBytes.Length / 1024} KB");


    }
}