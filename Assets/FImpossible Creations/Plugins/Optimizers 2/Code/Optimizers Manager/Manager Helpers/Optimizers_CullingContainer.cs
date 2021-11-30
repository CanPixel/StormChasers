using UnityEngine;

namespace FIMSpace.FOptimizing
{
    /// <summary>
    /// FM: Class which is managing CullingGroup bounding spheres and sending culling groups' events to optimizer components
    /// Objects need to have the same distance values to be able to be saved in container
    /// </summary>
    public class Optimizers_CullingContainer
    {
        /// <summary> Max capacity of this culling container </summary>
        private int MaxSlots = 1000;
        public int ID { get; private set; }
        public bool HaveFreeSlots { get { return HighestIndex < MaxSlots - 1; } }

        public bool Destroying = false;

        public CullingGroup CullingGroup { get; private set; }
        public Optimizer_Base[] Optimizers { get; private set; }
        public BoundingSphere[] CullingSpheres { get; private set; }

        /// <summary> If not using milti-shapes then this is number of added optimizers </summary>
        public int BoundingCount { get; private set; }
        /// <summary> Initial count of distance levels for ID specific optimizers </summary>
        public float[] DistanceLevels { get; private set; }

        public int HighestIndex { get; private set; }
        public int LowestFreeIndex { get; private set; }
        public int SlotsTaken { get; private set; }

#if UNITY_EDITOR
#endif

        public Optimizers_CullingContainer(int maxSlots)
        {
            MaxSlots = maxSlots;
            SlotsTaken = 0;
            Optimizers = new Optimizer_Base[MaxSlots];
            HighestIndex = 0;

#if UNITY_EDITOR
#endif
        }


        /// <summary>
        /// Initializing container with distances, defining ID and preparing CullingGroup to work
        /// </summary>
        public void InitializeContainer(int id, float[] distances, Camera targetCamera)
        {
            ID = id;

            DistanceLevels = new float[distances.Length + 2];
            DistanceLevels[0] = Mathf.Epsilon; // I'm disappointed I have to use additional distance to allow detect first frame culling event catch everything

            for (int i = 1; i < distances.Length + 1; i++)
                DistanceLevels[i] = distances[i - 1];

            // Additional distance level to be able detecting frustum ranges, instead of frustum with distance ranges combined
            DistanceLevels[DistanceLevels.Length - 1] = distances[distances.Length - 1] * 1.5f;

            CullingGroup = new CullingGroup { targetCamera = targetCamera };

            CullingSpheres = new BoundingSphere[MaxSlots];

            CullingGroup.SetBoundingSpheres(CullingSpheres);
            BoundingCount = 0;
            HighestIndex = -1;
            LowestFreeIndex = -1;
            CullingGroup.SetBoundingSphereCount(BoundingCount);

            CullingGroup.onStateChanged = CullingGroupStateChanged;

            CullingGroup.SetBoundingDistances(DistanceLevels);

            if (targetCamera) CullingGroup.SetDistanceReferencePoint(targetCamera.transform);
        }


        /// <summary>
        /// Setting new main camera
        /// </summary>
        public void SetNewCamera(Camera cam)
        {
            if (cam == null) return;

            CullingGroup.targetCamera = cam;
            CullingGroup.SetDistanceReferencePoint(cam.transform);
        }


        /// <summary>
        /// Returns true if list have free slots
        /// </summary>
        public bool AddOptimizer(Optimizer_Base optimizer)
        {
            if (!HaveFreeSlots) return false;
#if UNITY_EDITOR
#endif

            if (!optimizer.UseMultiShape) // Single bound
            {
                int nextId = HighestIndex + 1;

                CullingSpheres[nextId].position = optimizer.GetReferencePosition();
                CullingSpheres[nextId].radius = optimizer.DetectionRadius * Optimizer_Base.GetScaler(optimizer.transform);
                Optimizers[nextId] = optimizer;

                optimizer.AssignToContainer(this, nextId, ref CullingSpheres[nextId]);

                HighestIndex++;
                BoundingCount++;
                CullingGroup.SetBoundingSphereCount(BoundingCount);
                SlotsTaken++;
            }
            else // Assigning multi bounds for multi shape feature
            {
                int[] sphereIDs = new int[optimizer.Shapes.Count];

                for (int i = 0; i < optimizer.Shapes.Count; i++)
                {
                    int nextId = HighestIndex + 1;
                    sphereIDs[i] = nextId;

                    if (optimizer.Shapes[i].transform == null)
                    {
                        CullingSpheres[nextId].radius = optimizer.Shapes[i].radius * optimizer.DetectionRadius;
                        CullingSpheres[nextId].position = optimizer.transform.TransformPoint(optimizer.Shapes[i].position);
                    }
                    else
                    {
                        CullingSpheres[nextId].position = optimizer.Shapes[i].transform.TransformPoint(optimizer.Shapes[i].position);
                        CullingSpheres[nextId].radius = optimizer.Shapes[i].radius * optimizer.DetectionRadius;
                    }

                    Optimizers[nextId] = optimizer;


                    HighestIndex++;
                    BoundingCount++;
                    CullingGroup.SetBoundingSphereCount(BoundingCount);
                    SlotsTaken++;
                }

                optimizer.AssignToContainer(this, sphereIDs);
            }

            return true;
        }


