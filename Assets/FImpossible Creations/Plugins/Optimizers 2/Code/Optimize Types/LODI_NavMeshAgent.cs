using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using FIMSpace.FOptimizing;

/// <summary>
/// FM: Helper class for single LOD level settings on NavMeshAgent
/// </summary>
//[CreateAssetMenu(menuName = "Custom Optimizers/LOD_NavMeshAgent Reference")]
[System.Serializable]
public sealed class LODI_NavMeshAgent : ILODInstance
{

    #region Main Settings : Interface Properties

    public int Index { get { return index; } set { index = value; } }
    internal int index = -1;
    public string Name { get { return LODName; } set { LODName = value; } }
    internal string LODName = "";
    public bool CustomEditor { get { return false; } }
    public bool Disable { get { return SetDisabled; } set { SetDisabled = value; } }
    [HideInInspector] public bool SetDisabled = false;
    public bool DrawDisableOption { get { return true; } }
    public bool SupportingTransitions { get { return true; } }
    public bool DrawLowererSlider { get { return false; } }
    public float QualityLowerer { get { return 1f; } set { new System.NotImplementedException(); } }
    public string HeaderText { get { return "NavMeshAgent LOD Settings"; } }
    public float ToCullDelay { get { return 0f; } }
    public int DrawingVersion { get { return 1; } set { new System.NotImplementedException(); } }
    public Texture Icon { get { return
#if UNITY_EDITOR
        EditorGUIUtility.IconContent("NavMeshAgent Icon").image;
#else
        null;
#endif
        } }

    public Component TargetComponent { get { return cmp; } }
    [SerializeField] [HideInInspector] private NavMeshAgent cmp;

    #endregion


    [Space(4)]
    [Range(0f, 1f)]
    public float Priority = 1f;
    public UnityEngine.AI.ObstacleAvoidanceType Quality = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;



    public void SetSameValuesAsComponent(Component component)
    {
        if (component == null) Debug.LogError("[Custom OPTIMIZERS] Given component is null instead of NavMeshAgent!");

        UnityEngine.AI.NavMeshAgent comp = component as UnityEngine.AI.NavMeshAgent;

        if (comp != null)
        {
            Priority = comp.avoidancePriority;
            Quality = comp.obstacleAvoidanceType;
            cmp = comp;
        }
    }


    public void ApplySettingsToTheComponent(Component component, ILODInstance initialSettingsRef)
    {
        // Casting LOD to correct type
        LODI_NavMeshAgent initialSettings = initialSettingsRef as LODI_NavMeshAgent;
        UnityEngine.AI.NavMeshAgent comp = component as UnityEngine.AI.NavMeshAgent;

        #region Security

        // Checking if casting is right
        if (initialSettings == null || comp == null) { Debug.Log("[Custom OPTIMIZERS] Target LOD is not NavMeshAgent LOD or is null"); return; }

        #endregion

        comp.avoidancePriority = (int)Mathf.Clamp(initialSettings.Priority * Priority, 0, 99);
        comp.obstacleAvoidanceType = Quality;

        FLOD.ApplyEnableDisableState(this, comp);
    }


    public void AssignAutoSettingsAsForLODLevel(int lodIndex, int lodCount, Component component)
    {
        UnityEngine.AI.NavMeshAgent comp = component as UnityEngine.AI.NavMeshAgent;
        if (comp == null) Debug.LogError("[Custom OPTIMIZERS] Given component for reference values is null or is not NavMeshAgent Component!");

        // REMEMBER: LOD = 0 is not nearest but one after nearest
        // Trying to auto configure universal LOD settings

        float mul = FLOD.GetValueForLODLevel(1f, 0f, lodIndex, lodCount); // Starts from 0.75 (LOD1), then 0.5, 0.25 and 0.0 (Culled) if lod count is = 4

        // Your auto settings depending of LOD count
        Priority = mul;
        int q = (int)Quality;
        q = (int)(q * mul);
        Quality = (UnityEngine.AI.ObstacleAvoidanceType)q;

        Name = "LOD" + (lodIndex + 2); // + 2 to view it in more responsive way for user inside inspector window
    }


    public void AssignSettingsAsForCulled(Component component)
    {
        FLOD.AssignDefaultCulledParams(this);
        Priority = 0;
        Quality = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
    }

    public void AssignSettingsAsForNearest(Component component)
    {
        FLOD.AssignDefaultNearestParams(this);

        //UnityEngine.AI.NavMeshAgent comp = component as UnityEngine.AI.NavMeshAgent;
        Priority = 1;
        Quality = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    }

    public void AssignSettingsAsForHidden(Component component)
    {
        FLOD.AssignDefaultHiddenParams(this);
        Priority = 0.2f;
        Quality = UnityEngine.AI.ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    }

    public ILODInstance GetCopy() { return MemberwiseClone() as ILODInstance; }

    public void InterpolateBetween(ILODInstance lodA, ILODInstance lodB, float transitionToB)
    {
        FLOD.DoBaseInterpolation(this, lodA, lodB, transitionToB);

        LODI_NavMeshAgent a = lodA as LODI_NavMeshAgent;
        LODI_NavMeshAgent b = lodB as LODI_NavMeshAgent;

        Priority = Mathf.Lerp(a.Priority, b.Priority, transitionToB);

        int ia = (int)a.Quality;
        int ib = (int)b.Quality;
        int avoidance = (int)Mathf.Lerp(ia, ib, transitionToB);
        Quality = (UnityEngine.AI.ObstacleAvoidanceType)avoidance;
    }





#if UNITY_EDITOR
    public void AssignToggler(ILODInstance reference)
    { }

    public void DrawTogglers(SerializedProperty iflodProp)
    { }

    public void CustomEditorWindow(SerializedProperty prop)
    { }
#endif

}
