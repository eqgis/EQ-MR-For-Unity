using Eqgis.Editor;
using UnityEditor;

namespace Holo.XR.Editor.Installer
{
    public class Installer
    {
        [MenuItem("Installer/Install HybirdCLR", false, 101)]
        public static void InstallHybirdCLR()
        {
            HybridCLRInstaller.Import();
        }

        [MenuItem("Installer/Install ARCore", false, 102)]
        public static void InstallARCore()
        {
            ARCoreInstaller.Import();
        }
    }
}