using Android.Graphics;
using Java.Lang;
using SkinSelfie.Droid;
using SkinSelfie.Interfaces;
using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(DiskIO_Droid))]
namespace SkinSelfie.Droid
{
    public class DiskIO_Droid :DiskIO
    {
        public string[] SaveImage(string filename, byte[] data)
        {
            var documentsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString();
            var filePath = System.IO.Path.Combine(documentsPath, filename);
            File.WriteAllBytes(filePath, data);

            // Read back file as bitmap and get resolution & image ratio
            Stream input = new MemoryStream(data);
            BitmapFactory.Options onlyBoundsOptions = new BitmapFactory.Options();
            onlyBoundsOptions.InJustDecodeBounds = true;
            BitmapFactory.DecodeStream(input, null, onlyBoundsOptions);
            input.Close();

            if ((onlyBoundsOptions.OutWidth == -1) || (onlyBoundsOptions.OutHeight == -1))
                return null;

            int originalSize = (onlyBoundsOptions.OutHeight > onlyBoundsOptions.OutWidth)
                              ? onlyBoundsOptions.OutHeight
                              : onlyBoundsOptions.OutWidth;
            double ratio = (originalSize > 350) ? (originalSize / 350) : 1.0;

            // Use this ratio to open the image into a much smaller bitmap
            BitmapFactory.Options bitmapOptions = new BitmapFactory.Options();
            bitmapOptions.InSampleSize = GetPowerOfTwoForSampleRatio(ratio);
            input = new MemoryStream(data);
            Bitmap bitmap = BitmapFactory.DecodeStream(input, null, bitmapOptions);
            input.Close();

            // Save this new bitmap as the image's thumbnail and return both addresses
            var thumbFilename = System.IO.Path.GetFileNameWithoutExtension(filename) + "_thumb" + System.IO.Path.GetExtension(filename);
            var thumbPath = System.IO.Path.Combine(documentsPath, thumbFilename);
            var thumbStream = new FileStream(thumbPath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Jpeg, 80, thumbStream);
            thumbStream.Close();

            return new string[] { filePath,  thumbPath};
        }

        private static int GetPowerOfTwoForSampleRatio(double ratio)
        {
            int k = Integer.HighestOneBit((int)Java.Lang.Math.Floor(ratio));
            if (k == 0) return 1;
            else return k;
        }

        public byte[] Load(string filename)
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            var filePath = System.IO.Path.Combine(documentsPath, filename);
            return File.ReadAllBytes(filePath);
        }
    }
}