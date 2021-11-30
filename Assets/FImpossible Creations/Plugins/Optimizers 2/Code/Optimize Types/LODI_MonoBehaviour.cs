using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Helper class for single LOD level settings on MonoBehaviour
    /// </summary>
    //[CreateAssetMenu(fileName = "MonoBehaviour Reference - Move it to Resources - Optimizers - Custom", menuName = "Custom Optimizers/FLOD_MonoBehaviour Reference")]
    [System.Serializable]
    public sealed class LODI_MonoBehaviour : ILODInstance
    {

        #region Main Settings : Interface Properties

        public int Index { get { return index; } set {  index = value; } }
        internal int index = -1;
        public string Name { get { return LODName; } set {  LODName = value; } }
        internal string LODName = "";
        public bool CustomEditor { get { return true; } }
        public bool Disable { get { return SetDisabled; } set {  SetDisabled = value; } }
        [HideInInspector] public bool SetDisabled = false;
        public bool DrawDisableOption { get { return true; } }
        public bool SupportingTransitions { get { return true; } }
        public bool DrawLowererSlider { get { return false; } }
        public float QualityLowerer { get { return 1f; } set {  new System.NotImplementedException(); } }
        public string HeaderText { get { return "MonoBehaviour LOD Settings"; } }
        public float ToCullDelay { get { return 0f; } }
        public int DrawingVersion { get { return ver; } set {  ver = value; } }
        [SerializeField] [HideInInspector] private int ver = 0;
        public Texture Icon { get { return null; } }// EditorGUIUtility.IconContent("cs Script Icon").image;
        public Component TargetComponent { get { return cmp; } }
        [SerializeField] [HideInInspector] private MonoBehaviour cmp;

        #endregion


        //public Type TypeOptimized;

        public bool BaseLOD = false;

        public UnityEvent Event;
        public List<ParameterHelper> Parameters;
        public List<ParameterHelper> NotSupported;
        internal bool DrawNotSupported = false;

#if UNITY_EDITOR
        private bool drawEvent = false;
#endif

        public static readonly int intId = "int".GetHashCode();
        public static readonly int floatId = "float".GetHashCode();
        public static readonly int boolId = "bool".GetHashCode();
        //private readonly int v2Id = "Vector2".GetHashCode();
        //private readonly int v3Id = "Vector3".GetHashCode();
        public static readonly int colorId = "Color".GetHashCode();



        public void SetSameValuesAsComponent(Component component)
        {
            if (component == null) Debug.LogError("[OPTIMIZERS] Given component is null instead of MonoBehaviour!");

            MonoBehaviour monoBeh = component as MonoBehaviour;
            //TypeOptimized = component.GetType();
            cmp = monoBeh;

            if (DrawingVersion == 0)
                if (monoBeh != null)
                {
#if UNITY_EDITOR
                    // Finding parameters available to optimize
                    SerializedObject s = new UnityEditor.SerializedObject(component);
                    if (Parameters == null) Parameters = new List<ParameterHelper>();
                    if (NotSupported == null) NotSupported = new List<ParameterHelper>();

                    var prop = s.GetIterator();
                    int safeLimit = 0;
                    prop.NextVisible(true); // ignoring "Script" field


                    while (prop.NextVisible(false))
                    {
                        //if (prop.displayName.ToLower().Contains("element")) break;

                        ParameterHelper helper = new ParameterHelper(prop.name, prop.type);
                        bool supported = true;

                        switch (prop.type)
                        {
                            case "float": helper.Float = prop.floatValue; break;
                            case "int": helper.Int = prop.intValue; break;
                            case "bool": helper.Bool = prop.boolValue; break;
                            //case "Vector2": helper.Vec2 = prop.vector2Value; break;
                            //case "Vector3": helper.Vec3 = prop.vector3Value; break;
                            case "Color": helper.Color = prop.colorValue; break;
                            default: supported = false; helper.Supported = false; break;
                        }

                        bool contains = false;
                        for (int p = 0; p < Parameters.Count; p++)
                            if (Parameters[p].ParamName == prop.name)
                            {
                                contains = true;
                                break;
                            }

                        if (!contains)
                        {
                            if (supported)
                                Parameters.Add(helper);
                            else
                                NotSupported.Add(helper);

                        }

                        if (++safeLimit > 1000) break;
                    }
#endif
                }
        }


        public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettingsReference)
        {
            if (DrawingVersion == 0)
            {
                // Casting LOD to correct type
                LODI_MonoBehaviour initialSettings = initialSettingsReference as LODI_MonoBehaviour;

                #region Security

                // Checking if casting is right
                if (initialSettings == null) { Debug.Log("[OPTIMIZERS] Target LOD is not MonoBehaviour LOD or is null"); return; }

                #endregion

                if (Parameters != null)
                    for (int i = 0; i < Parameters.Count; i++)
                    {
                        if (!Parameters[i].Change && BaseLOD == false) continue;

                        FieldInfo field = component.GetType().GetField(Parameters[i].ParamName);

                        if (field != null)
                        {
                            if (Parameters[i].TypeID == intId)
                                field.SetValue(component, Parameters[i].Int);
                            else if (Parameters[i].TypeID == floatId)
                                field.SetValue(component, Parameters[i].Float);
                            else if (Parameters[i].TypeID == boolId)
                                field.SetValue(component, Parameters[i].Bool);
                            else if (Parameters[i].TypeID == colorId)
                                field.SetValue(component, Parameters[i].Color);
                        }
                        else
                        {
                            Debug.LogError("[OPTIMIZERS] Not found field with name " + Parameters[i].ParamName + " in " + component.GetType() + " of " + component + " " + component.name);
                        }
                    }
            }

            if (Event != null) Event.Invoke();

            FLOD.ApplyEnableDisableState(this, component);
            //base.ApplySettingsToComponent(component, initialSettingsReference);
        }


        public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component component)
        {
            if (component == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not MonoBehaviour Component!");

            SetSameValuesAsComponent(component);

            Name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
        }

        public void AssignSettingsAsForCulled(Component component)
        {
            FLOD.AssignDefaultCulledParams(this);
            SetSameValuesAsComponent(component);
        }

        public void AssignSettingsAsForNearest(Component component)
        {
            FLOD.AssignDefaultNearestParams(this);
            SetSameValuesAsComponent(component);
            if (Parameters != null) for (int i = 0; i < Parameters.Count; i++) Parameters[i].Change = true;
        }

        public void AssignSettingsAsForHidden(Component component)
        {
            FLOD.AssignDefaultHiddenParams(this);
        }

        public ILODInstance GetCopy()
        {
            //Debug.Log("COPY");
            LODI_MonoBehaviour cpy = MemberwiseClone() as LODI_MonoBehaviour;

            cpy.Parameters = new List<ParameterHelper>();

            if (Parameters != null)
                for (int i = 0; i < Parameters.Count; i++)
                {
                    ParameterHelper helper = new ParameterHelper(Parameters[i].ParamName, Parameters[i].ParamType);
                    helper.SetValue(Parameters[i].TypeID, Parameters[i].GetValue(Parameters[i].TypeID));
                    helper.Change = Parameters[i].Change;
                    cpy.Parameters.Add(helper);
                }

            return cpy;
        }

        public void InterpolateBetween(ILODInstance lodA, ILODInstance lodB, float transitionToB)
        {
            FLOD.DoBaseInterpolation(this, lodA, lodB, transitionToB);
            if (DrawingVersion == 1) return;

            LODI_MonoBehaviour a = lodA as LODI_MonoBehaviour;
            LODI_MonoBehaviour b = lodB as LODI_MonoBehaviour;

            BaseLOD = b.BaseLOD;

            if (Parameters != null)
                for (int i = 0; i < Parameters.Count; i++)
                {
                    if (b.Parameters[i].Change)
                    {
                        Parameters[i].Change = true;
                    }

                    if (!a.BaseLOD)
                        if (!a.Parameters[i].Change)
                        {
                            Parameters[i].SetValue(Parameters[i].TypeID, b.Parameters[i].GetValue(Parameters[i].TypeID));
                            continue;
                        }

                    if (Parameters[i].TypeID == intId)
                    {
                        Parameters[i].Int = (int)Mathf.Lerp(a.Parameters[i].Int, b.Parameters[i].Int, transitionToB);
                    }
                    else if (Parameters[i].TypeID == floatId)
                    {
                        Parameters[i].Float = Mathf.Lerp(a.Parameters[i].Float, b.Parameters[i].Float, transitionToB);
                    }
                    else if (Parameters[i].TypeID == boolId)
                    {
                        if (transitionToB > 0.5f)
                            Parameters[i].Bool = b.Parameters[i].Bool;
                        else
                            Parameters[i].Bool = a.Parameters[i].Bool;
                    }
                    else if (Parameters[i].TypeID == colorId)
                    {
                        Parameters[i].Color = Color.Lerp(a.Parameters[i].Color, b.Parameters[i].Color, transitionToB);
                    }
                }
        }




        public void AssignToggler(ILODInstance reference)
        { }

#if UNITY_EDITOR
        public void DrawTogglers(SerializedProperty iflodProp)
        { }

        public void CustomEditorWindow(SerializedProperty prop)
        {
            if (Parameters == null) Parameters = new List<ParameterHelper>();
            if (Parameters.Count != 0) DrawingVersion = 0;

            if (NotSupported == null) NotSupported = new List<ParameterHelper>();

            SerializedObject s = prop.serializedObject;
            Undo.RecordObject(s.targetObject, "Changing custom component parameters");

            bool preEnabled = GUI.enabled;

            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i].DrawParameter();
            }

            preEnabled = GUI.enabled;
            GUI.enabled = false;

            if (NotSupported.Count > 0)
            {
                EditorGUI.indentLevel++;
                DrawNotSupported = EditorGUILayout.Foldout(DrawNotSupported, "Not Supported Variables", true);
                if (DrawNotSupported)
                    for (int i = 0; i < NotSupported.Count; i++)
                    {
                        EditorGUILayout.LabelField(new GUIContent("Not Supported Type (" + NotSupported[i].ParamType + ")", "You can create custom implementation to support all your component variables, check documentation for more (" + NotSupported[i].ParamName + ")"));
                    }

                EditorGUI.indentLevel--;
            }

            GUI.enabled = preEnabled;

            EditorGUI.indentLevel++;
            drawEvent = EditorGUILayout.Foldout(drawEvent, "Draw Custom Event", true);
            EditorGUI.indentLevel--;

            if (drawEvent)
            {
                SerializedProperty eventProp = prop.FindPropertyRelative("Event");
                if (eventProp != null) EditorGUILayout.PropertyField(eventProp, true);
            }

            s.ApplyModifiedProperties();

        }
