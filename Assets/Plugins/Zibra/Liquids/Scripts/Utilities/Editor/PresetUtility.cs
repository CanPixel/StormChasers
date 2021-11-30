using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;

namespace com.zibra.liquid.Editor.Utilities
{
    public static class PresetUtility
    {
        public static void ApplyPresetExcludingProperties(Preset preset, Object target,
                                                          params string[] excludedPropertyPaths)
        {
            var appliedPropertyPaths = GetAllPropertyPaths(target);

            foreach (var excludedPropertyPath in excludedPropertyPaths)
            {
                appliedPropertyPaths.Remove(excludedPropertyPath);
            }

            preset.ApplyTo(target, appliedPropertyPaths.ToArray());
        }

        public static List<string> GetAllPropertyPaths(Object target)
        {
            var serializedObject = new SerializedObject(target);
            var propertyPaths = new List<string>(10);
            var serializedProperty = serializedObject.GetIterator();

            if (serializedProperty.NextVisible(true))
            {
                while (serializedProperty.NextVisible(false))
                {
                    propertyPaths.Add(serializedProperty.propertyPath);
                }
            }

            return propertyPaths;
        }
    }
}