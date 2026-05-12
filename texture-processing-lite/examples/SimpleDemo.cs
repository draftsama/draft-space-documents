using UnityEngine;
using UnityEngine.UI;
namespace Draft.TextureProcessing.Demo
{
    public class SimpleDemo : MonoBehaviour
    {
        [SerializeField] private Texture2D _sourceTexture;
        [SerializeField] private RawImage _previewImage;
        async void Start()
        {
            // Initialize shaders (optional)
            TextureProcessor.Initialize();

            // Example: Flip the texture horizontally
            var flipped = TextureProcessor.FlipX(_sourceTexture);

            // Example: Resize the texture to 256x256 using Fit mode
            var resized = flipped.FastResize(512, 512, TextureProcessor.ResizeMode.Fit);

            // Example: Chain multiple operations: Rotate 90 degrees, then apply rounded corners with a radius of 50 pixels
            var result = TextureProcessor.BeginChain(resized).Rotate(90).RoundedCorners(50).Execute();
            
            // Note: The chained version may be more efficient as it can optimize the sequence of operations internally, while the individual calls may create intermediate textures at each step.
            //TextureProcessor.BeginChain(resized).Rotate(90).RoundedCorners(50).Execute(); vs resized.FastRotate(90).FastCropRoundedCorners(50);
        

            //save composited texture to disk (for testing)
            var bytes = result.EncodeToPNG();
            var savePath = System.IO.Path.Combine(Application.persistentDataPath, "output.png");
            await System.IO.File.WriteAllBytesAsync(savePath, bytes);


            // Display the result
            _previewImage.texture = result;

            // Clean up shaders when done (optional)
            TextureProcessor.ReleaseShaders();
        }

    }
}
