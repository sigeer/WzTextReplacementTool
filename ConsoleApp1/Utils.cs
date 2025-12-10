using MapleLib.WzLib;
using MapleLib.WzLib.Util;

namespace ConsoleApp1
{
    internal class Utils
    {
        public static void SaveImg(string outputPath, WzImage img, WzMapleVersion version)
        {
            using var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var imgWriter = new WzBinaryWriter(outputStream, WzTool.GetIvByMapleVersion(version));
            img.SaveImage(imgWriter, forceReadFromData: true);
        }
    }
}
