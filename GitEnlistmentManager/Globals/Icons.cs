using System.Collections.Generic;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace GitEnlistmentManager.Globals
{
    public static class Icons
    {
        private static Dictionary<string, BitmapImage> ImageCache = new Dictionary<string, BitmapImage>();

        public static BitmapImage? GemIcon => GetBitMapImage("gem.png");

        public static BitmapImage? GetBitMapImage(string imagePath)
        {
            var resourcePath = $"GitEnlistmentManager.Images.{imagePath}";

            if (!ImageCache.ContainsKey(resourcePath))
            {
                var assembly = Assembly.GetExecutingAssembly();

                using (var stream = assembly.GetManifestResourceStream(resourcePath))
                {
                    if (stream != null)
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.StreamSource = stream;
                        image.EndInit();
                        ImageCache[resourcePath] = image;
                    }
                }
            }

            return ImageCache.ContainsKey(resourcePath) ? ImageCache[resourcePath] : null;
        }
    }
}
