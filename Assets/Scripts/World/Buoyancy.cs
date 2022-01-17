 using UnityEngine;
 using System.Collections;
 
 public class Buoyancy : MonoBehaviour 
 {
 
     public float buoyancy = 20;            //Buoyancy force
     public float viscosity = 20;                  //How easily an object can move through the water
     
     private Rigidbody rb;                //Rigidbody attached to this object
 
     // Use this for initialization
     void Start () 
     {
         rb = GetComponent<Rigidbody>();            //Set the rigidbody at startup
     }
     
     // Update is called once per frame
     void FixedUpdate () 
     {
         Vector3[] vertices = WaterDeformation.mesh.vertices;            //Find the water's vertices
         Vector3[] worldVerts = new Vector3[vertices.Length];            //Create a new array to store world space vertex positions
         
         for (int i = 0; i < vertices.Length; i++) {
             worldVerts[i] = WaterDeformation.water.TransformPoint (vertices[i]);        //For every vertex, transform the position into world space
         }
         
         Vector3 nearestVert = NearestVertice (transform.position, worldVerts);            //Find the nearest vertice to this object
         
         if (transform.position.y < nearestVert.y)    {    //If this object is below the nearest vertice
             rb.AddForce (Vector3.up * buoyancy);        //Apply force upwards
                     rb.velocity /= ((viscosity / 100) + 1);           //Slow the objects movements when in water
             }
     }
     
     Vector3 NearestVertice (Vector3 pos, Vector3[] verts)            //Takes a position and a position array
     {
         Vector3 nearestVert = Vector3.zero;            //Create the initial nearestVert variable and initialise it
         float minDist = 100;                        //Declare the min dist (can be whatever you want, something large though)
         
         for (int i = 0; i < verts.Length; i++) {        //For every vertice
             if (Vector3.Distance (pos, verts[i]) < minDist) {        //If the vertice is closer than the one before it
                 nearestVert = verts[i];                                //Set the nearest vertice variable
                 minDist = Vector3.Distance (pos, verts[i]);            //Update the minDist
             }
         }
         
         return nearestVert;            //Return the nearest vertice
     }
 }