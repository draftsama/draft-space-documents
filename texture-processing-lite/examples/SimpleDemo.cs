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
           var resized = await _sourceTexture.FastResizeAsync( 512, 512, TextureProcessor.ResizeMode.Fit);

            // Example: Rotate the texture 90 degrees clockwise and rounded corners with radius 50
            //var composited = resized.FastRotate(90).FastCropRounded(50);


            //save composited texture to disk (for testing)
            // var bytes = composited.EncodeToPNG();
            // var savePath = System.IO.Path.Combine(Application.persistentDataPath, "output.png");
            // await System.IO.File.WriteAllBytesAsync(savePath, bytes);


            // Display the result
            _previewImage.texture = flipped;

            // Clean up shaders when done (optional)
            TextureProcessor.ReleaseShaders();
        }

    }
}
