using com.zibra.liquid.Solver;
using com.zibra.liquid.SDFObjects;
using com.zibra.liquid.Manipulators;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace com.zibra.liquid.Editor.Solver
{
    [CustomEditor(typeof(ZibraLiquid), true)]
    public class ZibraLiquidEditor : UnityEditor.Editor
    {
        public static int liquidCount = 0;

        [MenuItem("GameObject/Zibra/Zibra Liquid", false, 10)]
        private static void CreateZibraLiquid(MenuCommand menuCommand)
        {
            liquidCount++;
            // Create a custom game object
            var go = new GameObject("Zibra Liquid " + liquidCount);
            go.AddComponent<ZibraLiquid>();
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

        [MenuItem("GameObject/Zibra/Zibra Liquid Emitter", false, 10)]
        private static void CreateZibraEmitter(MenuCommand menuCommand)
        {
#if !ZIBRA_LIQUID_PAID_VERSION
            if (Manipulator.AllManipulators.Count >= 1)
                return;
#endif

            // Create a custom game object
            String name = "ZibraLiquid Emitter " + (Manipulator.AllManipulators.Count + 1);
            var go = new GameObject(name);
            go.AddComponent<ZibraLiquidEmitter>();
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }

#if ZIBRA_LIQUID_PAID_VERSION
        [MenuItem("GameObject/Zibra/Zibra Liquid Void", false, 10)]
        private static void CreateZibraVoid(MenuCommand menuCommand)
        {
            String name = "ZibraLiquid Void " + (Manipulator.AllManipulators.Count + 1);
            // Create a custom game object
            var go = new GameObject(name);
            go.AddComponent<ZibraLiquidVoid>();
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Register the creation in the undo system
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
#endif

        private enum EditMode
        {
            None,
            Container,
            Emitter
        }

        private static readonly Color containerColor = new Color(1f, 0.8f, 0.4f);

        private ZibraLiquid ZibraLiquidInstance;

        private SerializedProperty TimeStepMax;
        private SerializedProperty SimTimePerSec;

        private SerializedProperty MaxParticleNumber;
        private SerializedProperty IterationsPerFrame;
        private SerializedProperty GridResolution;
        private SerializedProperty RunSimulation;
        private SerializedProperty ContainerReference;
        private SerializedProperty sdfColliders;
        private SerializedProperty manipulators;
        private SerializedProperty allManipulators;
        private SerializedProperty allSDFColliders;
        private SerializedProperty reflectionProbe;
        private SerializedProperty customLight;

        private bool colliderDropdownToggle = true;
        private bool manipulatorDropdownToggle = true;
        private bool statsDropdownToggle = true;
        private bool footprintDropdownToggle = true;
        private EditMode editMode;
        private readonly BoxBoundsHandle boxBoundsHandleContainer = new BoxBoundsHandle();

        private GUIStyle containerText;

        protected void OnEnable()
        {
            ZibraLiquidInstance = target as ZibraLiquid;

#if UNITY_PIPELINE_HDRP
            customLight = serializedObject.FindProperty("customLight");
#endif
            reflectionProbe = serializedObject.FindProperty("reflectionProbe");
            TimeStepMax = serializedObject.FindProperty("timeStepMax");
            SimTimePerSec = serializedObject.FindProperty("simTimePerSec");

            MaxParticleNumber = serializedObject.FindProperty("MaxNumParticles");
            IterationsPerFrame = serializedObject.FindProperty("iterationsPerFrame");
            GridResolution = serializedObject.FindProperty("gridResolution");

            RunSimulation = serializedObject.FindProperty("runSimulation");
            ContainerReference = serializedObject.FindProperty("containerReference");

            manipulators = serializedObject.FindProperty("manipulators");
            allManipulators = serializedObject.FindProperty("allManipulators");

            sdfColliders = serializedObject.FindProperty("sdfColliders");
            allSDFColliders = serializedObject.FindProperty("allSDFColliders");

            containerText = new GUIStyle { alignment = TextAnchor.MiddleLeft, normal = { textColor = containerColor } };
        }

        protected void OnSceneGUI()
        {
            if (ZibraLiquidInstance == null)
            {
                Debug.LogError("ZibraLiquidEditor not attached to ZibraLiquid component.");
                return;
            }

            var localToWorld = Matrix4x4.TRS(ZibraLiquidInstance.transform.position,
                                             ZibraLiquidInstance.transform.rotation, Vector3.one);

            //ZibraLiquidInstance.containerPos = ZibraLiquidInstance.transform.position;
            //ZibraLiquidInstance.transform.rotation = Quaternion.identity;
            //ZibraLiquidInstance.transform.localScale = Vector3.one;

            using (new Handles.DrawingScope(containerColor, localToWorld))
            {
                if (editMode == EditMode.Container)
                {
                    ZibraLiquidInstance.useContainerReference = false;

                    Handles.Label(Vector3.zero, "Container Area", containerText);

                    boxBoundsHandleContainer.center = Vector3.zero;
                    boxBoundsHandleContainer.size = ZibraLiquidInstance.containerSize;

                    EditorGUI.BeginChangeCheck();
                    var newPos = Handles.PositionHandle(Vector3.zero, Quaternion.identity);
                    boxBoundsHandleContainer.DrawHandle();
                    if (EditorGUI.EndChangeCheck())
                    {
                        // record the target object before setting new values so changes can be undone/redone
                        Undo.RecordObject(ZibraLiquidInstance, "Change Container");

                        var startPos = ZibraLiquidInstance.containerPos;
                        ZibraLiquidInstance.containerPos = newPos;
                        ZibraLiquidInstance.containerPos += boxBoundsHandleContainer.center - startPos;
                        ZibraLiquidInstance.containerSize = boxBoundsHandleContainer.size;
                        EditorUtility.SetDirty(ZibraLiquidInstance);
                    }
                }
                else
                {
                    Handles.DrawWireCube(Vector3.zero, ZibraLiquidInstance.containerSize);
                }
            }
        }

        public override void OnInspectorGUI()
        {
            if (ZibraLiquidInstance == null)
            {
                Debug.LogError("ZibraLiquidEditor not attached to ZibraLiquid component.");
                return;
            }

            //ZibraLiquidInstance.transform.position = ZibraLiquidInstance.containerPos;
            //ZibraLiquidInstance.transform.rotation = Quaternion.identity;
            //ZibraLiquidInstance.transform.localScale = Vector3.one;

            serializedObject.Update();

          /*   EditorGUILayout.BeginHorizontal();
            GUILayout.Space(20);

             if (GUILayout.Button(EditorGUIUtility.IconContent("EditCollider"), GUILayout.MaxWidth(40),
                                 GUILayout.Height(30)))
            {
                editMode = editMode == EditMode.Container ? EditMode.None : EditMode.Container;
                SceneView.RepaintAll();
            }  */

            /* GUILayout.Space(10);
             EditorGUILayout.LabelField("Edit Container Area", containerText, GUILayout.MaxWidth(100),
                                       GUILayout.Height(30));
            GUILayout.Space(40);
 */
          /*   EditorGUI.BeginChangeCheck();
            ZibraLiquidInstance.useContainerReference = GUILayout.Toggle(
                ZibraLiquidInstance.useContainerReference, "Use Reference Mesh Renderer", GUILayout.Height(30));

            if (EditorGUI.EndChangeCheck() && ZibraLiquidInstance.useContainerReference)
            {
                editMode = EditMode.None;

                SceneView.RepaintAll();
            }  */

    /*         if (ZibraLiquidInstance.useContainerReference)
            {
                if (ZibraLiquidInstance.ContainerReference != null)
                {
                    ZibraLiquidInstance.containerPos = ZibraLiquidInstance.ContainerReference.bounds.center;
                    ZibraLiquidInstance.containerSize = ZibraLiquidInstance.ContainerReference.bounds.size;

                    SceneView.RepaintAll();
                }
            }  */

         //   EditorGUILayout.EndHorizontal();

            //if (ZibraLiquidInstance.useContainerReference)
/*             {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(ContainerReference);

                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            }
            else */
            //{
                EditorGUI.BeginChangeCheck();
                ZibraLiquidInstance.containerPos = ZibraLiquidInstance.transform.position;
                ZibraLiquidInstance.containerSize = ZibraLiquidInstance.transform.localScale;
                    //EditorGUILayout.Vector3Field(new GUIContent("Container Center"), ZibraLiquidInstance.containerPos);
           //     ZibraLiquidInstance.containerSize =
           //         EditorGUILayout.Vector3Field(new GUIContent("Container Size"), ZibraLiquidInstance.containerSize);

                if (EditorGUI.EndChangeCheck())
                {
                    SceneView.RepaintAll();
                }
            //}

            GUILayout.Space(25);

            EditorGUILayout.PropertyField(sdfColliders, true);

            if (!ZibraLiquidBridge.IsPaidVersion() && ZibraLiquidInstance.sdfColliders.Count > 5)
            {
                Debug.LogWarning(
                    "Too many SDF colliders for free version of Zibra Liquids, some colliders will be disabled. Free version limited to 5 SDF colliders.");
                ZibraLiquidInstance.sdfColliders.RemoveRange(5, ZibraLiquidInstance.sdfColliders.Count - 5);
            }

            GUIContent btnTxt = new GUIContent("Add Collider");
            var rtBtn = GUILayoutUtility.GetRect(btnTxt, GUI.skin.button, GUILayout.MaxWidth(400));
            rtBtn.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rtBtn.center.y);

            if (EditorGUI.DropdownButton(rtBtn, btnTxt, FocusType.Keyboard))
            {
                colliderDropdownToggle = !colliderDropdownToggle;
            }

            if (ZibraLiquidInstance.sdfColliders != null)
                ZibraLiquidInstance.sdfColliders.RemoveAll(item => item == null);

            if (colliderDropdownToggle && allSDFColliders.isArray)
            {
                EditorGUI.indentLevel++;

                var empty = true;

                for (var i = 0; i < allSDFColliders.arraySize; i++)
                {
                    var serializedProperty = allSDFColliders.GetArrayElementAtIndex(i);
                    var sdfCollider = serializedProperty.objectReferenceValue as SDFCollider;

                    if ((ZibraLiquidInstance.sdfColliders == null) ||
                        sdfCollider != null && ZibraLiquidInstance.sdfColliders.Contains(sdfCollider))
                    {
                        continue;
                    }

                    empty = false;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedProperty, true);
                    EditorGUI.EndDisabledGroup();

#if ZIBRA_LIQUID_FREE_VERSION
                    if (ZibraLiquidInstance.sdfColliders.Count < 5)
#endif
                    {
                        if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
                        {
                            ZibraLiquidInstance.sdfColliders.Add(sdfCollider);
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if (empty)
                {

                    GUIContent labelText = new GUIContent("The list is empty.");
                    var rtLabel = GUILayoutUtility.GetRect(labelText, GUI.skin.label, GUILayout.ExpandWidth(false));
                    rtLabel.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rtLabel.center.y);

                    EditorGUI.LabelField(rtLabel, labelText);
                }

                EditorGUI.indentLevel--;
            }

            GUILayout.Space(25);

            EditorGUILayout.PropertyField(manipulators, true);

            if (ZibraLiquidInstance.manipulators != null)
                ZibraLiquidInstance.manipulators.RemoveAll(item => item == null);

            GUIContent manipBtnTxt = new GUIContent("Add Manipulator");
            var manipBtn = GUILayoutUtility.GetRect(manipBtnTxt, GUI.skin.button, GUILayout.MaxWidth(400));
            manipBtn.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, manipBtn.center.y);

            if (EditorGUI.DropdownButton(manipBtn, manipBtnTxt, FocusType.Keyboard))
            {
                manipulatorDropdownToggle = !manipulatorDropdownToggle;
            }

            if (manipulatorDropdownToggle && allManipulators.isArray)
            {
                var empty = true;
                for (var i = 0; i < allManipulators.arraySize; i++)
                {
                    var serializedProperty = allManipulators.GetArrayElementAtIndex(i);
                    var manipulator = serializedProperty.objectReferenceValue as Manipulator;

                    if ((ZibraLiquidInstance.manipulators == null) ||
                        manipulator != null && ZibraLiquidInstance.manipulators.Contains(manipulator))
                    {
                        continue;
                    }

                    empty = false;

                    EditorGUILayout.BeginHorizontal();

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.PropertyField(serializedProperty, true);
                    EditorGUI.EndDisabledGroup();

                    if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
                    {
                        ZibraLiquidInstance.manipulators.Add(manipulator);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel++;

                if (empty)
                {

                    GUIContent labelText = new GUIContent("The list is empty.");
                    var rtLabel = GUILayoutUtility.GetRect(labelText, GUI.skin.label, GUILayout.ExpandWidth(false));
                    rtLabel.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, rtLabel.center.y);

                    EditorGUI.LabelField(rtLabel, labelText);
                }

                EditorGUI.indentLevel--;
            }

            if (ZibraLiquidInstance.Activated)
            {
                GUIContent statsButtonText = new GUIContent("Simulation statistics");
                var statsButton = GUILayoutUtility.GetRect(statsButtonText, GUI.skin.button, GUILayout.MaxWidth(400));
                statsButton.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, statsButton.center.y);

                if (EditorGUI.DropdownButton(statsButton, statsButtonText, FocusType.Keyboard))
                {
                    statsDropdownToggle = !statsDropdownToggle;
                }
                if (statsDropdownToggle)
                {
                    GUILayout.Label("Current time step: " + ZibraLiquidInstance.timestep);
                    GUILayout.Label("Internal time: " + ZibraLiquidInstance.simulationInternalTime);
                    GUILayout.Label("Simulation frame: " + ZibraLiquidInstance.simulationInternalFrame);
                    GUILayout.Label("Active particles: " + ZibraLiquidInstance.activeParticleNumber + " / " +
                                    ZibraLiquidInstance.MaxNumParticles);
                }
                GUILayout.Space(10);
            }

            GUIContent footprintButtonText = new GUIContent("Approximate VRAM footprint");
            var footprintButton =
                GUILayoutUtility.GetRect(footprintButtonText, GUI.skin.button, GUILayout.MaxWidth(400));
            footprintButton.center = new Vector2(EditorGUIUtility.currentViewWidth / 2, footprintButton.center.y);

            if (EditorGUI.DropdownButton(footprintButton, footprintButtonText, FocusType.Keyboard))
            {
                footprintDropdownToggle = !footprintDropdownToggle;
            }
            if (footprintDropdownToggle)
            {
                int totalParticleFootprint = ZibraLiquidInstance.GetParticleCountFootprint();

                GUILayout.Label($"Particle count footprint: {(float)totalParticleFootprint / (1 << 20):N2}MB");

                int totalCollidersFootprint = ZibraLiquidInstance.GetCollidersFootprint();

                GUILayout.Label($"Colliders footprint: {(float)totalCollidersFootprint / (1 << 20):N2}MB");

                int totalDridFootprint = ZibraLiquidInstance.GetGridFootprint();

                GUILayout.Label($"Grid size footprint: {(float)totalDridFootprint / (1 << 20):N2}MB");
            }
            GUILayout.Space(10);

            EditorGUILayout.PropertyField(TimeStepMax);
            EditorGUILayout.PropertyField(SimTimePerSec);

            if (!ZibraLiquidInstance.Activated)
                EditorGUILayout.PropertyField(MaxParticleNumber);

            EditorGUILayout.PropertyField(IterationsPerFrame);

            if (!ZibraLiquidInstance.Activated)
                EditorGUILayout.PropertyField(GridResolution);
            var solverRes = Vector3Int.CeilToInt(ZibraLiquidInstance.containerSize / ZibraLiquidInstance.cellSize);
            GUILayout.Label("Effective grid resolution:   " + solverRes);
            ZibraLiquidInstance.cellSize =
                Math.Max(ZibraLiquidInstance.containerSize.x,
                         Math.Max(ZibraLiquidInstance.containerSize.y, ZibraLiquidInstance.containerSize.z)) /
                ZibraLiquidInstance.gridResolution;
            EditorGUILayout.PropertyField(RunSimulation);

#if UNITY_PIPELINE_HDRP
            EditorGUILayout.PropertyField(customLight);
#endif
            EditorGUILayout.PropertyField(reflectionProbe);
            serializedObject.ApplyModifiedProperties();
        }
    }
}