#if ZIBRA_LIQUID_PAID_VERSION

using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace com.zibra.liquid.Editor.SDFObjects
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class ZibraServerAuthenticationManager
    {
        private const string BASE_URL = "http://gen.zibra.ai:5000/";
        private string UserHardwareID = "";
        private string UserID = "";
        UnityWebRequestAsyncOperation request;

        public string PluginLicenseKey = "";
        public bool IsLicenseKeyValid = false;
        public bool IsKeyRequestInProgress = false;
        public string GenerationURL = "";
        public string ErrorText = "";
        public bool IsInitialized = false;
        public bool bNeedRefresh = false;

#if UNITY_EDITOR
        private class LicenseKeyResponse
        {
            public string api_key;
        }

        private static ZibraServerAuthenticationManager instance = null;

        public string GetUserID()
        {
            if (UserID == "")
            {
                CollectUserInfo();
            }

            return UserID;
        }

        [RuntimeInitializeOnLoadMethod]
        public static ZibraServerAuthenticationManager GetInstance()
        {
            if (instance == null)
            {
                instance = new ZibraServerAuthenticationManager();
                instance.Initialize();
            }
            // Re-send request if for some reason it failed at first
            else if (!instance.IsInitialized)
            {
                instance.Initialize();
            }

            return instance;
        }

        private string GetEditorPrefsLicenceKey()
        {
            if (EditorPrefs.HasKey("ZibraLiquidLicenceKey"))
            {
                return EditorPrefs.GetString("ZibraLiquidLicenceKey");
            }

            return "";
        }

        private string GetValidationURL(string key)
        {
            return BASE_URL + "api/apiKey?api_key=" + key + "&type=validation";
        }

        private string GetRenewalKeyURL()
        {
            return BASE_URL + "api/apiKey?user_id=" + UserID + "&type=renew";
        }

        void SendRequest(string key)
        {
            string requestURL;
            if (key != "")
            {
                // check if key is valid
                requestURL = GetValidationURL(key);
            }
            else if (UserID != "")
            {
                // request new key based on User and Hardware ID
                requestURL = GetRenewalKeyURL();
            }
            else
            {
                return;
            }

            request = UnityWebRequest.Get(requestURL).SendWebRequest();
            request.completed += UpdateKeyRequest;
            IsKeyRequestInProgress = true;
        }

        public void Initialize()
        {
            if (IsKeyRequestInProgress)
                return;

            CollectUserInfo();
            PluginLicenseKey = GetEditorPrefsLicenceKey();
            SendRequest(PluginLicenseKey);
        }

        void RefreshAboutTab()
        {
            if (!bNeedRefresh)
                bNeedRefresh = true;
        }

        public void UpdateKeyRequest(AsyncOperation obj)
        {
            if (IsLicenseKeyValid)
                return;

            if (request.isDone)
            {
                IsInitialized = true;
                IsKeyRequestInProgress = false;
                var result = request.webRequest.downloadHandler.text;
                if (result != null && !request.webRequest.isHttpError && !request.webRequest.isNetworkError)
                {
                    PluginLicenseKey = JsonUtility.FromJson<LicenseKeyResponse>(result).api_key;
                    if (PluginLicenseKey != "")
                    {
                        IsLicenseKeyValid = true;
                        SetLicenceKey(PluginLicenseKey);
                        // populate server request URL if everything is fine
                        GenerationURL = CreateGenerationRequestURL();
                    }
                    else if (request.webRequest.isHttpError || request.webRequest.isNetworkError)
                    {
                        Debug.Log(request.webRequest.error);
                    }
                    RefreshAboutTab();
                    // empty response meaning User is not registered
                    ErrorText = "To use this feature please register Zibra Liquid and get authentication key." +
                                Environment.NewLine + "See Zibra AI Liquids - Info tab in main menu.";
                }
            }
            return;
        }

        public void RegisterKey(string key)
        {
            IsLicenseKeyValid = false;
            IsKeyRequestInProgress = false;
            SendRequest(key);
        }

        private void SetLicenceKey(string key)
        {
            EditorPrefs.SetString("ZibraLiquidLicenceKey", key);
        }

        public void CollectUserInfo()
        {
            UserHardwareID = SystemInfo.deviceUniqueIdentifier;

            var assembly = Assembly.GetAssembly(typeof(UnityEditor.EditorWindow));
            var uc = assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false,
                                             BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
            // Cache type of UnityConnect.
            if (uc == null)
            {
                ErrorText = "Could not create UnityEditor.Connect.UnityConnect. Try later.";
                Debug.Log(ErrorText);
                IsInitialized = false;
                return;
            }

            var t = uc.GetType();
            // Get user info object from UnityConnect.
            var userInfo = t.GetProperty("userInfo")?.GetValue(uc, null);
            // Retrieve user id from user info.
            if (userInfo == null)
            {
                ErrorText = "Could not retrieve User Info from UnityEditor.Connect.UnityConnect. Try later.";
                Debug.Log(ErrorText);
                IsInitialized = false;
                return;
            }

            var userInfoType = userInfo.GetType();
            var isValid = userInfoType.GetProperty("valid");
            if (isValid == null || isValid.GetValue(userInfo, null).Equals(false))
            {
                ErrorText = "User authentication error. Try later.";
                Debug.Log(ErrorText);
                IsInitialized = false;
                return;
            }

            UserID = userInfoType.GetProperty("userId")?.GetValue(userInfo, null) as string;
            if (UserID == "")
            {
                ErrorText =
                    "Can't get Unity account information. Please ensure you are logged in to Unity Hub, and that Unity Editor was launched with Unity Hub.";
                Debug.Log(ErrorText);
                IsInitialized = false;
                return;
            }
        }

        private string CreateGenerationRequestURL()
        {
            if (GenerationURL != "")
                return GenerationURL;

            GenerationURL = BASE_URL + "api/unity/compute?";

            if (UserID != "")
            {
                GenerationURL += "&user_id=" + UserID;
            }

            if (UserHardwareID != "")
            {
                GenerationURL += "&hardware_id=" + UserHardwareID;
            }

            if (PluginLicenseKey != "")
            {
                GenerationURL += "&api_key=" + PluginLicenseKey;
            }

            return GenerationURL;
        }
#endif
    }
}
#endif