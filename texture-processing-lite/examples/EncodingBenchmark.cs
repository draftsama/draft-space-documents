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

        var sw = Stopwatch.StartNew();
        byte[] bytes = null;

        for (int i = 0; i < iterations; i++)
        {
            bytes = sourceTexture.EncodeToPNG();
           Debug.Log($"Iteration {i + 1}/{iterations} completed, main thread was blocked until encoding finished.");
        }

        Debug.Log($"[Unity Built-in] EncodeToPNG : {sw.Elapsed.TotalMilliseconds / iterations:F2} ms avg, {bytes.Length / 1024} KB");
        Debug.Log("===================================");
        sw.Restart();

        for (int i = 0; i < iterations; i++)
        {
            var task = sourceTexture.EncodePNGAsync();
            bytes = await task;
            Debug.Log($"Iteration {i + 1}/{iterations} completed on background thread, main thread was not blocked.");
        }
        sw.Stop();

        Debug.Log($"[Texture Processing] EncodePNGAsync : {sw.Elapsed.TotalMilliseconds / iterations:F2} ms avg (encoding runs on background thread), {bytes.Length / 1024} KB");

        System.IO.File.WriteAllBytes(System.Environment.CurrentDirectory + "/unity_result.png", bytes);

       

    }

   
}