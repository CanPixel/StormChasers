using UnityEngine;
using FIMSpace.FOptimizing;

/// <summary>
/// FC: Scriptable container for IFLOD
/// </summary>
//[CreateAssetMenu(menuName = "Custom Optimizers/LOD_NavMeshAgent Reference")]
public sealed class ScrLOD_NavMeshAgent : ScrLOD_Base
{
    [SerializeField]
    private LODI_NavMeshAgent settings;
    public override ILODInstance GetLODInstance() { return settings; }
    public ScrLOD_NavMeshAgent() { settings = new LODI_NavMeshAgent(); }

    public override ScrLOD_Base GetScrLODInstance()
    {
        return CreateInstance<ScrLOD_NavMeshAgent>();
    }

    public override ScrLOD_Base CreateNewScrCopy()
    {
        ScrLOD_NavMeshAgent lodA = CreateInstance<ScrLOD_NavMeshAgent>();
        //lodA.CopyBase(this);
        //lodA.Priority = Priority;
        //lodA.Quality = Quality;
        return lodA;
    }


    public override ScriptableLODsController GenerateLODController(Component target, ScriptableOptimizer optimizer)
    {
        UnityEngine.AI.NavMeshAgent c = target as UnityEngine.AI.NavMeshAgent;
        if (!c) c = target.GetComponentInChildren<UnityEngine.AI.NavMeshAgent>();
        if (c) if (!optimizer.ContainsComponent(c))
            {
                return new ScriptableLODsController(optimizer, c, -1, "NavMeshAgent", this);
            }

        return null;
    }

}