#endif

        public void Simplify()
        {
            if (Parameters == null) Parameters = new List<ParameterHelper>();
            else Parameters.Clear();

            if (NotSupported == null) NotSupported = new List<ParameterHelper>();
            else NotSupported.Clear();

            DrawingVersion = 1;
        }


        /// <summary>
        /// Since I can't use dictionaries because they aren't serialable
        /// </summary>
        [System.Serializable]
        public class ParameterHelper
        {
            public bool Change = false;
            public int ParamID;
            public int TypeID;
            public string ParamName;
            public string ParamType;
            public bool Supported = true;

            // Can't use 'object' cause it is not serializable
            public int Int;
            public float Float;
            public Vector2 Vec2;
            public Vector3 Vec3;
            public Color Color;
            public bool Bool;

            public ParameterHelper(string name, string type)
            {
                ParamID = name.GetHashCode();
                ParamName = name;

                TypeID = type.GetHashCode();
                ParamType = type;
                Supported = true;
            }

            public void SetValue(int valueId, object value)
            {
                if (valueId == intId)
                    Int = (int)value;
                else if (valueId == floatId)
                    Float = (float)value;
                else if (valueId == boolId)
                    Bool = (bool)value;
                else if (valueId == colorId)
                    Color = (Color)value;
            }

            public object GetValue(int valueId)
            {
                if (valueId == intId) return Int;
                else if (valueId == floatId) return Float;
                else if (valueId == boolId) return Bool;
                else if (valueId == colorId) return Color;
                return null;
            }

            public void DrawParameter()
            {
#if UNITY_EDITOR
                bool preEnabled = GUI.enabled;
                if (!Change) GUI.enabled = false;

                bool viewX = true;

                EditorGUILayout.BeginHorizontal();

                if (TypeID == intId)
                {
                    Int = EditorGUILayout.IntField(ParamName, Int);
                }
                else if (TypeID == floatId)
                {
                    Float = EditorGUILayout.FloatField(ParamName, Float);
                }
                else if (TypeID == boolId)
                {
                    Bool = EditorGUILayout.Toggle(ParamName, Bool);
                }
                else if (TypeID == colorId)
                {
                    Color = EditorGUILayout.ColorField(ParamName, Color);
                }
                else
                {
                    viewX = false;
                    EditorGUILayout.EndHorizontal();
                    GUI.enabled = false;
                    EditorGUILayout.LabelField(new GUIContent("Not Supported Type (" + ParamType + ")", "You can create custom implementation to support all your component variables, check documentation for more (" + ParamName + ")"));
                    GUI.enabled = preEnabled;
                }

                if (viewX)
                {
                    GUILayout.FlexibleSpace();
                    GUI.enabled = true;
                    Change = EditorGUILayout.Toggle("", Change, new GUILayoutOption[2] { GUILayout.Width(20), GUILayout.Height(14) });
                    GUI.enabled = preEnabled;
                    EditorGUILayout.EndHorizontal();
                }

                GUI.enabled = preEnabled;
#endif
            }
        }
    }
}
