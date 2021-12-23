
namespace EasySnap
{

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SnapInfo : MonoBehaviour
{
    [Tooltip("Adjust the transparency amount.")]
    [Range(0,255)]
    public byte transparencyAmount;                     // Transparency value for the selected snap target 
    internal static byte transparencyThreshold;

    [Tooltip("Should the snap target object be made transparent?")]
    public bool makeTargetTransparent = true;           // Should the snap target object be made transparent?
    internal static bool targetTransparency;

    [Tooltip("Should the move tool be placed near the mouse pointer when clicked on an object?")]
    public bool handlesToPointer = true;                // Should the move tool be placed near the mouse pointer when clicked on an object?
    internal static bool handles2Pointer;

    [Tooltip("Replace the arrow handles with a circular free move handle (Gives freedom of directions)?")]
    public bool freeMoveHandle = false;                 // Should we replace the arrow handles with a circular free move handle(Gives freedom of directions), same as the one in blender?
    internal static bool isFreeMoveHandle;

    [Tooltip("The radius of the circular free move handle.")]
    [Range(0.1f, 1f)]
    public float freeMoveHandleSize;                    // The radius of the circular free move handle
    internal static float freeHandleSize;

    [Tooltip("Should the vertex markers be shown?.This indicates which vertex will get snapped to which one.")]
    public bool showMarkers = true;                     // Should the vertex markers be shown?(This indicates which vertex will get snapped to which one)
    internal static bool isMarkerHidden;

    [Tooltip("Snap to the corner vertex nearest to the pointer, on the selected snap target.")]
    public bool vertexSnap = true;                      // Snap to the corner vertex nearest to the pointer, on the selected snap target
    internal static bool isVertexSnap;

    [Tooltip("If enabled you can snap vertices by clicking on a vertex on the selected object and then clicking on the vertex of the target object to snap vertices together.")]
    public bool vertexClickSnapping = true;            // If enabled you can snap vertices by clicking on a vertex on the selected object and then clicking on the vertex of the target object to snap vertices together.
    internal static bool isVertexClickSnapping;

    [Tooltip("The color to use for the line that guides vertex snapping when \"Vertex Click Snapping\" is selected.")]
    public Color32 snapGuideLineColor;                   //The color to use for the line that guides vertex snapping when \"Vertex Click Snapping\" is selected.

    [Tooltip("Snap to a vertex on a Face nearest to the pointer, on the selected snap target.")]
    public bool faceSnap = false;                       // Snap to the vertex on a Face nearest to the pointer, on the selected snap target
    internal static bool isFaceSnap;

    [Tooltip("Should we snap during rotation? i-e when the rotation tool is selected.")]
    public bool snapAtRotation;                         // Should we snap during rotation? i-e when the rotation tool is selected
    internal static bool snapAtRotationTool;
    internal static GameObject targetVertexMarker;
    internal static GameObject selectionVertexMarker;

    [Tooltip("Minimum distance required between the selected two vertices to snap.Adjusting this can cause snap to result at different distance ranges.")]
    public float snapThreshold = 2.5f;                  // Minimum distance that must be satisfied between the selected two vertices to snap
    internal static float minDistToSnap;

    [Tooltip("This color marks the target snap object's vertex to which we will snap the actively selected object's vertex (Default red color).")]
    public Color32 targetMarkerColor;                   // This color marks the target snap object's vertex to which we will snap the actively selected object's vertex      @Default red color

    [Tooltip("This color marks the actively selected object's vertex which will get snapped to the target snap object's vertex (Default green color).")]
    public Color32 selectionMarkerColor;                // This color marks the actively selected object's vertex which will get snapped to the target snap object's vertex  @Default green color

    [HideInInspector]
    public Material defaultMat;                         // Leave this as it is assigned by default

    internal static GameObject snap;
    
    internal static bool snapActive = false;
    internal static Dictionary<int, Material> materialsTable = new Dictionary<int, Material> { };


    private bool wasVertexActive = false;




    void Start()
    {
      

    }

    void Update()
    {
        //Debug.Log("This is snap info");
        if (!vertexSnap && !faceSnap)
        {
            if (wasVertexActive) { faceSnap = false; vertexSnap = true; wasVertexActive = false; }
            else { faceSnap = true; vertexSnap = false; wasVertexActive = true; }

        }

        snap = this.gameObject;

        selectionVertexMarker = snap.transform.GetChild(0).gameObject;
        targetVertexMarker = snap.transform.GetChild(1).gameObject;
        targetVertexMarker.GetComponent<MeshRenderer>().sharedMaterial.color = targetMarkerColor;
        selectionVertexMarker.GetComponent<MeshRenderer>().sharedMaterial.color = selectionMarkerColor;

        if (wasVertexActive) { if (faceSnap) { vertexSnap = false; wasVertexActive = false; } }
        else if (vertexSnap) { isVertexSnap = true; faceSnap = false; wasVertexActive = true; }// vertexClickSnapping = true; } 

        //if (!isVertexSnap) { vertexClickSnapping = false; }


        isMarkerHidden = !showMarkers;
        isVertexSnap = vertexSnap;
        isVertexClickSnapping = vertexClickSnapping;
        isFaceSnap = faceSnap;
        minDistToSnap = snapThreshold;
        targetTransparency = makeTargetTransparent;
        transparencyThreshold = transparencyAmount;
        handles2Pointer = handlesToPointer;
        isFreeMoveHandle = freeMoveHandle;
        freeHandleSize = freeMoveHandleSize;
        snapAtRotationTool = snapAtRotation;



        
        EditorPrefs.SetFloat("transparencyThreshold", transparencyAmount);
        EditorPrefs.SetBool("targetTransparency", makeTargetTransparent);
        EditorPrefs.SetBool("handles2Pointer", handlesToPointer);
        EditorPrefs.SetBool("isFreeMoveHandle",freeMoveHandle);
        EditorPrefs.SetFloat("freeHandleSize", freeMoveHandleSize);
        EditorPrefs.SetBool("isMarkerHidden", showMarkers);
        EditorPrefs.SetBool("isVertexSnap", vertexSnap);
        EditorPrefs.SetBool("isVertexClickSnapping",vertexClickSnapping);
        EditorPrefs.SetBool("isFaceSnap", faceSnap);
        EditorPrefs.SetBool("snapAtRotationTool", snapAtRotation);
        EditorPrefs.SetFloat("minDistToSnap", snapThreshold);
        EditorPrefs.SetString("snapGuideLineColor", ColorUtility.ToHtmlStringRGBA(Col32ToCol(snapGuideLineColor)));
        EditorPrefs.SetString("targetMarkerColor", ColorUtility.ToHtmlStringRGBA(Col32ToCol(targetMarkerColor)));
        EditorPrefs.SetString("selectionMarkerColor", ColorUtility.ToHtmlStringRGBA(Col32ToCol(selectionMarkerColor)));



    }

    void OnDisable()
    {
        snapActive = false;
        Tools.hidden = false;
    }

    void OnEnable()
    {
        snapActive = true;


        isVertexSnap = EditorPrefs.HasKey("isVertexSnap") ? EditorPrefs.GetBool("isVertexSnap") : vertexSnap;
        isVertexClickSnapping = EditorPrefs.HasKey("isVertexClickSnapping") ? EditorPrefs.GetBool("isVertexClickSnapping") : vertexClickSnapping;
        isFaceSnap = EditorPrefs.HasKey("isFaceSnap") ? EditorPrefs.GetBool("isFaceSnap") : faceSnap;
        isMarkerHidden = EditorPrefs.HasKey("isMarkerHidden") ? EditorPrefs.GetBool("isMarkerHidden") : !showMarkers;
        minDistToSnap = EditorPrefs.HasKey("minDistToSnap") ? EditorPrefs.GetFloat("minDistToSnap") : snapThreshold;
        targetTransparency = EditorPrefs.HasKey("targetTransparency") ? EditorPrefs.GetBool("targetTransparency") : makeTargetTransparent;
        transparencyThreshold = (byte)(EditorPrefs.HasKey("transparencyThreshold") ? EditorPrefs.GetInt("transparencyThreshold") : transparencyAmount);
        handles2Pointer = EditorPrefs.HasKey("handles2Pointer") ? EditorPrefs.GetBool("handles2Pointer") : handlesToPointer;
        isFreeMoveHandle = EditorPrefs.HasKey("isFreeMoveHandle") ? EditorPrefs.GetBool("isFreeMoveHandle") : freeMoveHandle;
        freeHandleSize = EditorPrefs.HasKey("freeHandleSize") ? EditorPrefs.GetFloat("freeHandleSize") : freeMoveHandleSize;
        snapAtRotationTool = EditorPrefs.HasKey("snapAtRotationTool") ? EditorPrefs.GetBool("snapAtRotationTool") : snapAtRotation;


        Color col = Color.black;

        if (EditorPrefs.HasKey("snapGuideLineColor"))
        {
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("snapGuideLineColor"), out col);
            snapGuideLineColor = ColToCol32(col);
        }

        if (EditorPrefs.HasKey("targetMarkerColor"))
        {
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("targetMarkerColor"), out col);
            targetMarkerColor = ColToCol32(col);
        }

        if (EditorPrefs.HasKey("selectionMarkerColor"))
        {
            ColorUtility.TryParseHtmlString("#" + EditorPrefs.GetString("selectionMarkerColor"), out col);
            selectionMarkerColor = ColToCol32(col);
        }
    }






    public static Color32 ColToCol32(Color col)
    {
        return new Color32((byte)(col.r * 255), (byte)(col.g * 255), (byte)(col.b * 255), (byte)(col.a * 255));
    }



    public static Color Col32ToCol(Color32 col)
    {
        return new Color(col.r / 255f, col.g / 255f, col.b / 255f, col.a / 255f);
    }

}

#endif

}