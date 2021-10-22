using UnityEngine;
using System.Collections;

public class VehicleDamage : MonoBehaviour {
    public float maxMoveDelta = 1.0f; // maximum distance one vertice moves per explosion (in meters)
    public float maxCollisionStrength = 50.0f;
    public float YforceDamp = 0.1f; // 0.0 - 1.0
    public float demolutionRange = 0.5f;
    public float impactDirManipulator = 0.0f;
    public MeshFilter[] optionalMeshList;
    public AudioSource crashSound;

    private MeshFilter[] meshFilters;
    private float sqrDemRange;

    public bool enable = false;

    public void Start() {
        if(!enable) return;

        sqrDemRange = demolutionRange * demolutionRange;

        meshFilters = optionalMeshList;

        for(int i = 0; i < meshFilters.Length; i++) {
            var mesh = Mesh.Instantiate(meshFilters[i].mesh);
            meshFilters[i].mesh = mesh;
        }
    }

    private Vector3 colPointToMe;
    private float colStrength;

    public void OnCollisionEnter(Collision collision) {
        if(!enable) return;
        //  if (collision.gameObject.CompareTag("car")) return;

        Vector3 colRelVel = collision.relativeVelocity;
        colRelVel.y *= YforceDamp;

        if (collision.contacts.Length > 0) {
            colPointToMe = transform.position - collision.contacts[0].point;
            colStrength = colRelVel.magnitude * Vector3.Dot(collision.contacts[0].normal, colPointToMe.normalized);

            if (colPointToMe.magnitude > 1.0f && !crashSound.isPlaying) {
                crashSound.Play();
                crashSound.volume = colStrength / 200;

                OnMeshForce(collision.contacts[0].point, Mathf.Clamp01(colStrength / maxCollisionStrength));
            }
        }
    }

    // if called by SendMessage(), we only have 1 param
    public void OnMeshForce(Vector4 originPosAndForce) {
        OnMeshForce((Vector3)originPosAndForce, originPosAndForce.w);
    }

    public void OnMeshForce(Vector3 originPos, float force) {
        // force should be between 0.0 and 1.0
        force = Mathf.Clamp01(force);

        for (int j = 0; j < meshFilters.Length; ++j) {   
            Vector3[] verts = meshFilters[j].mesh.vertices;

            for (int i = 0; i < verts.Length; ++i) {
                Vector3 scaledVert = Vector3.Scale(verts[i], transform.localScale);
                Vector3 vertWorldPos = meshFilters[j].transform.position + (meshFilters[j].transform.rotation * scaledVert);
                Vector3 originToMeDir = vertWorldPos - originPos;
                Vector3 flatVertToCenterDir = transform.position - vertWorldPos;
                flatVertToCenterDir.y = 0.0f;

                Debug.Log(originToMeDir.sqrMagnitude + " | " + sqrDemRange);

                // 0.5 - 1 => 45� to 0�  / current vertice is nearer to exploPos than center of bounds
                if (originToMeDir.sqrMagnitude < sqrDemRange) //dot > 0.8f ) 
                {
                    float dist = Mathf.Clamp01(originToMeDir.sqrMagnitude / sqrDemRange);
                    float moveDelta = force * (1.0f - dist) * maxMoveDelta;

                    Vector3 moveDir = Vector3.Slerp(originToMeDir, flatVertToCenterDir, impactDirManipulator).normalized * moveDelta;

                    verts[i] += Quaternion.Inverse(transform.rotation) * moveDir;
                }
            }
            meshFilters[j].mesh.vertices = verts;
            meshFilters[j].mesh.RecalculateBounds(); 
        }
    }
}