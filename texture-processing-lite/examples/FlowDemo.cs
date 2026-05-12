using UnityEngine;
using UnityEngine.UI;
namespace Draft.TextureProcessing.Demo
{
    public class FlowDemo : MonoBehaviour
    {

        [SerializeField] private Texture2D _sourceTexture;
        [SerializeField] private TextureFlow _flow;
        [SerializeField] private RawImage _previewImage;

        AspectRatioFitter _aspectFitter;

        async void Start()
        {
            try
            {
                //start time
                var startTime = Time.realtimeSinceStartup;
                _aspectFitter = _previewImage.GetComponent<AspectRatioFitter>();
                var result = _flow.Execute(_sourceTexture);
                var endTime = Time.realtimeSinceStartup;
                Debug.Log($"Execution time: {endTime - startTime} seconds");
                Debug.Log($"Result size: {result.width}x{result.height}");
                _previewImage.texture = result;
                var ratio = (float)result.width / result.height;
                _aspectFitter.aspectRatio = ratio;


                var savePath = System.Environment.CurrentDirectory + "/0001.png";
                var success = await result.SavePNGAsync(savePath);

                if (success)
                {
                    Debug.Log($"Saved result to: {savePath}");
                }
                else
                {
                    Debug.LogError($"Failed to save result to: {savePath}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error during flow execution: {ex.Message}");
            }
        }


    }
}
