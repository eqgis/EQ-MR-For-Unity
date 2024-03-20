using System.IO;

namespace Holo.XR.Utils
{
    class FileUtils
    {
        public static void DirectoryDeleteRF(string path)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            if (null != di && di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                di.Delete(true);
            }
        }

        public static void FileDeleteByExtension(string path, string extension, bool recurisive)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(path);
            if (null != di && di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    if (file.Extension.Equals(extension))
                        file.Delete();
                }
                if (recurisive)
                {
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        FileDeleteByExtension(path + dir.Name + "/", extension, recurisive);
                    }
                }
            }
        }

        public static void CopyDirectory(string src, string dest, bool recurisive)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(src);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + src);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(dest))
            {
                Directory.CreateDirectory(dest);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(dest, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (recurisive)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(dest, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, recurisive);
                }
            }
        }

        public static void CopyFile(string src, string dest, bool overwrite)
        {
            File.Copy(src, dest, overwrite);
        }

        public static string ReplaceExtension(string src, string ext)
        {
            string src_path = Path.GetDirectoryName(src);
            string src_file = Path.GetFileNameWithoutExtension(src);
            return src_path + "/" + src_file + ext;
        }
    }
}