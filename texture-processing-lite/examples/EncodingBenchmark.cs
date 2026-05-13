using System.Diagnostics;
using System.Threading.Tasks;
using Draft.TextureProcessing;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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
        byte[] bytes = null;

        for (int i = 0; i < iterations; i++)
        {
            var task = sourceTexture.EncodePNGAsync();
            bytes = await task;
            Debug.Log($"  Iteration {i + 1}/{iterations} completed on background thread, main thread was not blocked.");
        }
        sw.Stop();

        Debug.Log($"[Fast] EncodePNGAsync main-thread stall: {sw.Elapsed.TotalMilliseconds / iterations:F2} ms avg (encoding runs on background thread), {bytes.Length / 1024} KB");

        System.IO.File.WriteAllBytes(System.Environment.CurrentDirectory + "/unity_result.png", bytes);

       

    }

   
}