using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MoreMountains.Feel
{
    /// <summary>
    /// A class used to handle the characters in Feel's Tactical demo scene, detects input,
    /// shoots while a button is pressed, stops shooting when released, handles reload
    /// </summary>
    public class Tactical : MonoBehaviour
    {
        [Header("Input")]
        /// a key to use to shoot
        [Tooltip("a key to use to shoot")]
        public KeyCode ActionKey = KeyCode.Space;
        /// a secondary key to use to shoot
        [Tooltip("a secondary key to use to shoot")]
        public KeyCode ActionKeyAlt = KeyCode.Joystick1Button0;
        
        [Header("Cooldown")]
        /// a duration, in seconds, between two shots, during which shots are prevented
        [Tooltip("a duration, in seconds, between two shots, during which shots are prevented")]
        public float CooldownDuration = 0.1f;

        [Header("Bindings")] 
        /// the position of the shot's impact
        [Tooltip("the position of the shot's impact")]
        public Transform ImpactPosition;
        
        [Header("Feedbacks")]
        /// a feedback to call when shooting
        [Tooltip("a feedback to call when shooting")]
        public MMFeedbacks ShootFeedback;
        /// a feedback to call when shooting stops
        [Tooltip("a feedback to call when shooting stops")]
        public MMFeedbacks ShootStopFeedback;
        /// a feedback to call when a reload happens
        [Tooltip("a feedback to call when a reload happens")]
        public MMFeedbacks ReloadFeedback;

        protected float _lastJumpStartedAt = -100f;
        protected int _magazine = 15;
        
         /// <summary>
        /// On Update we look for input
        /// </summary>
        protected virtual void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// Detects input
        /// </summary>
        protected virtual void HandleInput()
        {
            if (Input.GetKey(ActionKey) || Input.GetKey(ActionKeyAlt) || Input.GetMouseButton(0))
            {
                Shoot();
            }
            if (Input.GetKeyUp(ActionKey) || Input.GetKeyUp(ActionKeyAlt) || Input.GetMouseButtonUp(0))
            {
                ShootStop();
            }
        }

        /// <summary>
        /// Shoots if possible
        /// </summary>
        protected virtual void Shoot()
        {
            if (Time.time - _lastJumpStartedAt > CooldownDuration)
            {
                float damage = Random.Range(20, 200);
                ShootFeedback?.PlayFeedbacks(ImpactPosition.position, damage);
                _lastJumpStartedAt = Time.time;
                _magazine--;
            }         
        }

        /// <summary>
        /// Stops shooting
        /// </summary>
        protected virtual void ShootStop()
        {
            ShootStopFeedback?.PlayFeedbacks();
            if (_magazine < 0)
            {
                ReloadFeedback?.PlayFeedbacks();
                _magazine = 15;
            }
        }
    }
}
