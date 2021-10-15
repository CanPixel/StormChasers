using System;
using FrameplaySDK.AdSpace;
using FrameplaySDK.General;
using UnityEngine;
using UnityEngine.Analytics;

namespace FrameplaySDK
{
    using Player = General.Player;

    public class Frameplay
    {
        #region Public API

        /// <summary>
        /// Has SDK session successfully started
        /// </summary>
        public static bool SessionStarted => FrameplayInternal.SessionStarted;

        /// <summary>
        /// Initialise Frameplay SDK Session
        /// The Data Asset will be loaded using Resources.Load
        /// </summary>
        /// <param name="player">Frameplay Player class optionally containing age, Gender, Language etc.</param>
        /// <param name="camera">Camera object to be registered for tracking</param>
        public static void StartSession(Player player, Camera camera)
        {
            FrameplayInternal.StartSession(player, camera);
        }

        /// <summary>
        /// Initialise Frameplay SDK Session
        /// Reference the Frameplay Data Asset here to load as part of the Scene or Prefab.
        /// If this property is left empty, the Data Asset will be loaded using Resources.Load
        /// </summary>
        /// <param name="dataAsset">Scriptable object loaded with the Scene or Prefab</param>
        /// <param name="player">Frameplay Player class optionally containing age, Gender, Language etc.</param>
        /// <param name="camera">Camera object to be registered for tracking</param>
        public static void StartSession(FrameplayDataAsset dataAsset, Player player, Camera camera)
        {
            FrameplayInternal.StartSession(dataAsset, player, camera);
        }

        #region Obsolete

        /// <summary>
        /// Initialise Frameplay SDK Session
        /// </summary>
        /// <param name="dataAsset">Scriptable object Frameplay asset</param>
        /// <param name="playerAge">If player data is present</param>
        /// <param name="playerGender">If player data is present</param>
        /// <param name="playerId">If player data is present</param>
        /// <param name="playerLanguage">If player data is present</param>
        /// <param name="allowDataCollection">Allow player data to be collected</param>
        /// <param name="sessionStarted">Optional Action called when the Session has started successfully</param>
        [Obsolete("Please use StartSession(FrameplayDataAsset dataAsset, Player player, Camera camera)"), System.Reflection.Obfuscation(ApplyToMembers = false)]
        public static void StartSession(FrameplayDataAsset dataAsset, int playerAge = -1, Gender playerGender = Gender.Unknown,
            string playerId = "", SystemLanguage playerLanguage = SystemLanguage.Unknown, Action sessionStarted = null, bool allowDataCollection = true)
        {
            FrameplayInternal.StartSession(dataAsset, playerAge, playerGender, playerId, playerLanguage, sessionStarted, allowDataCollection);
        }

        /// <summary>
        /// Initialise Frameplay SDK Session
        /// </summary>
        /// <param name="dataAsset">Scriptable object Frameplay asset</param>
        /// <param name="playerAge">If player data is present</param>
        /// <param name="playerGender">If player data is present</param>
        /// <param name="playerId">If player data is present</param>
        /// <param name="playerLanguage">If player data is present (Should be in ISO_639-1 https://en.wikipedia.org/wiki/ISO_639-1 )</param>
        /// <param name="allowDataCollection">Allow player data to be collected</param>
        /// <param name="sessionStarted">Optional Action called when the Session has started successfully</param>
        [Obsolete("Please use StartSession(FrameplayDataAsset dataAsset, Player player, Camera camera)"), System.Reflection.Obfuscation(ApplyToMembers = false)]
        public static void StartSession(FrameplayDataAsset dataAsset, int playerAge = -1, Gender playerGender = Gender.Unknown,
            string playerId = "", string playerLanguage = "", Action sessionStarted = null, bool allowDataCollection = true)
        {
            FrameplayInternal.StartSession(dataAsset, playerAge, playerGender, playerId, playerLanguage, sessionStarted, allowDataCollection);
        }

        #endregion Obsolete


        /// <summary>
        /// Shutdown Frameplay session
        /// </summary>
        public static void Shutdown()
        {
            FrameplayInternal.Shutdown();
        }

        /// <summary>
        ///  Register a Camera object for position tracking and Ad Space activation detection
        ///  Note: Only one camera can be tracked at any given time.
        ///  Creates a trigger collider component on the given object.
        /// </summary>
        /// <param name="camera">Camera Object</param>
        public static void RegisterCamera(Camera camera)
        {
            FrameplayInternal.RegisterCamera(camera);
        }

        /// <summary>
        /// Unregister the currently registered Camera object. Pauses all metrics and future advertisements from being loaded.
        /// </summary>
        public static void DeregisterCamera()
        {
            FrameplayInternal.DeregisterCamera();
        }

        /// <summary>
        /// Returns the currently registered camera. Null if none are registered.
        /// </summary>
        /// <returns></returns>
        public static Camera GetRegisteredCamera()
        {
            return FrameplayInternal.GetRegisteredCamera();
        }

        /// <summary>
        /// Returns true if a camera is currently registered.
        /// </summary>
        /// <returns></returns>
        public static bool IsCameraRegistered()
        {
            return FrameplayInternal.IsCameraRegistered();
        }

        /// <summary>
        /// Return array of all Active Ad Spaces.
        /// Only use this if the Frameplay session has actually started.
        /// This does not include Synchronized Ad Spaces.
        /// </summary>
        /// <returns>An array of Ad Spaces. Null (or an empty array) if
        /// the Session has not started</returns>
        public static FrameplayAdSpace[] GetAdSpacesInActiveScene()
        {
            return FrameplayInternal.GetAdSpacesInActiveScene();
        }

        /// <summary>
        /// Pause all AdSpaces in the active scene
        /// </summary>
        public static void PauseAdSpacesInActiveScene()
        {
            FrameplayInternal.PauseAdSpacesInActiveScene();
        }

        /// <summary>
        /// Unpause all AdSpaces in the active scene
        /// </summary>
        public static void UnpauseAdSpacesInActiveScene()
        {
            FrameplayInternal.UnpauseAdSpacesInActiveScene();
        }

        #endregion Public API
    }
}