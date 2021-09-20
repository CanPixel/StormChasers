// GENERATED AUTOMATICALLY FROM 'Assets/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""VehicleControls"",
            ""id"": ""e01c862e-2886-4f72-9f9d-fd11072e60dd"",
            ""actions"": [
                {
                    ""name"": ""RotationX"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d345a7fa-af93-4ef4-8e54-2e30b6cdade1"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotationY"",
                    ""type"": ""PassThrough"",
                    ""id"": ""6f601d07-089a-4a79-b81a-8b974740e942"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Brake"",
                    ""type"": ""PassThrough"",
                    ""id"": ""bbe92e8b-8b07-4e70-ae41-67ccbbdc6ad2"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Gas"",
                    ""type"": ""PassThrough"",
                    ""id"": ""771c0de9-bd73-4909-a8b2-ee573a5846da"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Steer"",
                    ""type"": ""PassThrough"",
                    ""id"": ""435c2bd3-4839-42c6-9eb9-66d5054465d8"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Drift"",
                    ""type"": ""PassThrough"",
                    ""id"": ""faba2312-e31c-44f5-a935-e2fe0df2ea26"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""32bf4ecb-75ae-4a5c-b926-4e1b3f260bc4"",
                    ""path"": ""<Mouse>/delta/x"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""RotationX"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""55befc5c-1e6a-4a66-bfe4-c5829a48f64a"",
                    ""path"": ""<Mouse>/delta/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""RotationY"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""951d765a-f5f3-4cdb-9951-48471712939b"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Brake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""bf00ada0-412f-4a09-a534-8eb7fa5d58ec"",
                    ""path"": ""<Gamepad>/leftTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Brake"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""e7e287e0-e206-4994-b0d3-3a123a6f636a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Gas"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""7873936d-7253-4d18-8402-f9c2d83ffec4"",
                    ""path"": ""<Gamepad>/rightTrigger"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Gas"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Arrows"",
                    ""id"": ""5999e4c3-25f2-4409-9bca-034849368044"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""ca510825-d070-44a5-87e4-80d7ae15ccd9"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""b6d9f61e-29a7-47a8-9a49-c5c36898dd19"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""WASD"",
                    ""id"": ""bae7a2bf-8705-40b2-8fe3-344dcfb9b2c2"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""71567267-f0ab-468e-9671-c7587c23617d"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""e445d7e6-b9a6-4b6d-b8c7-389b74755e54"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""3b572ab5-c38b-434b-9ead-c58886d5e026"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Steer"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""34a0864f-5f12-49f0-959c-00855fbfc80b"",
                    ""path"": ""<Gamepad>/leftStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""a28a0f93-e9be-40f0-99ff-d64a19d005f4"",
                    ""path"": ""<Gamepad>/leftStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Steer"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7f1016fb-38bc-44f8-a82d-284f9ac335ca"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Drift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6c9baff7-6111-4b6e-abc2-aabb4cd62405"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Drift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        },
        {
            ""name"": ""CameraControls"",
            ""id"": ""2c29fd32-6eb0-497a-b0b8-084854160b87"",
            ""actions"": [],
            ""bindings"": []
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        },
        {
            ""name"": ""Gamepad"",
            ""bindingGroup"": ""Gamepad"",
            ""devices"": [
                {
                    ""devicePath"": ""<Gamepad>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // VehicleControls
        m_VehicleControls = asset.FindActionMap("VehicleControls", throwIfNotFound: true);
        m_VehicleControls_RotationX = m_VehicleControls.FindAction("RotationX", throwIfNotFound: true);
        m_VehicleControls_RotationY = m_VehicleControls.FindAction("RotationY", throwIfNotFound: true);
        m_VehicleControls_Brake = m_VehicleControls.FindAction("Brake", throwIfNotFound: true);
        m_VehicleControls_Gas = m_VehicleControls.FindAction("Gas", throwIfNotFound: true);
        m_VehicleControls_Steer = m_VehicleControls.FindAction("Steer", throwIfNotFound: true);
        m_VehicleControls_Drift = m_VehicleControls.FindAction("Drift", throwIfNotFound: true);
        // CameraControls
        m_CameraControls = asset.FindActionMap("CameraControls", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // VehicleControls
    private readonly InputActionMap m_VehicleControls;
    private IVehicleControlsActions m_VehicleControlsActionsCallbackInterface;
    private readonly InputAction m_VehicleControls_RotationX;
    private readonly InputAction m_VehicleControls_RotationY;
    private readonly InputAction m_VehicleControls_Brake;
    private readonly InputAction m_VehicleControls_Gas;
    private readonly InputAction m_VehicleControls_Steer;
    private readonly InputAction m_VehicleControls_Drift;
    public struct VehicleControlsActions
    {
        private @Controls m_Wrapper;
        public VehicleControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @RotationX => m_Wrapper.m_VehicleControls_RotationX;
        public InputAction @RotationY => m_Wrapper.m_VehicleControls_RotationY;
        public InputAction @Brake => m_Wrapper.m_VehicleControls_Brake;
        public InputAction @Gas => m_Wrapper.m_VehicleControls_Gas;
        public InputAction @Steer => m_Wrapper.m_VehicleControls_Steer;
        public InputAction @Drift => m_Wrapper.m_VehicleControls_Drift;
        public InputActionMap Get() { return m_Wrapper.m_VehicleControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(VehicleControlsActions set) { return set.Get(); }
        public void SetCallbacks(IVehicleControlsActions instance)
        {
            if (m_Wrapper.m_VehicleControlsActionsCallbackInterface != null)
            {
                @RotationX.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationX;
                @RotationX.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationX;
                @RotationX.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationX;
                @RotationY.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationY;
                @RotationY.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationY;
                @RotationY.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnRotationY;
                @Brake.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnBrake;
                @Brake.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnBrake;
                @Brake.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnBrake;
                @Gas.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnGas;
                @Gas.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnGas;
                @Gas.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnGas;
                @Steer.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnSteer;
                @Steer.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnSteer;
                @Steer.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnSteer;
                @Drift.started -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnDrift;
                @Drift.performed -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnDrift;
                @Drift.canceled -= m_Wrapper.m_VehicleControlsActionsCallbackInterface.OnDrift;
            }
            m_Wrapper.m_VehicleControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
                @RotationX.started += instance.OnRotationX;
                @RotationX.performed += instance.OnRotationX;
                @RotationX.canceled += instance.OnRotationX;
                @RotationY.started += instance.OnRotationY;
                @RotationY.performed += instance.OnRotationY;
                @RotationY.canceled += instance.OnRotationY;
                @Brake.started += instance.OnBrake;
                @Brake.performed += instance.OnBrake;
                @Brake.canceled += instance.OnBrake;
                @Gas.started += instance.OnGas;
                @Gas.performed += instance.OnGas;
                @Gas.canceled += instance.OnGas;
                @Steer.started += instance.OnSteer;
                @Steer.performed += instance.OnSteer;
                @Steer.canceled += instance.OnSteer;
                @Drift.started += instance.OnDrift;
                @Drift.performed += instance.OnDrift;
                @Drift.canceled += instance.OnDrift;
            }
        }
    }
    public VehicleControlsActions @VehicleControls => new VehicleControlsActions(this);

    // CameraControls
    private readonly InputActionMap m_CameraControls;
    private ICameraControlsActions m_CameraControlsActionsCallbackInterface;
    public struct CameraControlsActions
    {
        private @Controls m_Wrapper;
        public CameraControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputActionMap Get() { return m_Wrapper.m_CameraControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(CameraControlsActions set) { return set.Get(); }
        public void SetCallbacks(ICameraControlsActions instance)
        {
            if (m_Wrapper.m_CameraControlsActionsCallbackInterface != null)
            {
            }
            m_Wrapper.m_CameraControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
            }
        }
    }
    public CameraControlsActions @CameraControls => new CameraControlsActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    private int m_GamepadSchemeIndex = -1;
    public InputControlScheme GamepadScheme
    {
        get
        {
            if (m_GamepadSchemeIndex == -1) m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
            return asset.controlSchemes[m_GamepadSchemeIndex];
        }
    }
    public interface IVehicleControlsActions
    {
        void OnRotationX(InputAction.CallbackContext context);
        void OnRotationY(InputAction.CallbackContext context);
        void OnBrake(InputAction.CallbackContext context);
        void OnGas(InputAction.CallbackContext context);
        void OnSteer(InputAction.CallbackContext context);
        void OnDrift(InputAction.CallbackContext context);
    }
    public interface ICameraControlsActions
    {
    }
}
