using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using com.zibra.liquid.Foundation.UIElements;
using com.zibra.liquid.Editor.SDFObjects;

#if UNITY_2019_4_OR_NEWER
namespace com.zibra.liquid.Plugins.Editor
{
    internal class AboutTab : BaseTab
    {
        private VisualElement m_RegistrationBlock;

#if ZIBRA_LIQUID_PAID_VERSION
        private TextField m_AuthKeyInputField;
        private Button m_CheckAuthKeyBtn;
        private Button m_RegisterAuthKeyBtn;
        private Label m_InvalidKeyLabel;
        private Label m_RegisteredKeyLabel;
#endif
        public AboutTab() : base($"{ZibraAIPackage.UIToolkitPath}/AboutTab/AboutTab")
        {
            m_RegistrationBlock = this.Q<SettingsBlock>("registrationBlock");
#if ZIBRA_LIQUID_PAID_VERSION
            m_AuthKeyInputField = this.Q<TextField>("authKeyInputField");
            m_CheckAuthKeyBtn = this.Q<Button>("validateAuthKeyBtn");
            m_RegisterAuthKeyBtn = this.Q<Button>("registerKeyBtn");
            m_InvalidKeyLabel = this.Q<Label>("invalidKeyLabel");
            m_RegisteredKeyLabel = this.Q<Label>("registeredKeyLabel");

            ZibraServerAuthenticationManager.GetInstance().Initialize();
            m_RegisterAuthKeyBtn.clicked += OnRegisterAuthKeyBtnOnClickedHandler;
            m_AuthKeyInputField.value = ZibraServerAuthenticationManager.GetInstance().PluginLicenseKey;
            m_CheckAuthKeyBtn.clicked += OnAuthKeyBtnOnClickedHandler;
            // Hide if key is valid.
            if (ZibraServerAuthenticationManager.GetInstance().IsLicenseKeyValid)
            {
                m_RegisterAuthKeyBtn.style.display = DisplayStyle.None;
                m_CheckAuthKeyBtn.style.display = DisplayStyle.None;
                m_AuthKeyInputField.style.display = DisplayStyle.None;
                m_InvalidKeyLabel.style.display = DisplayStyle.None;
                m_RegisteredKeyLabel.style.display = DisplayStyle.Flex;
            }
            else
            {
                m_RegisteredKeyLabel.style.display = DisplayStyle.None;
                m_InvalidKeyLabel.style.display = DisplayStyle.Flex;
            }
#else
            m_RegistrationBlock.style.display = DisplayStyle.None;
#endif
        }

#if ZIBRA_LIQUID_PAID_VERSION
        private void OnRegisterAuthKeyBtnOnClickedHandler()
        {
            Application.OpenURL("https://zibra.ai/liquids/registration");
        }

        private void OnAuthKeyBtnOnClickedHandler()
        {
            string key = m_AuthKeyInputField.text.Trim();

            if (ZibraServerAuthenticationManager.GetInstance().GetUserID() != "")
            {
                if (key.Length == 36)
                {
                    ZibraServerAuthenticationManager.GetInstance().RegisterKey(m_AuthKeyInputField.text);
                    m_InvalidKeyLabel.style.display = DisplayStyle.None;
                    m_RegisteredKeyLabel.style.display = DisplayStyle.None;
                }
                else
                {
                    EditorUtility.DisplayDialog("Zibra Liquid Key Error", "Incorrect key format.", "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Zibra Liquid Error",
                                            "User Info is absent.Please log in to Unity with your Unity ID.", "Ok");
                Debug.Log("User Info is absent.Please log in to Unity with your Unity ID.");
            }
        }
#endif
    }
}
#endif