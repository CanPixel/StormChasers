namespace EasySnap
{

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using EasySnap;


    [CustomEditor(typeof(Transform))]
    public class Snap : DecoratorEditor
    {
        public Snap() : base("TransformInspector") { }


        private static GameObject selectedSnapObject;          // The object which we want to snap
        private static GameObject targetSnapObject;            // The target object to which we will snap
        private static Vector3 mousePosition;                  // The current mousePosition in the scene view
        private static RaycastHit hitInfo;                     // Collision information about the ray
        private static Ray ray;                                // The ray to be casted to select the targetobject in the scene
        private static Vector3 targetSnapVertex;               // The vertex of the target object to which we will align/snap the vertex of the selected Object
        private static Vector3 selectionSnapVertex;            // The vertex of the selected object which will be aligned/snapped to the vertex of the target Object
        private static float distBTvertices;                   // The distance between the targetSnapVertex and selectionSnapVertex
        private static int controlId;                          // The control ID used for identifying the free move Handle
                                                               //private static Camera cam;
        private static GameObject prevTarget;
        private static Color32 prevColor;
        private static Material prevMaterial;
        private static bool flag;
        private static Vector3 screenPoint;
        private static Vector3 offset;
        private static Transform prevObj;
        private static bool feasibleTarget;
        private static Vector3 handlePosition;
        private static bool objSelectionChanged;
        private static bool moveToolSelected;
        private static bool skipFrame;
        //private static int frameNumber =  1;
        private static bool executeInNextFrame;
        private static bool snapNotInScene;
        private static bool isVersionOk;
        public static bool pressedOnce;
        private static bool greenSel;
        private static int origLayer;
        private static bool wasFreeMoveSelected;
        public static bool wasMarkerPointed;
        public static bool wasDown;
        public static bool flagger;
        public static bool mouseUp;
        private bool enableSnap;

        void OnEnable()
        {
            
            // Remove delegate listener if it has previously been assigned.

            Selection.selectionChanged -= SelectionChanged;
            Selection.selectionChanged += SelectionChanged;

            string version = Application.unityVersion;

            isVersionOk = version.Contains("2017.1") || version.Contains("2017.2") ? false : true;
            if (version.Contains("2015")) { isVersionOk = false; }

        }


        void OnDisable()
        {
            
            Selection.selectionChanged -= SelectionChanged;
        }




        void OnSceneGUI()
        {
            
            if (SnapInfo.snap == null)
            {
                return;
            }

            if (Selection.gameObjects == null || Selection.gameObjects.Length > 1)
            {
                return;
            }

            Event e = Event.current;


            if (e == null) { return; }
            

            switch (e.type)
            {

                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == (KeyCode.S))
                        {
                            enableSnap = true;
                        }

                        else
                        {
                            //return;
                        }
                        break;
                    }

                case EventType.KeyUp:
                    {
                        if (Event.current.keyCode == (KeyCode.S))
                        {
                            enableSnap = false;
                        }

                        break;
                    }

            }


            if (Selection.activeGameObject && Selection.activeGameObject.GetHashCode().Equals(SnapInfo.selectionVertexMarker.gameObject.GetHashCode()))
            {

            }

            else if (Selection.activeGameObject && Selection.activeGameObject.GetHashCode().Equals(SnapInfo.targetVertexMarker.gameObject.GetHashCode()))
            {

            }

            else if (!enableSnap)
            {
                selectedSnapObject = null;
                targetSnapObject = null;
                Tools.hidden = false;

                SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                SnapInfo.selectionVertexMarker.gameObject.SetActive(false);

                return;
            }


            if (wasDown && Event.current.clickCount == 0)
            {
                wasDown = false;
                flagger = true;
            }

            if (flagger && Event.current.clickCount > 0) { mouseUp = true; flagger = false; }

            if (Event.current.clickCount > 0 && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                wasDown = true;
            }



            if (Selection.activeGameObject == null)
            {
                SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
            }

            else
            {
                SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = true;
            }



            if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, ~RayCastMasks.ignoreMask))
            {
                bool prevState = greenSel;

                if (mouseUp && hitInfo.transform.name != SnapInfo.targetVertexMarker.name && Tools.viewTool != ViewTool.Orbit && !(Tools.current == Tool.View && Tools.viewTool == ViewTool.Pan))
                {
                    greenSel = false;
                }
            }

            else if (mouseUp && Tools.viewTool != ViewTool.Orbit && !(Tools.current == Tool.View && Tools.viewTool == ViewTool.Pan))
            {
                greenSel = false;
            }

            if (greenSel) { Selection.activeGameObject = SnapInfo.selectionVertexMarker; }   // This


            if (HandleControlsUtility.instance == null) { HandleControlsUtility.instance = new HandleControlsUtility(); }

            GameObject[] snaps = GameObject.FindGameObjectsWithTag("EditorOnly");

            if (snaps == null)
            {
                mouseUp = false;
                return;
            }

            foreach (GameObject gameObject in snaps)
            {
                if (gameObject.GetComponent<SnapRecognize>()) { snapNotInScene = false; break; }
                else { snapNotInScene = true; }
            }

            if (snapNotInScene)
            {
                mouseUp = false;
                return;
            }


            if (!SnapInfo.snapActive)
            {
                prevObj = null;
                mouseUp = false;
                return;
            }

            float s = HandleUtility.GetHandleSize(SnapInfo.selectionVertexMarker.transform.position) / 2.3f;
            SnapInfo.selectionVertexMarker.transform.localScale = new Vector3(s, s, s);

            s = HandleUtility.GetHandleSize(SnapInfo.targetVertexMarker.transform.position) / 2.3f;
            SnapInfo.targetVertexMarker.transform.localScale = new Vector3(s, s, s);


            if (Selection.activeTransform == null && SnapInfo.isVertexClickSnapping)
            {
                greenSel = false;
                SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                SnapInfo.selectionVertexMarker.gameObject.SetActive(false);

                //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;  
            }


            if (Event.current.type == EventType.MouseDown && SnapInfo.isVertexClickSnapping)
            {
                ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);



                if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, ~RayCastMasks.ignoreMask))
                {
                    if (hitInfo.transform.GetComponent<SnapRecognize>())
                    {
                        //Selection.activeTransform = hitInfo.transform;

                        if (hitInfo.transform.name == SnapInfo.selectionVertexMarker.name)
                        {
                            greenSel = true;
                        }

                        else if (greenSel && hitInfo.transform.name == SnapInfo.targetVertexMarker.name && Selection.activeTransform.name == SnapInfo.targetVertexMarker.name)
                        {
                            greenSel = false;
                        }

                        //if (mouseUp) { Selection.activeGameObject = SnapInfo.selectionVertexMarker.transform.parent.gameObject; }

                    }
                }


                else if (Selection.activeTransform != null && !Selection.activeTransform.GetComponent<SnapRecognize>())
                {
                    greenSel = false;
                }


            }




            Transform activeTransform = Selection.activeTransform;

            if (activeTransform != null && activeTransform.GetComponent<MeshRenderer>() && (Tools.current == Tool.Move || Tools.current == Tool.Rotate) && (activeTransform.GetComponent<SnapRecognize>() == null))
            {



                mousePosition = Event.current.mousePosition;
                mousePosition = HandleUtility.GUIPointToWorldRay(mousePosition).origin;

                selectedSnapObject = Selection.activeGameObject;

                ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);




                //Debug.Log("Hot Control ID  " + hotControlID + "  Position Handle StartID  " + positionHandleStartID);


                if (SnapInfo.isFreeMoveHandle)
                {
                    moveToolSelected = (GUIUtility.hotControl == controlId) ? true : false;
                }

                else
                {
                    if (!isVersionOk)
                    {
                        int hotControlID = GUIUtility.hotControl;
                        int positionHandleStartID = GetPositionHandleStartID();
                        int positionHandleEndID = positionHandleStartID + 5;

                        moveToolSelected = (hotControlID >= positionHandleStartID && hotControlID <= positionHandleEndID) ? true : false;
                    }
                    else { moveToolSelected = (HandleControlsUtility.instance.GetHandleType(HandleControlsUtility.instance.GetCurrentSelectedControl()) == HandleControlsUtility.HandleType.position); }
                }



                if (SnapInfo.isMarkerHidden)
                {
                    SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                    SnapInfo.selectionVertexMarker.gameObject.SetActive(false);

                    //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                    //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    SnapInfo.targetVertexMarker.gameObject.SetActive(true);
                    SnapInfo.selectionVertexMarker.gameObject.SetActive(true);

                    //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                    //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                }




                var eventType = Event.current.type;

                //cam = SceneView.lastActiveSceneView.camera;
                mousePosition = Event.current.mousePosition;
                mousePosition = HandleUtility.GUIPointToWorldRay(mousePosition).origin;

                selectedSnapObject = Selection.activeGameObject;

                ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                //Handles.DrawLine(mousePosition, ray.direction * 20f);


                if (SnapInfo.handles2Pointer && !Selection.activeTransform.tag.Equals("EditorOnly") && (Tools.current == Tool.Move))
                {

                    if (prevObj == null)
                    {

                        if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, RayCastMasks.ignoreMask))
                        {

                            Tools.hidden = true;
                            Vector3 line = hitInfo.point - Selection.activeTransform.position;
                            handlePosition = (line * 0.1f) + hitInfo.point;
                            prevObj = hitInfo.transform;
                            feasibleTarget = true;
                        }

                        else
                        {
                            Tools.hidden = false;
                            prevObj = Selection.activeTransform;
                            feasibleTarget = false;
                        }


                    }

                    else if (objSelectionChanged)
                    {

                        Tools.hidden = true;
                        Vector3 line = hitInfo.point - Selection.activeTransform.position;
                        handlePosition = (line * 0.1f) + hitInfo.point;
                        prevObj = hitInfo.transform;
                    }


                    else

                    {



                        if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo))
                        {

                            //if (moveToolSelected) { Debug.Log("Move Tool Selected  frame Number " + frameNumber); }
                            //else { Debug.Log("Move Tool notSleected  frame Number " + frameNumber); }
                            if (hitInfo.transform.GetInstanceID() == Selection.activeTransform.GetInstanceID())
                            {


                                if (executeInNextFrame)
                                {

                                    executeInNextFrame = false;

                                    if (!moveToolSelected)
                                    {
                                        //Debug.Log("You can change handle pos now!  frameNumber  " + frameNumber);

                                        Tools.hidden = true;
                                        Vector3 line = hitInfo.point - Selection.activeTransform.position;
                                        handlePosition = (line * 0.1f) + hitInfo.point;

                                    }
                                }


                                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && !Event.current.isKey)
                                {
                                    executeInNextFrame = true;
                                }

                                if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && !Event.current.isKey)
                                {
                                    //Debug.Log("MouseUp frameNumber  " + frameNumber);
                                    executeInNextFrame = false;
                                }

                            }

                        }

                    }




                    if (feasibleTarget)
                    {


                        Tools.hidden = false;
                        float size = HandleUtility.GetHandleSize(Selection.activeTransform.position) * SnapInfo.freeHandleSize;
                        Vector3 snap = Vector3.one * 0.5f;
                        Tools.hidden = true;
                        Vector3 currentHandlePos = Vector3.zero;


                        if (SnapInfo.isFreeMoveHandle)
                        {
                            controlId = GUIUtility.GetControlID(FocusType.Passive);

                            if (!SnapInfo.isVertexClickSnapping)
                            {
                                currentHandlePos = Handles.FreeMoveHandle(controlId, handlePosition, Quaternion.identity, size, snap, Handles.CircleHandleCap);
                            }
                        }


                        else if (!SnapInfo.isVertexClickSnapping)
                        {
                            currentHandlePos = Handles.PositionHandle(handlePosition, Quaternion.identity);
                        }


                        if (currentHandlePos != handlePosition && !SnapInfo.isVertexClickSnapping)
                        {
                            // Handle has been moved

                            Vector3 change = currentHandlePos - handlePosition;
                            Selection.activeTransform.position += change;
                            handlePosition = currentHandlePos;


                            if (SnapInfo.isFreeMoveHandle)
                            {
                                Handles.FreeMoveHandle(controlId, handlePosition, Quaternion.identity, size, snap, Handles.CircleHandleCap);
                            }

                            else { /*Handles.PositionHandle(handlePosition, Quaternion.identity);*/ }

                        }

                    }


                }


                else
                {

                    Tools.hidden = false;
                    Vector3 handleCenterPos = Tools.handlePosition;
                    Vector3 currentHandlePos;
                    Tools.hidden = true;

                    if (SnapInfo.isFreeMoveHandle && !Selection.activeTransform.tag.Equals("EditorOnly") && (Tools.current == Tool.Move))
                    {

                        float size = HandleUtility.GetHandleSize(Selection.activeTransform.position) * SnapInfo.freeHandleSize;
                        Vector3 snap = Vector3.one * 0.5f;

                        currentHandlePos = Vector3.zero;

                        if (!SnapInfo.isVertexClickSnapping)
                        {
                            currentHandlePos = Handles.FreeMoveHandle(handleCenterPos, Quaternion.identity, size, snap, Handles.CircleHandleCap);
                        }

                        if (GUI.changed && !SnapInfo.isVertexClickSnapping)
                        {
                            Vector3 change = currentHandlePos - handleCenterPos;
                            Selection.activeTransform.position += change;
                            handleCenterPos = currentHandlePos;
                            Handles.FreeMoveHandle(controlId, handlePosition, Quaternion.identity, size, snap, Handles.CircleHandleCap);

                        }
                    }

                    else
                    {
                        Tools.hidden = false;
                        prevObj = null;
                    }
                }


                if (SnapInfo.isFreeMoveHandle)
                {
                    moveToolSelected = (GUIUtility.hotControl == controlId) ? true : false;
                    if (GUIUtility.hotControl <= 0) { moveToolSelected = false; }
                }



                if (Tools.current == Tool.Move && !moveToolSelected)
                {

                    //  Scope executes
                    SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                    if (!SnapInfo.isVertexClickSnapping) { SnapInfo.selectionVertexMarker.gameObject.SetActive(false); }

                    //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                    //if(!SnapInfo.isVertexClickSnapping) { SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false; }


                    if (prevTarget && prevTarget.GetComponent<MeshRenderer>().sharedMaterial != null)
                    {
                        prevTarget.GetComponent<MeshRenderer>().sharedMaterial = prevMaterial;
                        prevTarget = null;
                        prevMaterial = null;
                        flag = true;
                    }




                    if (SnapInfo.isVertexClickSnapping && Selection.activeTransform != null && (Selection.activeTransform.GetComponent<MeshFilter>() && Selection.activeTransform.GetComponent<MeshFilter>().sharedMesh))
                    {


                        if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, RayCastMasks.ignoreMask))
                        {
                            if (!hitInfo.transform.gameObject.tag.Equals("EditorOnly"))
                            {
                                SnapInfo.targetVertexMarker.gameObject.SetActive(true);
                                SnapInfo.selectionVertexMarker.gameObject.SetActive(true);

                                //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                                //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = true;


                                //targetSnapVertex = SelectTargetSnapVertex(targetSnapObject, hitInfo.point, true);
                                selectionSnapVertex = SelectSelectionSnapVertex(Selection.activeTransform.gameObject, hitInfo.point);

                                SnapInfo.selectionVertexMarker.transform.position = selectionSnapVertex;
                                selectedSnapObject = Selection.activeTransform.gameObject;

                                if (targetSnapObject != null && selectedSnapObject != null)
                                {
                                    //SnapInfo.selectionVertexMarker.transform.position = selectionSnapVertex;
                                    SnapInfo.targetVertexMarker.transform.position = targetSnapVertex;
                                }

                            }


                            else
                            {
                                SnapInfo.targetVertexMarker.gameObject.SetActive(false);

                                //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                                //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                            }


                        }

                        else
                        {
                            SnapInfo.targetVertexMarker.gameObject.SetActive(false);

                            //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                            //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                        }


                    }



                    if (greenSel)
                    {
                        SnapInfo.targetVertexMarker.gameObject.SetActive(true);
                        //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                    }
                    else
                    {
                        SnapInfo.targetVertexMarker.gameObject.SetActive(false);

                        //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                    }


                    if (SnapInfo.isVertexClickSnapping) { Tools.hidden = true; }


                    if (SnapInfo.isFreeMoveHandle && !(wasFreeMoveSelected && !moveToolSelected))
                    {
                        wasFreeMoveSelected = moveToolSelected;
                        mouseUp = false;
                        return;
                    }

                    else if (!SnapInfo.isFreeMoveHandle)
                    {
                        mouseUp = false;
                        return;
                    }
                }

                //bool flug = (Event.current.type == EventType.MouseDown && moveToolSelected);

                if (selectedSnapObject != null) { selectedSnapObject.layer = RayCastMasks.ignoreLayer; }

                if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, RayCastMasks.ignoreMask))
                {

                    if (hitInfo.collider && selectedSnapObject != null && hitInfo.transform.GetComponent<MeshRenderer>())
                    {
                        targetSnapObject = hitInfo.transform.gameObject;

                        if (SnapInfo.targetTransparency && (prevTarget == null))
                        {
                            //this cause
                            //Debug.Log("Transparrenting");
                            if (targetSnapObject.GetComponent<MeshRenderer>().sharedMaterial != null)
                            {
                                prevMaterial = new Material(targetSnapObject.GetComponent<MeshRenderer>().sharedMaterial);
                                Color32 col = new Color32();
                                bool error = false;



                                if (prevMaterial.HasProperty("_Color") || prevMaterial.HasProperty("_MainTex"))
                                {
                                    col = prevMaterial.color;
                                }
                                else
                                {
                                    error = true;
                                }

                                if (!error)
                                {
                                    col.a = SnapInfo.transparencyThreshold;
                                    SnapInfo.snap.GetComponent<SnapInfo>().defaultMat.color = col;
                                    targetSnapObject.GetComponent<MeshRenderer>().sharedMaterial = SnapInfo.snap.GetComponent<SnapInfo>().defaultMat;
                                }

                            }


                            flag = false;

                        }


                        else if (SnapInfo.targetTransparency && (prevTarget.GetInstanceID() != targetSnapObject.GetInstanceID()))
                        {
                            if (prevMaterial && prevTarget.GetComponent<MeshRenderer>().sharedMaterial != null)
                            {
                                prevTarget.GetComponent<MeshRenderer>().sharedMaterial = prevMaterial;
                                prevTarget = null;
                                prevMaterial = null;
                                flag = true;
                            }
                        }


                        if (!flag) { prevTarget = targetSnapObject; }


                    }



                }



                else
                {

                    if (prevTarget != null && SnapInfo.targetTransparency && prevTarget.GetComponent<MeshRenderer>() != null)
                    {
                        prevTarget.GetComponent<MeshRenderer>().sharedMaterial = prevMaterial;
                        prevMaterial = null;
                    }

                    targetSnapObject = null;
                    prevTarget = null;

                }


                if (selectedSnapObject != null && !selectedSnapObject.tag.Equals("EditorOnly")) { selectedSnapObject.layer = RayCastMasks.defaultLayer; }


                if (selectedSnapObject != null && targetSnapObject != null)
                {
                    if (SnapInfo.isVertexSnap)
                    {
                        targetSnapVertex = SelectTargetSnapVertex(targetSnapObject, hitInfo.point, true);
                        selectionSnapVertex = SelectSelectionSnapVertex(selectedSnapObject, targetSnapVertex);
                    }

                    else if (SnapInfo.isFaceSnap)
                    {
                        targetSnapVertex = hitInfo.point; // Tri index might help
                        selectionSnapVertex = SelectSelectionSnapVertex(selectedSnapObject, targetSnapVertex);
                    }




                    if (targetSnapObject != null && selectedSnapObject != null)
                    {
                        SnapInfo.selectionVertexMarker.transform.position = selectionSnapVertex;
                        SnapInfo.targetVertexMarker.transform.position = targetSnapVertex;
                    }


                    distBTvertices = Vector3.Distance(targetSnapVertex, selectionSnapVertex);

                    if (distBTvertices <= SnapInfo.minDistToSnap)
                    {

                        if (eventType == EventType.MouseUp && Event.current.button == 0 && !Event.current.isKey)
                        {
                            AlignObjects(selectedSnapObject, targetSnapObject, selectionSnapVertex, targetSnapVertex);
                        }

                    }


                }

                objSelectionChanged = false;

            }


            else
            {

                Tools.hidden = false;

                if (prevTarget != null && SnapInfo.targetTransparency && prevTarget.GetComponent<MeshRenderer>() != null)
                {
                    prevTarget.GetComponent<MeshRenderer>().sharedMaterial = prevMaterial;
                    prevMaterial = null;
                }

                targetSnapObject = null;
                prevTarget = null;
                prevObj = null;
                objSelectionChanged = true;
            }


            if (SnapInfo.isVertexClickSnapping)
            {
                if (greenSel)
                {
                    SnapInfo.targetVertexMarker.gameObject.SetActive(true);

                    //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                }
                else
                {
                    SnapInfo.targetVertexMarker.gameObject.SetActive(false);

                    //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                }
            }



            origLayer = 0;

            if (selectedSnapObject != null) { origLayer = selectedSnapObject.layer; selectedSnapObject.layer = RayCastMasks.ignoreLayer; }



            if (Selection.activeTransform && Selection.activeTransform.GetComponent<SnapRecognize>() && SnapInfo.isVertexClickSnapping)
            {

                Tools.hidden = true;

                if (greenSel)
                {

                    Debug.DrawLine(selectionSnapVertex, targetSnapVertex, Color.black);


                    if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, RayCastMasks.ignoreMask) && selectedSnapObject != null)
                    {
                        if (!hitInfo.transform.GetHashCode().Equals(selectedSnapObject.transform.GetHashCode()))
                        {

                            if (!hitInfo.transform.gameObject.tag.Equals("EditorOnly") && hitInfo.transform.GetComponent<MeshFilter>() && hitInfo.transform.GetComponent<MeshFilter>().sharedMesh)
                            {

                                SnapInfo.targetVertexMarker.gameObject.SetActive(true);
                                SnapInfo.selectionVertexMarker.gameObject.SetActive(true);

                                //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = true;
                                //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = true;

                                targetSnapObject = hitInfo.transform.gameObject;

                                if (SnapInfo.isVertexSnap)
                                {
                                    targetSnapVertex = SelectSelectionSnapVertex(targetSnapObject, hitInfo.point);
                                }

                                else
                                {
                                    targetSnapVertex = hitInfo.point;
                                }

                                SnapInfo.targetVertexMarker.transform.position = targetSnapVertex;
                            }

                        }


                        else
                        {
                            SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                            //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                            //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                        }

                    }

                    else
                    {
                        //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                        //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                    }


                }

                else
                {
                }


                Tools.hidden = true;

                ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                if (Physics.Linecast(ray.origin, ray.direction * 1000000f, out hitInfo, ~RayCastMasks.ignoreMask))
                {
                    if (hitInfo.transform.GetComponent<SnapRecognize>())
                    {
                        HandleUtility.AddDefaultControl(0);

                        if (hitInfo.transform.name == "m1arke_rS1el.." && Selection.activeTransform.name == "m1arke_rS1el..")
                        {
                            greenSel = true;
                        }


                        else if (greenSel && hitInfo.transform.name == "m1arker_T1ar.." && Event.current.type == EventType.MouseUp)
                        {
                            greenSel = false;

                            AlignObjects(selectedSnapObject, targetSnapObject, selectionSnapVertex, targetSnapVertex);

                            SnapInfo.targetVertexMarker.gameObject.SetActive(false);
                            SnapInfo.selectionVertexMarker.gameObject.SetActive(false);
                            //SnapInfo.targetVertexMarker.GetComponent<MeshRenderer>().enabled = false;
                            //SnapInfo.selectionVertexMarker.GetComponent<MeshRenderer>().enabled = false; 
                        }
                    }
                }
            }

            else
            {
                greenSel = false;
            }


            if (selectedSnapObject != null) { selectedSnapObject.layer = origLayer; }

            wasFreeMoveSelected = moveToolSelected;

            mouseUp = false;
        }


        private static Vector3 ToggleToolHandle(bool hide)
        {
            //Tools.hidden = hide;
            //return (Handles.PositionHandle(Tools.handlePosition, Quaternion.identity));
            return Vector3.zero;
        }




        private static Vector3 SelectTargetSnapVertex(GameObject target, Vector3 point, bool isTarget, Vector3? exclude = null)
        {


            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            Vector3 targetVertex = Vector3.zero;



            bool isExclude = (exclude == null) ? false : true;

            if (isExclude) { exclude = target.transform.InverseTransformPoint((Vector3)exclude); }

            if (meshFilter != null)
            {

                float minDist = Mathf.Infinity;
                point = target.transform.InverseTransformPoint(point);
                foreach (Vector3 vertex in meshFilter.sharedMesh.vertices)
                {
                    Vector3 vert = vertex;

                    if (isExclude && vert.Equals(exclude)) { continue; }

                    float dist = Vector3.Distance(point, vert);
                    if (dist < minDist) { minDist = dist; targetVertex = vert; }
                }

            }

            return target.transform.TransformPoint(targetVertex);

        }





        private static Vector3 SelectSelectionSnapVertex(GameObject target, Vector3 point)
        {
            return (SelectTargetSnapVertex(target, point, false));
        }





        private static GameObject GetSnapTarget(Vector3 mousePosition)
        {

            GameObject[] allSceneObjects = SceneView.FindObjectsOfType<GameObject>();
            GameObject target = null;
            float minDist = Mathf.Infinity;

            foreach (GameObject gObject in allSceneObjects)
            {

                Vector3 unifiedPos = gObject.transform.position;
                float dist = Vector3.Distance(mousePosition, unifiedPos);
                if (dist < minDist) { minDist = dist; target = gObject; }

            }

            return target;

        }



        private static void AlignObjects(GameObject selectedSnapObject, GameObject targetSnapObject, Vector3 selectionSnapVertex, Vector3 targetSnapVertex)

        {

            if (SnapInfo.snapAtRotationTool || Tools.current == Tool.Move)
            {

                //Debug.Log("Snapped");
                Vector3 positionBeforeSnap = selectedSnapObject.transform.position;
                Vector3 offset = targetSnapVertex - selectionSnapVertex;
                Vector3 newCenter = selectedSnapObject.transform.position + offset;

                Undo.RecordObject(selectedSnapObject.transform, "undosnap");
                selectedSnapObject.transform.position = newCenter;

                Vector3 change = newCenter - positionBeforeSnap;
                handlePosition += change;
            }

            if (Tools.current == Tool.Move && !SnapInfo.isVertexClickSnapping)
            {
                Handles.PositionHandle(handlePosition, Quaternion.identity);
            }

        }



        private static void SelectionChanged()
        {
            objSelectionChanged = true;
        }


        private static int GetPositionHandleStartID()
        {
            int hashCode = "xAxisFreeMoveHandleHash".GetHashCode();
            return (GUIUtility.GetControlID(hashCode, FocusType.Passive) + 1);
        }

        static public bool HasProperty(Type type, string name)
        {
            return (type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Any(p => p.Name == name));
        }

        static public bool HasField(Type type, string name)
        {
            return (type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Any(p => p.Name == name));
        }



    }

#endif
}



