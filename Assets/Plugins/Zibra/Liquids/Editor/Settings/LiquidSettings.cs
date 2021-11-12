using System.Collections.Generic;
using com.zibra.liquid.Plugins;
using UnityEditor;

namespace com.zibra.liquid
{
    /// <summary>
    /// Scene Management Settings scriptable object.
    /// You can modify this settings using C# or Scene Management Editor Window.
    /// </summary>
    internal class LiquidSettings : PackageScriptableSettingsSingleton<LiquidSettings>
    {
        protected override bool IsEditorOnly => true;
        public override string PackageName => ZibraAIPackage.PackageName;
    }
}
