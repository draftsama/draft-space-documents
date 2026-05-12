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

        void Start()
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
        }

       
    }
}
