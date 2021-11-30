using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FOptimizing
{
    public abstract partial class Optimizer_Base
    {
        /// <summary>
        /// Getting bounding spheres if not adding to container
        /// </summary>
        protected BoundingSphere[] GetBoundingSpheresMultiShape()
        {
            BoundingSphere[] spheres = new BoundingSphere[Shapes.Count];

            for (int i = 0; i < Shapes.Count; i++)
            {
                if (Shapes[i].transform == null)
                    spheres[i] = new BoundingSphere(transform.TransformPoint(Shapes[i].position), DetectionRadius * Shapes[i].radius);
                else
                    spheres[i] = new BoundingSphere(Shapes[i].transform.TransformPoint(Shapes[i].position), DetectionRadius * Shapes[i].radius);
            }

            return spheres;
        }



        /// <returns> 5 element array of floats, 0-x, 1-y, 2-z, 3-farthest sphere from center distance, 4-biggest radius sphere </returns>
        protected float[] GetCenterPosAndFarthest()
        {
            float[] elements = new float[5];

            Vector3 result = Vector3.zero;

            if (visibilitySpheres == null)
                visibilitySpheres = GetBoundingSpheresMultiShape();

            for (int i = 0; i < visibilitySpheres.Length; i++)
                result += GetVisibilitySphere(i).position;

            result /= (float)Shapes.Count;

            float dist;
            float farthest = 0f;
            float biggestRadius = 0f;

            for (int i = 0; i < visibilitySpheres.Length; i++)
            {
                dist = Vector3.Distance(GetVisibilitySphere(i).position, result);
                if (dist > farthest) farthest = dist;
                if (GetVisibilitySphere(i).radius > biggestRadius) biggestRadius = GetVisibilitySphere(i).radius;
            }

            elements[0] = result.x;
            elements[1] = result.y;
            elements[2] = result.z;
            elements[3] = farthest;
            elements[4] = biggestRadius;
            return elements;
        }



        /// <summary>
        /// Generating spheres for target mesh structure
        /// </summary>
        public void GenerateAutoShape()
        {
            if (AutoReferenceMesh)
            {
                List<Vector3> positions = GetPointsFromMesh(AutoReferenceMesh, AutoPrecision);
                Shapes = new List<MultiShapeBound>();
                for (int i = 0; i < positions.Count; i++)
                {
                    Shapes.Add(new MultiShapeBound());
                    Shapes[i].position = positions[i];
                }
            }
            else
            {
                Debug.LogError("[OPTIMIZERS] No mesh to reference from");
            }
        }



        /// <summary>
        /// Getting points from mesh in certain separation distances
        /// </summary>
        protected List<Vector3> GetPointsFromMesh(Mesh mesh, float precision)
        {
            try
            {

                List<Vector3> avPoints = new List<Vector3>();
                float radius = mesh.bounds.size.magnitude / Mathf.Lerp(2, 10, precision);
                DetectionRadius = radius;

                avPoints.Add(mesh.vertices[0]);

                for (int i = 0; i < 100; i++)
                {
                    float nearDist = float.MaxValue;
                    int nearestAv = -1;

                    for (int v = 0; v < mesh.vertices.Length; v++)
                    {
#if UNITY_EDITOR
                        if (v % 50 == 0)
                        {
                            UnityEditor.EditorUtility.DisplayProgressBar("Analyzing Vertices (" + (i + 1) + ")", "Checking vertices to create " + (i + 1) + (i == 1 ? "st" : (i == 2 ? "nd" : (i < 4 ? "rd" : "th"))) + " detection sphere... (" + v + "/" + mesh.vertexCount + ")", (float)v / (float)mesh.vertexCount);
                        }
#endif

                        bool can = true;
                        float dist;

                        for (int a = 0; a < avPoints.Count; a++)
                        {
                            dist = Vector3.Distance(mesh.vertices[v], avPoints[a]);
                            if (dist < radius)
                            {
                                can = false;
                                break;
                            }
                        }

                        if (!can) continue;

                        dist = Vector3.Distance(mesh.vertices[v], avPoints[i]);
                        if (dist < nearDist)
                        {
                            nearDist = dist;
                            nearestAv = v;
                        }
                    }

                    if (nearestAv == -1) { break; }

                    avPoints.Add(mesh.vertices[nearestAv]);
                }

#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif

                return avPoints;
            }
            catch (System.Exception)
            {
#if UNITY_EDITOR
                UnityEditor.EditorUtility.ClearProgressBar();
#endif
            }

            return null;
        }



#if UNITY_EDITOR
        protected void Gizmos_DrawMultiShapes()
        {
            if (gameObject.activeInHierarchy == false) return;
            if (Shapes == null || Shapes.Count == 0) return;

            Color preCol = Gizmos.color;

            if (Shapes != null)
            {
                if (!Application.isPlaying)
                    visibilitySpheres = GetBoundingSpheresMultiShape();
                else if (AddToContainer)
                    visibilitySpheres = GetBoundingSpheresMultiShape();

                Gizmos.color = new Color(.9f, .9f, .9f, 0.5f * GizmosAlpha);

                for (int i = 0; i < visibilitySpheres.Length; i++)
                    Gizmos.DrawSphere(visibilitySpheres[i].position, visibilitySpheres[i].radius);

                float[] elements = GetCenterPosAndFarthest();
                DrawLODRangeSpheres(new Vector3(elements[0], elements[1], elements[2]), elements[3] + elements[4]);
            }

            Gizmos.color = preCol;
        }
#endif



    }
}
