using System;
using System.Collections.Generic;
using System.Web;

using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.Text;
using System.Xml.Linq;
using ImageMagick;

namespace FishEDexWebAPI.Controllers
{
    public static class ControllerHelpers
    {
        //note this is synced to /theme/_sass.scss
        private static int ThumbnailResolution = 80;
        private static int TileResolution = 400;
        public static bool IsValidImage(HttpPostedFileBase imageFile)
        {
            //accepts bmp,gif,icon,jpg,png,tiff,svg
            if (imageFile != null && imageFile.ContentLength != 0 && imageFile.ContentLength < 10240000)
            {
                using (var reader = new BinaryReader(imageFile.InputStream, Encoding.Default, true))
                {
                    //check magic bytes
                    if (imageFile.ContentType == "image/bmp")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[2];
                        buffer = reader.ReadBytes(2);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x42 && buffer[1] == 0x4D)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/gif")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[4];
                        buffer = reader.ReadBytes(4);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/x-icon")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[4];
                        buffer = reader.ReadBytes(4);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x00 && buffer[1] == 0x00 && buffer[2] == 0x01 && buffer[3] == 0x00)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/jpeg")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[3];
                        buffer = reader.ReadBytes(3);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/png")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[8];
                        buffer = reader.ReadBytes(8);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47 && buffer[4] == 0x0D && buffer[5] == 0x0A && buffer[6] == 0x1A && buffer[7] == 0x0A)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/tif" || imageFile.ContentType == "image/tiff")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[4];
                        buffer = reader.ReadBytes(4);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x49 && buffer[1] == 0x49 && buffer[2] == 0x2A && buffer[3] == 0x00)
                        {
                            return true;
                        }
                        if (buffer[0] == 0x4D && buffer[1] == 0x4D && buffer[2] == 0x00 && buffer[3] == 0x2A)
                        {
                            return true;
                        }
                    }
                    else if (imageFile.ContentType == "image/svg+xml")
                    {
                        var pos = imageFile.InputStream.Position;
                        byte[] buffer = new byte[6];
                        buffer = reader.ReadBytes(6);
                        imageFile.InputStream.Position = pos;
                        if (buffer[0] == 0x3C && buffer[1] == 0x3F && buffer[2] == 0x78 && buffer[3] == 0x6D && buffer[4] == 0x6C && buffer[5] == 0x20)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static void DeleteBlob(string URL, CloudBlobContainer blobContainer)
        {
            Uri blobUri = new Uri(URL);
            string blobName = blobUri.Segments[blobUri.Segments.Length - 1];
            CloudBlockBlob blobToDelete = blobContainer.GetBlockBlobReference(blobName);
            blobToDelete.Delete();
        }

        //delete all blobs 
        public static void DeleteBlobs(object ad, CloudBlobContainer blobContainer)
        {
            if (ad != null)
            {
                //Delete full-size images
                try
                {
                    var imageproperty = ad.GetType().GetProperty("ImageURL");
                    if (imageproperty != null)
                    {
                        var imageURL = imageproperty.GetValue(ad, null) as string;

                        if (!string.IsNullOrWhiteSpace(imageURL))
                        {
                            DeleteBlob(imageURL, blobContainer);
                        }
                    }
                }
                catch
                {
                }
                //Delete thumbnail images
                try
                {
                    var thumbnailproperty = ad.GetType().GetProperty("ThumbnailURL");
                    if (thumbnailproperty != null)
                    {
                        var thumbnailURL = thumbnailproperty.GetValue(ad, null) as string;
                        if (!string.IsNullOrWhiteSpace(thumbnailURL))
                        {
                            DeleteBlob(thumbnailURL, blobContainer);
                        }
                    }
                }
                catch
                {
                }
                //Delete tile images
                try
                {
                    var tileproperty = ad.GetType().GetProperty("TileURL");
                    if (tileproperty != null)
                    {
                        var tileURL = tileproperty.GetValue(ad, null) as string;
                        if (!string.IsNullOrWhiteSpace(tileURL))
                        {
                            DeleteBlob(tileURL, blobContainer);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public static CloudBlockBlob UploadBlobFile(Stream inputStream, string contentType, string outputName, CloudBlobContainer blobContainer)
        {
            CloudBlockBlob outputBlob = null;
            using (Stream image = SafeImage(inputStream, contentType))
            {
                outputBlob = UploadStream(image, contentType, outputName + ImageFormatHelpers.GetExtensionFromContentType(contentType), blobContainer);
            }
            return outputBlob;
        }

        public static CloudBlockBlob UploadBlobThumb(Stream inputStream, string contentType, string outputName, CloudBlobContainer blobContainer, bool crop = false)
        {
            CloudBlockBlob outputBlob = null;
            using (Stream safeStream = SafeImage(inputStream, contentType))
            {
                using (MagickImage safeImage = OpenStream(safeStream, contentType))
                {
                    ResizeImage(safeImage, ThumbnailResolution, crop);
                    outputBlob = UploadImage(safeImage, "image/png", outputName + "thumb.png", blobContainer);
                }
            }
            return outputBlob;
        }
        public static CloudBlockBlob UploadBlobTile(Stream inputStream, string contentType, string outputName, CloudBlobContainer blobContainer, bool crop = false)
        {
            CloudBlockBlob outputBlob = null;
            using (Stream safeStream = SafeImage(inputStream, contentType))
            {
                using (MagickImage safeImage = OpenStream(safeStream, contentType))
                {
                    ResizeImage(safeImage, TileResolution, crop);
                    outputBlob = UploadImage(safeImage, "image/png", outputName + "tile.png", blobContainer);
                }
            }
            return outputBlob;
        }
        public static CloudBlockBlob UploadStream(Stream stream, string contentType, String blobName, CloudBlobContainer blobContainer)
        {
            CloudBlockBlob outputBlob = blobContainer.GetBlockBlobReference(blobName);
            using (Stream outputStream = outputBlob.OpenWrite())
            {
                stream.CopyTo(outputStream);
            }
            outputBlob.Properties.ContentType = contentType;
            outputBlob.SetProperties();
            return outputBlob;
        }
        public static CloudBlockBlob UploadImage(MagickImage image, string contentType, String blobName, CloudBlobContainer blobContainer)
        {
            CloudBlockBlob outputBlob = blobContainer.GetBlockBlobReference(blobName);
            using (Stream outputStream = outputBlob.OpenWrite())
            {
                image.Write(outputStream, ImageFormatHelpers.GetFormatFromContentType(contentType));
            }
            outputBlob.Properties.ContentType = contentType;
            outputBlob.SetProperties();
            return outputBlob;
        }
        public static MagickImage OpenStream(Stream inputStream, string contentType)
        {
            if (contentType == "image/svg+xml")
            {
                MagickReadSettings settings = new MagickReadSettings() { Format = MagickFormat.Svg, Height = 400 };
                MagickImage image = new MagickImage(inputStream, settings);
                return image;
            }
            else
            {
                return new MagickImage(inputStream);
            }
        }
        public static Stream SafeImage(Stream inputStream, string contentType)
        {
            var pos = inputStream.Position;
            Stream s = new MemoryStream();
            //check for svg type on stream
            //either use contentype or stream startswith("<?xml")
            if (contentType == "image/svg+xml")
            {
                //remove script from svg
                XDocument xd = XDocument.Load(inputStream);
                xd.Elements("script").Remove();
                xd.Save(s);
            }
            else
            {
                //remove metadata
                using (MagickImage image = new MagickImage(inputStream))
                {
                    foreach (var profile in image.ProfileNames)
                    {
                        image.RemoveProfile(profile);
                    }
                    image.Write(s);
                }
            }
            inputStream.Position = pos;
            s.Position = 0;
            return s;
        }
        public static void ResizeImage(MagickImage originalImage, int imageSize, bool crop = false)
        {
            //do nothing
            if (originalImage.Width < imageSize && originalImage.Height < imageSize)
            {
            }
            //square crop, scale, and center
            else if (crop)
            {
                int x;
                int y;
                int width;
                int height;
                if (originalImage.Width > originalImage.Height)
                {
                    y = 0;
                    x = (originalImage.Width - originalImage.Height) / 2;
                    width = originalImage.Height;
                    height = originalImage.Height;
                }
                else
                {
                    y = (originalImage.Height - originalImage.Width) / 2;
                    x = 0;
                    width = originalImage.Width;
                    height = originalImage.Width;
                }
                originalImage.Crop(x, y, width, height);
                originalImage.Resize(imageSize, imageSize);
            }
            //no crop, only resize
            else
            {
                int width;
                int height;
                if (originalImage.Width > originalImage.Height)
                {
                    width = imageSize;
                    height = originalImage.Height * imageSize / originalImage.Width;
                }
                else
                {
                    width = originalImage.Width * imageSize / originalImage.Height;
                    height = imageSize;
                }
                originalImage.Resize(imageSize, imageSize);
            }
        }


    }
}