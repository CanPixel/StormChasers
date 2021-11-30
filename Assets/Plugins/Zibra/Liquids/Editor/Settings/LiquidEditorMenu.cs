#if UNITY_2019_4_OR_NEWER
using com.zibra.liquid.Plugins.Editor;
using UnityEditor;

namespace com.zibra.liquid
{
    internal static class LiquidEditorMenu
    {

        [MenuItem(ZibraAIPackage.RootMenu + "Info", false, 0)]
        public static void OpenSettings()
        {
            var windowTitle = LiquidSettingsWindow.WindowTitle;
            LiquidSettingsWindow.ShowTowardsInspector(windowTitle.text, windowTitle.image);
        }
    }
}
#endif
