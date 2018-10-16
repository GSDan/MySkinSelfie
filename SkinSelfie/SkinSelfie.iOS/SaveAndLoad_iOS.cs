using System;
using System.IO;
using SkinSelfie.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(SaveAndLoad_iOS))]
namespace SkinSelfie.iOS
{
	public class SaveAndLoad_iOS : ISaveAndLoad
	{
		private string TempFileLoc = "";

		public string GetFileLoc()
		{
			if (!string.IsNullOrWhiteSpace(TempFileLoc)) return TempFileLoc;

			string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			string lib = Path.Combine(documents, "Library");

			if (!Directory.Exists(lib))
			{
				Directory.CreateDirectory(lib);
			}

			string cache = Path.Combine(lib, "Caches");

			if (!Directory.Exists(cache))
			{
				Directory.CreateDirectory(cache);
			}

			TempFileLoc = cache;
			return cache;
		}

		private void DeleteEmptyImages(string p)
		{
			string[] a = Directory.GetFiles(p, "*.jpg");

			foreach (string name in a)
			{
				// Use FileInfo to get length of each file.
				FileInfo info = new FileInfo(name);
				if (info.Length <= 0)
				{
					File.Delete(info.FullName);
				}
			}
		}

		public long GetDirectorySize(string p)
		{
			// Get array of all file names.
			string[] a = Directory.GetFiles(p, "*.jpg");

			// Calculate total bytes of all files in a loop.
			long b = 0;
			foreach (string name in a)
			{
				// Use FileInfo to get length of each file.
				FileInfo info = new FileInfo(name);
				b += info.Length;
			}
			// Return total size
			return b;
		}

		public string SaveLocalCopy(byte[] data, string filename)
		{
			string localImgPath = Path.Combine(GetFileLoc(), filename);

			if (File.Exists(localImgPath)) File.Delete(localImgPath);

			File.WriteAllBytes(localImgPath, data);

			return localImgPath;
		}

		public byte[] LoadFromFile(string filepath)
		{
			if (!File.Exists(filepath)) return null;

			return File.ReadAllBytes(filepath);
		}

		public void DeleteLocalCopy(string filepath)
		{
			if (File.Exists(filepath))
			{
				File.Delete(filepath);
			}
		}
	}
}
