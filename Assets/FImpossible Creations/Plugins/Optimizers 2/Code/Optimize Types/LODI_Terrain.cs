using UnityEngine;
using FIMSpace.FOptimizing;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// FM: Helper class for single LOD level settings on Terrain
/// </summary>
//[CreateAssetMenu(menuName = "Custom Optimizers/FLOD_Terrain Reference")]
[System.Serializable]
public sealed class LODI_Terrain : ILODInstance
{

    #region Main Settings : Interface Properties

    public int Index { get { return index; } set {  index = value; } }
    internal int index = -1;
    public string Name { get { return LODName; } set {  LODName = value; } }
    internal string LODName = "";
    public bool CustomEditor { get { return false; } }
    public bool Disable { get { return SetDisabled; } set {  SetDisabled = value; } }
    [HideInInspector] public bool SetDisabled = false;
    public bool DrawDisableOption { get { return false; } }
    public bool SupportingTransitions { get { return true; } }
    public bool DrawLowererSlider { get { return false; } }
    public float QualityLowerer { get { return 1f; } set {  new System.NotImplementedException(); } }
    public string HeaderText { get { return "Terrain LOD Settings"; } }
    public float ToCullDelay { get { return 0f; } }
    public int DrawingVersion { get { return 1; } set {  new System.NotImplementedException(); } }
    public Texture Icon { get { return
#if UNITY_EDITOR
        EditorGUIUtility.IconContent("Terrain Icon").image;
#else
        null;
#endif
        } }

    public Component TargetComponent { get { return cmp; } }
    [SerializeField] [HideInInspector] private Terrain cmp;

    #endregion


    [Range(1, 200)]
    public float PixelError = 5;
    [Range(0, 2000)]
    public float BasemapDistance = 1250f;

    [Space(3f)]
    [Range(0, 250)]
    public float DetailDistance = 100f;
    [Range(0, 1)]
    public float DetailDensity = 1f;

    [Space(3f)]
    [Range(0, 2000)]
    public float TreeDistance = 2000f;
    [Range(1f, 5f)]
    public float TreeLODBias = 1f;
    [Range(5, 2000)]
    public float BillboardStart = 50f;

    [Space(3f)]
    public bool DrawFoliage = true;

    //#if UNITY_2019_1_OR_NEWER
    public UnityEngine.Rendering.ShadowCastingMode Mode;
    //#else
    public bool CastShadows = true;
    //#endif


    public bool DrawHeightmap = true;

    [Tooltip("Dividing resolution of heightmap")]
    [Range(0, 3)]
    public int ResolutionDivider = 0;

    [Space(3f)]
    [Tooltip("Optional - Replace drawing terrain with target gameObject with mesh renderer for final optimization when terrain is far away (terrain collider will still work)")]
    public GameObject MeshReplacement = null;


    public void SetSameValuesAsComponent(Component component)
    {
        if (component == null) Debug.LogError("[OPTIMIZERS] Given component is null instead of Terrain!");

        Terrain comp = component as Terrain;

        if (comp != null)
        {
            cmp = comp;
            PixelError = comp.heightmapPixelError;
            BasemapDistance = comp.basemapDistance;
            DetailDistance = comp.detailObjectDistance;
            DetailDensity = comp.detailObjectDensity;
            TreeDistance = comp.treeDistance;
            BillboardStart = comp.treeBillboardDistance;
            DrawFoliage = comp.drawTreesAndFoliage;

#if UNITY_2019_1_OR_NEWER
            Mode = comp.shadowCastingMode;
#else
            CastShadows = comp.castShadows;
#endif

            TreeLODBias = comp.treeLODBiasMultiplier;
            ResolutionDivider = comp.heightmapMaximumLOD;
            DrawHeightmap = comp.drawHeightmap;
            MeshReplacement = null;
        }
    }


