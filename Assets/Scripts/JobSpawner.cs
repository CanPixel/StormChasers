using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JobSpawner : MonoBehaviour {
    public int maxJobsToSpawn = 14;

    public MissionManager missionManager;
    public Transform jobLocationsObject, clientLocationsObject;

    [ReadOnly] public List<Transform> jobLocations = new List<Transform>();
    [ReadOnly] public Region[] clientLocations;
    
    [Space(5)]
    public GameObject[] characters;

    void Start() {
        InitLocations();
        SpawnJobs();
    }

    void OnValidate() {
        InitLocations();
    }

    protected void InitLocations() {
        jobLocations.Clear();
        foreach(Transform t in jobLocationsObject) jobLocations.Add(t);
        clientLocations = GameObject.FindObjectsOfType<Region>();
    }

    protected void SpawnJobs() {
        var temp = new List<Transform>(jobLocations);

        for(int i = 0; i < maxJobsToSpawn; i++) {
            if(temp.Count <= 0) break;
            var job = temp[Random.Range(0, temp.Count)];
            var obj = Instantiate(characters[Random.Range(0, characters.Length)]).GetComponent<DialogChar>();
            obj.transform.SetParent(job);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            obj.location.mission = MissionManager.CreateMission(obj);
            MissionManager.Initialize(obj.location.mission);
            temp.Remove(job);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach(var i in jobLocations) {
            if(i != null) ForGizmo(i.position, i.forward * 5, 1);
        }
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.color = color;
        Gizmos.DrawRay(pos, direction);
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
    }

    public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction);
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength);
        Debug.DrawRay(pos + direction, left * arrowHeadLength);
    }
    public static void ForDebug(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Debug.DrawRay(pos, direction, color);
        
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180+arrowHeadAngle,0) * new Vector3(0,0,1);
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0,180-arrowHeadAngle,0) * new Vector3(0,0,1);
        Debug.DrawRay(pos + direction, right * arrowHeadLength, color);
        Debug.DrawRay(pos + direction, left * arrowHeadLength, color);
    }
}