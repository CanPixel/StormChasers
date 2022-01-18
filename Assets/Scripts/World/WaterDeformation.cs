using UnityEngine;
 using System.Collections;
 
 public class WaterDeformation : MonoBehaviour 
 {
     
     public static Mesh mesh;            //The water mesh
     public static Transform water;        //The water transform
     
     public float deformAmount = 1;        //Amount to deform the water
     public float scale = 2.5f;            //How fine the displacement is
     public float speed = 1;                //The speed of waves
     
     private Vector2 time = Vector2.zero;    //The actual speed offset
     
     // Use this for initialization
     void Start () 
     {
         //Set the water and mesh variables at start
         water = transform;
         mesh = GetComponent<MeshFilter>().mesh;
     }
     
     // Update is called once per frame
     void Update () 
     {
         time = new Vector2 (Time.time, Time.time) * speed;            //Set up speed offset for deformation
         
         Vector3[] vertices = mesh.vertices;                //Create a variable for the vertices beforehand
         
         for (int i = 0; i < vertices.Length; i++) {
             vertices[i] = Deform (vertices[i]);                //For every vertice, deform the Y position
         }
         
         mesh.vertices = vertices;                    //Re-assign the vertices
         mesh.RecalculateNormals ();                    //Recalculate the normals so the object doesn't look flat
         GetComponent<MeshFilter>().mesh = mesh;        //Re-assign the mesh to the filter
     }
     
     Vector3 Deform (Vector3 v)            //Takes a Vector3
     {
         v.y = Mathf.PerlinNoise (v.x / scale + time.x, v.z / scale + time.y) * deformAmount;            //Distort the vertice's Y position based off its X and Z positions + time
         return v;            //Return the offset vertice position
     }
 }