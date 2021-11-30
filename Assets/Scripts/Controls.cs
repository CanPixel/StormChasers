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
            ""name"": ""GameControls"",
            ""id"": ""e01c862e-2886-4f72-9f9d-fd11072e60dd"",
            ""actions"": [
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
                },
                {
                    ""name"": ""Looking"",
                    ""type"": ""PassThrough"",
                    ""id"": ""ba13ece7-257b-4717-8d6f-f35fa30272e8"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""PassThrough"",
                    ""id"": ""623908aa-1c38-4a17-89ea-77f9730a08f5"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Boost"",
                    ""type"": ""PassThrough"",
                    ""id"": ""17a962dd-7b13-4359-8351-2a6a467991b1"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraAim"",
                    ""type"": ""PassThrough"",
                    ""id"": ""749119d6-42f7-4c76-af70-17671390cc9b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraShoot"",
                    ""type"": ""PassThrough"",
                    ""id"": ""a69c8358-55cf-40d6-ac8d-6059f689c642"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Backlook"",
                    ""type"": ""PassThrough"",
                    ""id"": ""3df37bbb-3751-46e6-bd5c-d50a969eb685"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CycleFilter"",
                    ""type"": ""PassThrough"",
                    ""id"": ""0e382c57-c152-4fe0-903c-48afaa7d5abf"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""PhotoBook"",
                    ""type"": ""PassThrough"",
                    ""id"": ""e0bf7e2c-01ba-4551-99fb-69d1290e1962"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ChangeFocus"",
                    ""type"": ""PassThrough"",
                    ""id"": ""bfaeeb31-67e4-41ea-aa3a-18dafd92c436"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SkipPicture"",
                    ""type"": ""Button"",
                    ""id"": ""3b2e73f5-75b3-4c46-abe3-a44adb12fd19"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""ScrollPortfolio"",
                    ""type"": ""PassThrough"",
                    ""id"": ""1d6e52ef-fef5-458b-a002-e594840d137d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""DiscardPicture"",
                    ""type"": ""PassThrough"",
                    ""id"": ""ad710639-cddc-47c5-a278-153c2d4f8a87"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
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
                    ""id"": ""fe18a06f-00ce-4a23-addf-4d18cfbec538"",
                    ""path"": ""<Keyboard>/leftShift"",
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
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Drift"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""0ec00ded-249c-4b76-b7c0-e72ff60c207e"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Looking"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""97597b24-1679-449c-9ac4-b9135a1b9e4b"",
                    ""path"": ""<Gamepad>/rightStick/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2593c55b-9205-4293-96bf-fd5ec3dc23af"",
                    ""path"": ""<Gamepad>/rightStick/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""15147479-fd14-4f80-ac5a-97d9cf299242"",
                    ""path"": ""<Gamepad>/rightStick/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""e1c04a0f-3d36-4cb0-8113-cbdbfb6842b9"",
                    ""path"": ""<Gamepad>/rightStick/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Mouse"",
                    ""id"": ""259e0cde-d94d-4fb9-a7a3-db0338d86368"",
                    ""path"": ""2DVector(mode=2)"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Looking"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""75d5813f-654f-4e8f-af44-3b6325d4c4c1"",
                    ""path"": ""<Mouse>/delta/y"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0.02)"",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""4f37edf5-7030-4c73-867f-b304ca762e5d"",
                    ""path"": ""<Mouse>/delta/y"",
                    ""interactions"": """",
                    ""processors"": ""Invert,Scale(factor=0.02)"",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""a53a1763-43e0-4271-bb8e-d32f69d563fa"",
                    ""path"": ""<Mouse>/delta/x"",
                    ""interactions"": """",
                    ""processors"": ""Invert,Scale(factor=0.02)"",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""a2803dbd-88b3-407c-984a-5e5fc9b76ff0"",
                    ""path"": ""<Mouse>/delta/x"",
                    ""interactions"": """",
                    ""processors"": ""Scale(factor=0.02)"",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Looking"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""c0248e4f-cd1e-41b6-8dd3-9d9df61b0711"",
                    ""path"": ""<Gamepad>/buttonSouth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""dffb3e72-3b9e-4a54-9ec1-a92f5d884d2b"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""611aab00-b166-4c7b-9050-3e93f9cf3b31"",
                    ""path"": ""<Gamepad>/buttonEast"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Boost"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9f0d93fc-c2fb-49ba-9a50-f425c544423e"",
                    ""path"": ""<Keyboard>/leftCtrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Boost"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""92f4336a-9727-4d40-8e9b-bf6e1abcc985"",
                    ""path"": ""<Gamepad>/leftShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraAim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2f966395-3f7e-4469-bab9-d644ce4df1b8"",
                    ""path"": ""<Mouse>/rightButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CameraAim"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""129e30ef-eeb2-4033-904b-209c04b17499"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CameraShoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""126c6d71-0dcc-4171-b720-d97466979609"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CameraShoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0bd715ed-ddf9-474a-aff5-0ccaed6c1367"",
                    ""path"": ""<Gamepad>/rightStickPress"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""Backlook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0f1770de-e5f0-461f-94e0-7378ac930247"",
                    ""path"": ""<Mouse>/middleButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Backlook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""5a07f5ec-beeb-4ffd-a84a-2edf6aca4b3c"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CycleFilter"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""d0ea811e-f51f-4de3-9e3c-3f36b0ced15a"",
                    ""path"": ""<Gamepad>/buttonNorth"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""CycleFilter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""Mouse"",
                    ""id"": ""b3b8d831-4367-4cd4-bdce-03aae41bf386"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CycleFilter"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""343f7fe7-c6cf-47c0-9dcd-01861cd436d2"",
                    ""path"": """",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CycleFilter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""9f5ae378-c287-4e8e-a47f-b56cea357b33"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""CycleFilter"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""7a3e5dd7-c275-43df-96ea-4f3048333ef9"",
                    ""path"": ""<Gamepad>/select"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""PhotoBook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""4481135f-0a19-4841-99b7-9b09685c9122"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""PhotoBook"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""83e9e22f-b360-43c5-91b4-d1a1010355db"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ChangeFocus"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""8e554b4d-fec7-480d-b530-1a09fbd4eeb7"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ChangeFocus"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""90b54965-c466-4ff9-ac65-f6778176734a"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ChangeFocus"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""20d6d1c1-c932-4a6e-b711-e522c2865995"",
                    ""path"": ""<Gamepad>/rightShoulder"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""SkipPicture"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""Gamepad"",
                    ""id"": ""21cc3453-4eb4-478f-bebb-fc24c4bbad13"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""ScrollPortfolio"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""92386711-e15c-466b-b95c-1515634aa27e"",
                    ""path"": ""<Gamepad>/dpad/up"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ScrollPortfolio"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""2ff3785d-8bfa-4e99-ac3f-5f7041f863bc"",
                    ""path"": ""<Gamepad>/dpad/down"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ScrollPortfolio"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""5da5640c-86ac-4eb4-8936-39cbe67bb61e"",
                    ""path"": ""<Gamepad>/dpad/left"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ScrollPortfolio"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b0a26bd6-8cdf-45a8-8dfb-64fee3c02b77"",
                    ""path"": ""<Gamepad>/dpad/right"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""ScrollPortfolio"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""6049189c-66be-4e81-9896-350c22efe1ea"",
                    ""path"": ""<Gamepad>/buttonWest"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Gamepad"",
                    ""action"": ""DiscardPicture"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
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
        // GameControls
        m_GameControls = asset.FindActionMap("GameControls", throwIfNotFound: true);
        m_GameControls_Brake = m_GameControls.FindAction("Brake", throwIfNotFound: true);
        m_GameControls_Gas = m_GameControls.FindAction("Gas", throwIfNotFound: true);
        m_GameControls_Steer = m_GameControls.FindAction("Steer", throwIfNotFound: true);
        m_GameControls_Drift = m_GameControls.FindAction("Drift", throwIfNotFound: true);
        m_GameControls_Looking = m_GameControls.FindAction("Looking", throwIfNotFound: true);
        m_GameControls_Jump = m_GameControls.FindAction("Jump", throwIfNotFound: true);
        m_GameControls_Boost = m_GameControls.FindAction("Boost", throwIfNotFound: true);
        m_GameControls_CameraAim = m_GameControls.FindAction("CameraAim", throwIfNotFound: true);
        m_GameControls_CameraShoot = m_GameControls.FindAction("CameraShoot", throwIfNotFound: true);
        m_GameControls_Backlook = m_GameControls.FindAction("Backlook", throwIfNotFound: true);
        m_GameControls_CycleFilter = m_GameControls.FindAction("CycleFilter", throwIfNotFound: true);
        m_GameControls_PhotoBook = m_GameControls.FindAction("PhotoBook", throwIfNotFound: true);
        m_GameControls_ChangeFocus = m_GameControls.FindAction("ChangeFocus", throwIfNotFound: true);
        m_GameControls_SkipPicture = m_GameControls.FindAction("SkipPicture", throwIfNotFound: true);
        m_GameControls_ScrollPortfolio = m_GameControls.FindAction("ScrollPortfolio", throwIfNotFound: true);
        m_GameControls_DiscardPicture = m_GameControls.FindAction("DiscardPicture", throwIfNotFound: true);
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

    // GameControls
    private readonly InputActionMap m_GameControls;
    private IGameControlsActions m_GameControlsActionsCallbackInterface;
    private readonly InputAction m_GameControls_Brake;
    private readonly InputAction m_GameControls_Gas;
    private readonly InputAction m_GameControls_Steer;
    private readonly InputAction m_GameControls_Drift;
    private readonly InputAction m_GameControls_Looking;
    private readonly InputAction m_GameControls_Jump;
    private readonly InputAction m_GameControls_Boost;
    private readonly InputAction m_GameControls_CameraAim;
    private readonly InputAction m_GameControls_CameraShoot;
    private readonly InputAction m_GameControls_Backlook;
    private readonly InputAction m_GameControls_CycleFilter;
    private readonly InputAction m_GameControls_PhotoBook;
    private readonly InputAction m_GameControls_ChangeFocus;
    private readonly InputAction m_GameControls_SkipPicture;
    private readonly InputAction m_GameControls_ScrollPortfolio;
    private readonly InputAction m_GameControls_DiscardPicture;
    public struct GameControlsActions
    {
        private @Controls m_Wrapper;
        public GameControlsActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Brake => m_Wrapper.m_GameControls_Brake;
        public InputAction @Gas => m_Wrapper.m_GameControls_Gas;
        public InputAction @Steer => m_Wrapper.m_GameControls_Steer;
        public InputAction @Drift => m_Wrapper.m_GameControls_Drift;
        public InputAction @Looking => m_Wrapper.m_GameControls_Looking;
        public InputAction @Jump => m_Wrapper.m_GameControls_Jump;
        public InputAction @Boost => m_Wrapper.m_GameControls_Boost;
        public InputAction @CameraAim => m_Wrapper.m_GameControls_CameraAim;
        public InputAction @CameraShoot => m_Wrapper.m_GameControls_CameraShoot;
        public InputAction @Backlook => m_Wrapper.m_GameControls_Backlook;
        public InputAction @CycleFilter => m_Wrapper.m_GameControls_CycleFilter;
        public InputAction @PhotoBook => m_Wrapper.m_GameControls_PhotoBook;
        public InputAction @ChangeFocus => m_Wrapper.m_GameControls_ChangeFocus;
        public InputAction @SkipPicture => m_Wrapper.m_GameControls_SkipPicture;
        public InputAction @ScrollPortfolio => m_Wrapper.m_GameControls_ScrollPortfolio;
        public InputAction @DiscardPicture => m_Wrapper.m_GameControls_DiscardPicture;
        public InputActionMap Get() { return m_Wrapper.m_GameControls; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(GameControlsActions set) { return set.Get(); }
        public void SetCallbacks(IGameControlsActions instance)
        {
            if (m_Wrapper.m_GameControlsActionsCallbackInterface != null)
            {
                @Brake.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBrake;
                @Brake.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBrake;
                @Brake.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBrake;
                @Gas.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnGas;
                @Gas.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnGas;
                @Gas.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnGas;
                @Steer.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSteer;
                @Steer.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSteer;
                @Steer.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSteer;
                @Drift.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDrift;
                @Drift.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDrift;
                @Drift.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDrift;
                @Looking.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnLooking;
                @Looking.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnLooking;
                @Looking.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnLooking;
                @Jump.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnJump;
                @Boost.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBoost;
                @Boost.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBoost;
                @Boost.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBoost;
                @CameraAim.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraAim;
                @CameraAim.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraAim;
                @CameraAim.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraAim;
                @CameraShoot.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraShoot;
                @CameraShoot.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraShoot;
                @CameraShoot.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCameraShoot;
                @Backlook.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBacklook;
                @Backlook.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBacklook;
                @Backlook.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnBacklook;
                @CycleFilter.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCycleFilter;
                @CycleFilter.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCycleFilter;
                @CycleFilter.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnCycleFilter;
                @PhotoBook.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnPhotoBook;
                @PhotoBook.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnPhotoBook;
                @PhotoBook.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnPhotoBook;
                @ChangeFocus.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnChangeFocus;
                @ChangeFocus.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnChangeFocus;
                @SkipPicture.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSkipPicture;
                @SkipPicture.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSkipPicture;
                @SkipPicture.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnSkipPicture;
                @ScrollPortfolio.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnScrollPortfolio;
                @ScrollPortfolio.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnScrollPortfolio;
                @ScrollPortfolio.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnScrollPortfolio;
                @DiscardPicture.started -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDiscardPicture;
                @DiscardPicture.performed -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDiscardPicture;
                @DiscardPicture.canceled -= m_Wrapper.m_GameControlsActionsCallbackInterface.OnDiscardPicture;
            }
            m_Wrapper.m_GameControlsActionsCallbackInterface = instance;
            if (instance != null)
            {
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
                @Looking.started += instance.OnLooking;
                @Looking.performed += instance.OnLooking;
                @Looking.canceled += instance.OnLooking;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Boost.started += instance.OnBoost;
                @Boost.performed += instance.OnBoost;
                @Boost.canceled += instance.OnBoost;
                @CameraAim.started += instance.OnCameraAim;
                @CameraAim.performed += instance.OnCameraAim;
                @CameraAim.canceled += instance.OnCameraAim;
                @CameraShoot.started += instance.OnCameraShoot;
                @CameraShoot.performed += instance.OnCameraShoot;
                @CameraShoot.canceled += instance.OnCameraShoot;
                @Backlook.started += instance.OnBacklook;
                @Backlook.performed += instance.OnBacklook;
                @Backlook.canceled += instance.OnBacklook;
                @CycleFilter.started += instance.OnCycleFilter;
                @CycleFilter.performed += instance.OnCycleFilter;
                @CycleFilter.canceled += instance.OnCycleFilter;
                @PhotoBook.started += instance.OnPhotoBook;
                @PhotoBook.performed += instance.OnPhotoBook;
                @PhotoBook.canceled += instance.OnPhotoBook;
                @ChangeFocus.started += instance.OnChangeFocus;
                @ChangeFocus.performed += instance.OnChangeFocus;
                @ChangeFocus.canceled += instance.OnChangeFocus;
                @SkipPicture.started += instance.OnSkipPicture;
                @SkipPicture.performed += instance.OnSkipPicture;
                @SkipPicture.canceled += instance.OnSkipPicture;
                @ScrollPortfolio.started += instance.OnScrollPortfolio;
                @ScrollPortfolio.performed += instance.OnScrollPortfolio;
                @ScrollPortfolio.canceled += instance.OnScrollPortfolio;
                @DiscardPicture.started += instance.OnDiscardPicture;
                @DiscardPicture.performed += instance.OnDiscardPicture;
                @DiscardPicture.canceled += instance.OnDiscardPicture;
            }
        }
    }
    public GameControlsActions @GameControls => new GameControlsActions(this);
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
    public interface IGameControlsActions
    {
        void OnBrake(InputAction.CallbackContext context);
        void OnGas(InputAction.CallbackContext context);
        void OnSteer(InputAction.CallbackContext context);
        void OnDrift(InputAction.CallbackContext context);
        void OnLooking(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnBoost(InputAction.CallbackContext context);
        void OnCameraAim(InputAction.CallbackContext context);
        void OnCameraShoot(InputAction.CallbackContext context);
        void OnBacklook(InputAction.CallbackContext context);
        void OnCycleFilter(InputAction.CallbackContext context);
        void OnPhotoBook(InputAction.CallbackContext context);
        void OnChangeFocus(InputAction.CallbackContext context);
        void OnSkipPicture(InputAction.CallbackContext context);
        void OnScrollPortfolio(InputAction.CallbackContext context);
        void OnDiscardPicture(InputAction.CallbackContext context);
    }
}
