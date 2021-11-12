using System.Collections.Generic;
using UnityEngine;

namespace com.zibra.liquid.Samples
{
    public class CouplingManager : MonoBehaviour
    {
        [SerializeField]
        private Transform doll;
        private List<Vector3> defaultPositions;
        private List<Quaternion> defaultRotations;

        protected void Start()
        {
            defaultPositions = new List<Vector3>();
            defaultRotations = new List<Quaternion>();
            defaultPositions.Add(doll.position);
            defaultRotations.Add(doll.rotation);
            foreach (Transform child in doll)
            {
                defaultPositions.Add(child.position);
                defaultRotations.Add(child.rotation);
            }
        }

        protected void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ResetDoll();
            }
        }
        public void ResetDoll()
        {
            doll.gameObject.SetActive(true);
            doll.rotation = defaultRotations[0];
            doll.position = defaultPositions[0];
            for (var i = 0; i < doll.childCount; i++)
            {
                var child = doll.GetChild(i);
                child.position = defaultPositions[i];
                child.rotation = defaultRotations[i];
            }
        }
    }
}
