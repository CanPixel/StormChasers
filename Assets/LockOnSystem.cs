	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	public class LockOnSystem : MonoBehaviour
	{
		bool locked;

		public GameObject crossHair;
    
		Transform target;

		List<GameObject> targetsInGame = new List<GameObject>();
		List<GameObject> targetsInFrame = new List<GameObject>();

		void Start()
		{
     		   crossHair.SetActive(false);
		   GameObject[] allTargets = GameObject.FindGameObjectsWithTag("Target");
		   foreach (GameObject t in allTargets)
			{
			   targetsInGame.Add(t);
			}
		}

		void Update()
		{
		   if(targetsInGame.Count > 0)
		   {
	   			for (int i = 0; i < targetsInGame; i++)
				{
					Vector3 targetPos = Camera.main.WorldToViewImportPoint(targetsInGame[i].transform.position);
				
					if(targetPos.z > 0 && targetPos.x > 0 && targetPos.x < 1 && targetPos.y > 0 && targetPos.y < 1)
					{
						targetsInFrame.Add(targetsInGame[i]);
					}
					else if(targetsInFrame.Contains(targetsInGame[i]))
					{
						targetsInFrame.Remove(targetsInGame[i]);
					}
				}
		   }
		   if (!locked && targetsInFrame.Count>0)
		   {
			locked = true;
			crossHair.SetActive(true);
		   }
		   else if (locked)
		   {
			locked = false;
			crossHair.SetActive(false);
		   }
		   if (locked)
		   {
			target = targetsInFrame[0].transform;
			crossHair.transform.position = Camera.main.WorldToScreenPoint(target.position);
		   }
        
        
		}
	}
