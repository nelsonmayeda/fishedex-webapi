using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ImageMagick;

namespace FishEDexWebAPI.Controllers
{
    public static class ImageFormatHelpers
    {
        private static readonly IDictionary<string, string> _ExtensionMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
        {"image/bmp", ".bmp"},
        {"image/gif", ".gif"},
        {"image/x-icon", ".ico"},
        {"image/jpeg",".jpg"},
        {"image/png", ".png"},
        {"image/tif",".tif"},
        {"image/tiff",".tiff"},
        {"image/svg+xml",".svg"}
        };

        public static string GetExtensionFromContentType(string contentType)
        {
            string extension;

            return _ExtensionMappings.TryGetValue(contentType, out extension) ? extension : "";
        }
        private static readonly IDictionary<string, MagickFormat> _FormatMappings = new Dictionary<string, MagickFormat>(StringComparer.InvariantCultureIgnoreCase)
        {
        {"image/bmp", MagickFormat.Bmp},
        {"image/gif", MagickFormat.Gif},
        {"image/x-icon", MagickFormat.Icon},
        {"image/jpeg", MagickFormat.Jpeg},
        {"image/png", MagickFormat.Png},
        {"image/tif", MagickFormat.Tif},
        {"image/tiff", MagickFormat.Tiff},
        {"image/svg+xml", MagickFormat.Svg}
        };
        public static MagickFormat GetFormatFromContentType(string contentType)
        {
            MagickFormat mime;

            return _FormatMappings.TryGetValue(contentType, out mime) ? mime : MagickFormat.Unknown;
        }
    }
}