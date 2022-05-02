using System;
using System.IO;

namespace IxMilia.BCad.Helpers
{
    public static class ImageHelpers
    {
        public static (int, int) GetImageDimensions(string imagePath, byte[] imageData)
        {
            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            return extension switch
            {
                ".png" => GetPngDimensions(imageData),
                ".jpg" => GetJpegDimensions(imageData),
                ".jpeg" => GetJpegDimensions(imageData),
                _ => throw new NotSupportedException(),
            };
        }
        
        private static (int, int) GetPngDimensions(byte[] array)
        {
            var width = (int)ToUInt32BigEndian(array, 16);
            var height = (int)ToUInt32BigEndian(array, 20);
            return (width, height);
        }

        private static (int, int) GetJpegDimensions(byte[] array)
        {
            for (int i = 0; i < array.Length - 9; i++)
            {
                if (array[i] == 0xFF && array[i + 1] == 0xC0)
                {
                    var height = ToUInt16BitEndian(array, i + 5);
                    var width = ToUInt16BitEndian(array, i + 7);
                    return (width, height);
                }
            }

            return (0, 0);
        }

        private static int ToUInt16BitEndian(byte[] array, int startIndex)
        {
            return (array[startIndex] << 8) | array[startIndex + 1];
        }

        private static uint ToUInt32BigEndian(byte[] array, int startIndex)
        {
            return (uint)((array[startIndex] << 24) | (array[startIndex + 1] << 16) | (array[startIndex + 2] << 8) | array[startIndex + 3]);
        }
    }
}
