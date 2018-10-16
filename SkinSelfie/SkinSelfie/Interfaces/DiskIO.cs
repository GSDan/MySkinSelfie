using System.IO;

namespace SkinSelfie.Interfaces
{
    public interface DiskIO
    {
        string[] SaveImage(string filename, byte[] data);
        byte[] Load(string filename);
    }
}