    public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettingsRef)
    {
        Terrain comp = component as Terrain;

        // Initital settings not needed for this type of component (terrain)
        if (comp == null) { Debug.LogError("[OPTIMIZERS] Target component is null or is not Terrain!"); return; }
        LODI_Terrain initLOD = initialSettingsRef as LODI_Terrain;

        if (MeshReplacement == null)
        {
            if (Disable)
            {
                comp.enabled = false;
            }
            else
            {
                if (comp.enabled == false) comp.enabled = true;

                comp.heightmapPixelError = PixelError;

                if (comp.detailObjectDistance != BasemapDistance) comp.detailObjectDistance = BasemapDistance;
                if (comp.detailObjectDensity != DetailDistance) comp.detailObjectDensity = DetailDistance;
                if (comp.detailObjectDensity != DetailDensity) comp.detailObjectDensity = DetailDensity;
                if (comp.treeDistance != TreeDistance) comp.treeDistance = TreeDistance;
                if (comp.treeBillboardDistance != BillboardStart) comp.treeBillboardDistance = BillboardStart;

                comp.drawTreesAndFoliage = DrawFoliage;

#if UNITY_2019_1_OR_NEWER
                comp.shadowCastingMode = Mode;
#else
comp.castShadows = CastShadows;
#endif

                comp.treeLODBiasMultiplier = TreeLODBias;
                comp.drawHeightmap = DrawHeightmap;

                if (comp.drawTreesAndFoliage == false || comp.drawHeightmap == false)
                    comp.collectDetailPatches = false;
                else
                    comp.collectDetailPatches = true;

                comp.heightmapMaximumLOD = ResolutionDivider;
            }

            if (initLOD.MeshReplacement) initLOD.MeshReplacement.SetActive(false);
        }
        else
        {
#if UNITY_2019_1_OR_NEWER
            comp.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
            comp.castShadows = false;
#endif


            comp.drawHeightmap = false;
            comp.drawTreesAndFoliage = false;
            comp.collectDetailPatches = false;

            Transform mesh = comp.transform.Find(comp.name);

            if (!mesh)
            {
                GameObject instantiated = GameObject.Instantiate(MeshReplacement);
                mesh = instantiated.transform;
                mesh.name = comp.name;
                mesh.position = comp.transform.position;
                mesh.SetParent(comp.transform, true);

                initLOD.MeshReplacement = mesh.gameObject;
            }

            mesh.gameObject.SetActive(true);
        }
    }


    public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component component)
    {
        Terrain comp = component as Terrain;
        if (comp == null) Debug.LogError("[OPTIMIZERS] Given component for reference values is null or is not Terrain Component!");

        // REMEMBER: LOD = 0 is not nearest but one after nearest
        // Trying to auto configure universal LOD settings

        float mul = FLOD.GetValueForLODLevel(1f, 0f, lodIndex, lodCount); // Starts from 0.75 (LOD1), then 0.5, 0.25 and 0.0 (Culled) if lod count is = 4

        // Your auto settings depending of LOD count
        // For example LOD count = 3, you want every next LOD go with parameters from 1f, to 0.6f, 0.3f, 0f - when culled

        PixelError = (int)Mathf.Lerp(comp.heightmapPixelError + 22, comp.heightmapPixelError, mul);
        BasemapDistance = Mathf.Lerp(comp.basemapDistance / 5f, comp.basemapDistance / 1f, mul);
        DetailDistance = Mathf.Lerp(comp.detailObjectDistance / 4f, comp.detailObjectDistance, mul);
        DetailDensity = Mathf.Lerp(comp.detailObjectDensity / 5f, comp.detailObjectDensity, mul);
        TreeDistance = comp.treeDistance;
        BillboardStart = comp.treeBillboardDistance;
        TreeLODBias = 1f;
        DrawHeightmap = true;
        ResolutionDivider = 0;

#if UNITY_2019_1_OR_NEWER
        Mode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
        CastShadows = false;
#endif


        DrawFoliage = false;

        if (lodIndex >= 1)
        {
            DrawFoliage = false;
            TreeLODBias = Mathf.Lerp(2f, 1f, mul);
            if (lodCount <= 3) PixelError = comp.heightmapPixelError + 16;
        }

        if (lodIndex >= 2)
        {
            ResolutionDivider = 1;
            PixelError = comp.heightmapPixelError + 18;
        }

        //if (lodCount > 2) if (lodIndex == lodCount - 2) CastShadows = false;

        Name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
    }


    public void AssignSettingsAsForCulled(Component component)
    {
        FLOD.AssignDefaultCulledParams(this);

        Disable = false;
        PixelError = 200;
        BasemapDistance = 500;
        DetailDistance = 0;
        DetailDensity = 0;
        TreeDistance = 0;
        BillboardStart = 5;
        DrawFoliage = false;

#if UNITY_2019_1_OR_NEWER
        Mode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
        CastShadows = false;
#endif

        TreeLODBias = 1f;
        ResolutionDivider = 0;
        DrawHeightmap = false;
    }


    public void AssignSettingsAsForNearest(Component component)
    {
        FLOD.AssignDefaultNearestParams(this);

        Terrain comp = component as Terrain;
        SetSameValuesAsComponent(comp);
    }

    public void AssignSettingsAsForHidden(Component component)
    {
        FLOD.AssignDefaultHiddenParams(this);

        DrawFoliage = false;

#if UNITY_2019_1_OR_NEWER
        Mode = UnityEngine.Rendering.ShadowCastingMode.Off;
#else
        CastShadows = false;
#endif

        TreeLODBias = 1f;
        ResolutionDivider = 0;
        DrawHeightmap = false;
    }

    public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }


    public void InterpolateBetween(ILODInstance lodA, ILODInstance lodB, float transitionToB)
    {
        FLOD.DoBaseInterpolation(this, lodA, lodB, transitionToB);

        LODI_Terrain a = lodA as LODI_Terrain;
        LODI_Terrain b = lodB as LODI_Terrain;

        PixelError = Mathf.Lerp(a.PixelError, b.PixelError, transitionToB);
        BasemapDistance = Mathf.Lerp(a.BasemapDistance, b.BasemapDistance, transitionToB);
        DetailDistance = Mathf.Lerp(a.DetailDistance, b.DetailDistance, transitionToB);
        DetailDensity = Mathf.Lerp(a.DetailDensity, b.DetailDensity, transitionToB);
        TreeDistance = Mathf.Lerp(a.TreeDistance, b.TreeDistance, transitionToB);
        BillboardStart = Mathf.Lerp(a.BillboardStart, b.BillboardStart, transitionToB);
        TreeLODBias = Mathf.Lerp(a.TreeLODBias, b.TreeLODBias, transitionToB);
        ResolutionDivider = (int)Mathf.Lerp(a.ResolutionDivider, b.ResolutionDivider, transitionToB);

        DrawFoliage = FLOD.BoolTransition(DrawFoliage, a.DrawFoliage, b.DrawFoliage, transitionToB);

#if UNITY_2019_1_OR_NEWER
        if (transitionToB > 0) Mode = b.Mode;
#else
        CastShadows = FLOD.BoolTransition(CastShadows, a.CastShadows, b.CastShadows, transitionToB);
#endif


        DrawHeightmap = FLOD.BoolTransition(DrawHeightmap, a.DrawHeightmap, b.DrawHeightmap, transitionToB);
        MeshReplacement = (GameObject)FLOD.ObjectTransition(MeshReplacement, a.MeshReplacement, b.MeshReplacement, transitionToB);
    }


#if UNITY_EDITOR
    public void AssignToggler(ILODInstance reference)
    { }

    public void DrawTogglers(SerializedProperty iflodProp)
    { }

    public void CustomEditorWindow( SerializedProperty iflodProp)
    { }
#endif
}
