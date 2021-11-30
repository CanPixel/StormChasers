// Sometimes we need to be outside editor directory to detect some of the namespaces

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

namespace FIMSpace.FEditor
{
    /// <summary>
    /// FM: Class with methods to help define if some scripts are inside the project
    /// </summary>
    sealed class FDefinesCompilation
    {
        // EXAMPLES OF USAGE
        //[InitializeOnLoad]
        //sealed class FIcons_DefineAdressables
        //{
        //    const string define = "ADRESSABLES_IMPORTED";

        //    static FIcons_DefineAdressables()
        //    {
        //        if (FDefinesCompilation.GetTypesInNamespace("UnityEngine.AddressableAssets").Count > 0)
        //            FDefinesCompilation.SetDefine(define);
        //        else
        //            FDefinesCompilation.RemoveDefine(define);
        //    }
        //}
        // THEN IN CODE
        // #if ADRESSABLES_IMPORTED
        // // When adressables are imported inside project
        // #endif

        public static List<Type> GetTypesInNamespace(string nameSpace, string root = "Assembly")
        {
            List<Type> childTypes = new List<Type>();

            Type[] types;
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int assemblyId = 0; assemblyId < assemblies.Length; assemblyId++)
            {
                bool can = false;
                if (root == "" || assemblies[assemblyId].FullName.StartsWith("Assembly")) can = true;

                if (can)
                {
                    if (assemblies[assemblyId] != null)
                    {
                        try
                        {
                            types = assemblies[assemblyId].GetTypes();
                            for (int typeI = 0; typeI < types.Length; typeI++)
                                if (!string.IsNullOrEmpty(types[typeI].Namespace))
                                {
                                    if (types[typeI].Namespace.StartsWith(nameSpace))
                                    {
                                        childTypes.Add(types[typeI]);
                                    }
                                }
                        }
                        catch (Exception)
                        {
                            //UnityEngine.Debug.Log("Couldn't Load " + assemblies[assemblyId].GetName());
                            continue;
                        }
                    }
                }
            }

            return childTypes;
        }


        public static void SetDefine(string newDefine)
        {
            string buildSettings = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

            if (!buildSettings.Contains(newDefine.ToUpper()))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, buildSettings + ";" + newDefine.ToUpper());
            }
        }


        public static void RemoveDefine(string def)
        {
            if (IsDefined(def))
            {
                string[] currentDefs = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
                string newDefs = "";

                for (int i = 0; i < currentDefs.Length; ++i)
                {
                    if (currentDefs[i] != def) newDefs += currentDefs[i] + ";";
                }

                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, newDefs);
            }
        }


        public static bool IsDefined(string def)
        {
            string[] currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < currentSymbols.Length; ++i)
            {
                if (currentSymbols[i] == def) return true;
            }

            return false;
        }

    }
}