        /// <summary>
        /// Remove optimizer from container and freeing slot for another optimizer
        /// </summary>
        public void RemoveOptimizer(Optimizer_Base optimizer)
        {
            if (Optimizers == null) return;

#if UNITY_EDITOR
            if (OptimizersManager.AppIsQuitting) return;
#endif

            if (!optimizer.UseMultiShape)
            {
                //Debug.Log("Next free = " + optimizer.ContainerSphereId);
                LowestFreeIndex = optimizer.ContainerSphereId;

                if (CullingGroup.targetCamera) CullingSpheres[LowestFreeIndex].position = CullingGroup.targetCamera.transform.position + new Vector3(99999, 99999, 99999);
                CullingSpheres[LowestFreeIndex].radius = 0f;// Mathf.Epsilon;

                Optimizers[LowestFreeIndex] = null;
                MoveStackOptimizerToFreeSlot();
                SlotsTaken--;
            }
            else // Multi shape set free sphere bounds
            {
                for (int i = 0; i < optimizer.ContainerSphereIds.Length; i++)
                {
                    LowestFreeIndex = optimizer.ContainerSphereIds[i];
                    if (CullingGroup.targetCamera) CullingSpheres[LowestFreeIndex].position = CullingGroup.targetCamera.transform.position + new Vector3(99999, 99999, 99999);
                    CullingSpheres[LowestFreeIndex].radius = 0f;
                    Optimizers[LowestFreeIndex] = null;
                    MoveStackOptimizerToFreeSlot();
                    SlotsTaken--;
                }
            }
        }


        private void MoveStackOptimizerToFreeSlot()
        {
            Optimizer_Base optimizerToMove = Optimizers[HighestIndex];
            Optimizers[HighestIndex] = null;
            HighestIndex--;
            BoundingCount--;

            if (optimizerToMove == null) return;
            int freeSlot = LowestFreeIndex;
            LowestFreeIndex = HighestIndex + 1;

            if (freeSlot < 0 || freeSlot >= CullingSpheres.Length) return;

            if (!optimizerToMove.UseMultiShape) // no multi shapes
            {
                CullingSpheres[freeSlot].position = optimizerToMove.GetReferencePosition();
                CullingSpheres[freeSlot].radius = optimizerToMove.DetectionRadius * Optimizer_Base.GetScaler(optimizerToMove.transform);
                Optimizers[freeSlot] = optimizerToMove;
                optimizerToMove.AssignToContainer(this, freeSlot, ref CullingSpheres[freeSlot]);
            }
            else // multi shape move in array
            {
                int movedSphereInId = -1;
                for (int i = 0; i < optimizerToMove.ContainerSphereIds.Length; i++)
                    if (optimizerToMove.ContainerSphereIds[i] == HighestIndex + 1)
                    {
                        movedSphereInId = i;
                        break;
                    }

                if (movedSphereInId != -1)
                {
                    optimizerToMove.ContainerSphereIds[movedSphereInId] = freeSlot;

                    if (optimizerToMove.Shapes[movedSphereInId].transform == null)
                        CullingSpheres[freeSlot].position = optimizerToMove.transform.TransformPoint(optimizerToMove.Shapes[movedSphereInId].position);
                    else
                        CullingSpheres[freeSlot].position = optimizerToMove.Shapes[movedSphereInId].transform.TransformPoint(optimizerToMove.Shapes[movedSphereInId].position);

                    CullingSpheres[freeSlot].radius = optimizerToMove.Shapes[movedSphereInId].radius * optimizerToMove.DetectionRadius;// * FOptimizer_Base.GetScaler(optimizerToMove.transform);

                    Optimizers[freeSlot] = optimizerToMove;
                    //optimizerToMove.AssignToContainer(this, freeSlot, ref CullingSpheres[freeSlot]);
                }
            }
        }


        /// <summary>
        /// Culling state changed for one culling sphere from container
        /// </summary>
        private void CullingGroupStateChanged(CullingGroupEvent cullingEvent)
        {
            if (Optimizers[cullingEvent.index] != null)
            {
                Optimizers[cullingEvent.index].CullingGroupStateChanged(cullingEvent);
            }
        }


        /// <summary>
        /// Cleaning culling group from memory
        /// </summary>
        public void Dispose()
        {
            CullingGroup.Dispose();
            CullingGroup = null;
            Optimizers = null;
        }


        /// <summary>
        /// Generating id for distance set
        /// </summary>
        public static int GetId(float[] distances)
        {
            float hashSource = distances.Length * 179;

            hashSource += distances[0];
            if (distances.Length > 1)
            {
                hashSource += distances[1];
                if (distances.Length > 2)
                {
                    hashSource += distances[2];
                    if (distances.Length > 3)
                    {
                        hashSource += distances[3];
                        if (distances.Length > 4)
                        {
                            hashSource += distances[4];
                            if (distances.Length > 5)
                            {
                                hashSource += distances[5];
                                if (distances.Length > 6)
                                {
                                    hashSource += distances[6];
                                    if (distances.Length > 7)
                                    {
                                        hashSource += distances[7];
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return hashSource.GetHashCode();
        }
    }
}
