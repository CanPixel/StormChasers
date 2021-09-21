using UnityEngine;
using System.Collections;

public class VehicleDamage : MonoBehaviour {
    public float maxMoveDelta = 1.0f; // maximum distance one vertice moves per explosion (in meters)
    public float maxCollisionStrength = 50.0f;
    public float YforceDamp = 0.1f; // 0.0 - 1.0
    public float demolutionRange = 0.5f;
    public float impactDirManipulator = 0.0f;
    public SkinnedMeshRenderer[] optionalMeshList;
    public MeshFilter[] optionalMeshListRender;
    public AudioSource crashSound;
    public bool useSkinnedMesh = true;

    private SkinnedMeshRenderer[] meshfilters;
    private MeshFilter[] meshfilterssss;
    private float sqrDemRange;

    public void Start() {
        sqrDemRange = demolutionRange * demolutionRange;

        if(useSkinnedMesh) {
            if (optionalMeshList.Length > 0) meshfilters = optionalMeshList;
            else meshfilters = GetComponentsInChildren<SkinnedMeshRenderer>();

            for(int i = 0; i < meshfilters.Length; i++) {
                var mesh = Mesh.Instantiate(meshfilters[i].sharedMesh);
                meshfilters[i].sharedMesh = mesh;
            }
        } else {
            if (optionalMeshList.Length > 0) meshfilterssss = optionalMeshListRender;
            else meshfilterssss = GetComponentsInChildren<MeshFilter>();

            for(int i = 0; i < meshfilterssss.Length; i++) {
                var mesh = Mesh.Instantiate(meshfilterssss[i].mesh);
                meshfilterssss[i].mesh = mesh;
            }
        }
    }

    private Vector3 colPointToMe;
    private float colStrength;

    public void OnCollisionEnter(Collision collision) {
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

        if(useSkinnedMesh) {
            for (int j = 0; j < meshfilters.Length; ++j) {   
                Vector3[] verts = meshfilters[j].sharedMesh.vertices;

                    for (int i = 0; i < verts.Length; ++i) {
                        Vector3 scaledVert = Vector3.Scale(verts[i], transform.localScale);
                        Vector3 vertWorldPos = meshfilters[j].transform.position + (meshfilters[j].transform.rotation * scaledVert);
                        Vector3 originToMeDir = vertWorldPos - originPos;
                        Vector3 flatVertToCenterDir = transform.position - vertWorldPos;
                        flatVertToCenterDir.y = 0.0f;

                        // 0.5 - 1 => 45� to 0�  / current vertice is nearer to exploPos than center of bounds
                        if (originToMeDir.sqrMagnitude < sqrDemRange) //dot > 0.8f ) 
                        {
                            float dist = Mathf.Clamp01(originToMeDir.sqrMagnitude / sqrDemRange);
                            float moveDelta = force * (1.0f - dist) * maxMoveDelta;

                            Vector3 moveDir = Vector3.Slerp(originToMeDir, flatVertToCenterDir, impactDirManipulator).normalized * moveDelta;

                            verts[i] += Quaternion.Inverse(transform.rotation) * moveDir;
                        }
                    }
                    meshfilters[j].sharedMesh.vertices = verts;
                    meshfilters[j].sharedMesh.RecalculateBounds();
                } 
        }
         else {
                for (int j = 0; j < meshfilterssss.Length; ++j) {   
                    Vector3[] verts = meshfilterssss[j].mesh.vertices;

                    for (int i = 0; i < verts.Length; ++i) {
                        Vector3 scaledVert = Vector3.Scale(verts[i], transform.localScale);
                        Vector3 vertWorldPos = meshfilterssss[j].transform.position + (meshfilterssss[j].transform.rotation * scaledVert);
                        Vector3 originToMeDir = vertWorldPos - originPos;
                        Vector3 flatVertToCenterDir = transform.position - vertWorldPos;
                        flatVertToCenterDir.y = 0.0f;

                        // 0.5 - 1 => 45� to 0�  / current vertice is nearer to exploPos than center of bounds
                        if (originToMeDir.sqrMagnitude < sqrDemRange) //dot > 0.8f ) 
                        {
                            float dist = Mathf.Clamp01(originToMeDir.sqrMagnitude / sqrDemRange);
                            float moveDelta = force * (1.0f - dist) * maxMoveDelta;

                            Vector3 moveDir = Vector3.Slerp(originToMeDir, flatVertToCenterDir, impactDirManipulator).normalized * moveDelta;

                            verts[i] += Quaternion.Inverse(transform.rotation) * moveDir;
                        }
                    }
                    meshfilterssss[j].mesh.vertices = verts;
                    meshfilterssss[j].mesh.RecalculateBounds(); 
                }
         }
    }
}