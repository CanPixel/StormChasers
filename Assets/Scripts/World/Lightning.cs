using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Lightning : MonoBehaviour {
    private Vector3 target;
    public LineRenderer lineRend;
    public float arcLength = 1.0f;
    public float arcVariation = 1.0f;
    public float inaccuracy = 0.5f;
    public float timeOfZap = 0.25f;
    public float strikeInterval = 1f;
    private float strikeTimer;

    public int segments = 10;

    private float strikeProgress = 0;

    private List<Vector3> points = new List<Vector3>();

    void Update() {
        if(strikeTimer > strikeInterval) {
            ZapTarget(GetRandomLocation());
            strikeTimer = 0;
        }
        strikeTimer += Time.deltaTime;

        if(strikeProgress > 0) strikeProgress -= Time.deltaTime;
        strikeProgress = Mathf.Clamp01(strikeProgress);

        var col = lineRend.material.GetColor("_Color");
        lineRend.material.SetColor("_Color", new Color(col.r, col.g, col.b, Mathf.Lerp(col.a, strikeProgress, Time.deltaTime * 7f)));
    }

    private Vector3 Randomize (Vector3 newVector, float devation) {
        newVector += new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * devation;
        newVector.Normalize();
        return newVector;
    }
 
    public void ZapTarget(Vector3 newTarget){
        target = newTarget;

        points.Clear();
        lineRend.positionCount = 1;
        strikeProgress = 1;

        Vector3 lastPoint = transform.position;
        int i = 1;
        points.Insert(0, transform.position);
        for (int k = 0; k < segments; k++) {//was the last arc not touching the target?
            Vector3 fwd = target - lastPoint;//gives the direction to our target from the end of the last arc
            fwd.Normalize ();//makes the direction to scale
            fwd = Randomize (fwd, inaccuracy);//we don't want a straight line to the target though
            fwd *= Random.Range (arcLength * arcVariation, arcLength);//nature is never too uniform
            fwd += lastPoint;//point + distance * direction = new point. this is where our new arc ends
            points.Insert(i, fwd);
            i++;
            lastPoint = fwd;//so we know where we are starting from for the next arc
        }
        points.Insert(i, target);

        var col = lineRend.material.GetColor("_Color");
        lineRend.material.SetColor("_Color", new Color(col.r, col.g, col.b, 1));

        lineRend.positionCount = points.Count;
        for(int m = 0; m < points.Count; m++) lineRend.SetPosition(m, points[m]);
    }

    Vector3 GetRandomLocation() {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int maxIndices = navMeshData.indices.Length - 3;
        // Pick the first indice of a random triangle in the nav mesh
        int firstVertexSelected = Random.Range(0, maxIndices);
        int secondVertexSelected = Random.Range(0, maxIndices);
        //Spawn on Verticies
        Vector3 point = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];

        Vector3 firstVertexPosition = navMeshData.vertices[navMeshData.indices[firstVertexSelected]];
        Vector3 secondVertexPosition = navMeshData.vertices[navMeshData.indices[secondVertexSelected]];
        //Eliminate points that share a similar X or Z position to stop spawining in square grid line formations
        if ((int)firstVertexPosition.x == (int)secondVertexPosition.x ||
            (int)firstVertexPosition.z == (int)secondVertexPosition.z
            )
        {
            point = GetRandomLocation(); //Re-Roll a position - I'm not happy with this recursion it could be better
        }
        else
        {
            // Select a random point on it
            point = Vector3.Lerp(
                                            firstVertexPosition,
                                            secondVertexPosition, //[t + 1]],
                                            Random.Range(0.05f, 0.95f) // Not using Random.value as clumps form around Verticies 
                                        );
        }
        //Vector3.Lerp(point, navMeshData.vertices[navMeshData.indices[t + 2]], Random.value); //Made Obsolete

        return point;
    }
}