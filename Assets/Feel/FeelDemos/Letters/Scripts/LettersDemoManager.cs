using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;

namespace MoreMountains.Feel
{
    /// <summary>
    /// A manager used to pilot Feel's Letters demo scene
    /// It detects input, and plays corresponding feedbacks when needed
    /// </summary>
    public class LettersDemoManager : MonoBehaviour
    {
        [Header("Input")]
        /// a key used to activate the F letter 
        public KeyCode KeyF;
        /// another key used to activate the F letter
        public KeyCode KeyFAlt;
        /// a key used to activate the first E letter
        public KeyCode KeyE1;
        /// another key used to activate the first E letter
        public KeyCode KeyE1Alt;
        /// a key used to activate the second E letter
        public KeyCode KeyE2;
        /// another key used to activate the second E letter
        public KeyCode KeyE2Alt;
        /// a key used to activate the L letter
        public KeyCode KeyL;
        /// another key used to activate the L letter
        public KeyCode KeyLAlt;

        [Header("Feedbacks")]
        /// a feedback to play when the F letter gets activated
        public MMFeedbacks FeedbackF;
        /// a feedback to play when the first E letter gets activated
        public MMFeedbacks FeedbackE1;
        /// a feedback to play when the second E letter gets activated
        public MMFeedbacks FeedbackE2;
        /// a feedback to play when the L letter gets activated
        public MMFeedbacks FeedbackL;

        /// <summary>
        /// On Update we look for input
        /// </summary>
        protected virtual void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Every frame, looks for input, and activates the corresponding letter if needed
        /// </summary>
        protected virtual void HandleInput()
        {
            if (Input.GetKeyDown(KeyF) || Input.GetKeyDown(KeyFAlt))
            {
                PlayF();
            }
            if (Input.GetKeyDown(KeyE1) || Input.GetKeyDown(KeyE1Alt))
            {
                PlayE1();
            }
            if (Input.GetKeyDown(KeyE2) || Input.GetKeyDown(KeyE2Alt))
            {
                PlayE2();
            }
            if (Input.GetKeyDown(KeyL) || Input.GetKeyDown(KeyLAlt))
            {
                PlayL();
            }
            
            if ( Input.GetMouseButtonDown (0))
            { 
                RaycastHit hit; 
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); 
                if ( Physics.Raycast (ray,out hit,100.0f)) 
                {
                    switch (hit.transform.name)
                    {
                        case "ColliderF":
                            PlayF();
                            break;
                        case "ColliderE1":
                            PlayE1();
                            break;
                        case "ColliderE2":
                            PlayE2();
                            break;
                        case "ColliderL":
                            PlayL();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Activates the letter F
        /// </summary>
        protected virtual void PlayF()
        {
            FeedbackF?.PlayFeedbacks();
        }

        /// <summary>
        /// Activates the first E letter
        /// </summary>
        protected virtual void PlayE1()
        {
            FeedbackE1?.PlayFeedbacks();
        }
        
        /// <summary>
        /// Activates the second E letter
        /// </summary>
        protected virtual void PlayE2()
        {
            FeedbackE2?.PlayFeedbacks();
        }
        
        /// <summary>
        /// Activates the letter L
        /// </summary>
        protected virtual void PlayL()
        {
            FeedbackL?.PlayFeedbacks();
        }
    }
}

