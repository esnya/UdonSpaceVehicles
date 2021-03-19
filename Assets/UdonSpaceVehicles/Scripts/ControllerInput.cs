
using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV Controller Input")]
    [HelpMessage("Simulates joystick (Pitch, Yaw, Roll) or slider (X,Y,Z) input.")]
    public class ControllerInput : UdonSharpBehaviour
    {
        #region Public Variables
        [Popup("GetModeList")] public string mode = "Joystick";
        bool joystick, slider;

        [SectionHeader("VR Input")] public VRCPlayerApi.TrackingDataType targetHand = VRCPlayerApi.TrackingDataType.RightHand;
        string gripAxis;
        public float gripThreshold = 0.75f;
        [HelpBox("Maximum angle in degrees when joystick mode. Maximam distance in meters when slider mode.")] public Vector3 maxValue = Vector3.one * 30.0f;
        Vector3 inverseMaxValue;
        [SectionHeader("Desktop Input")] public string keymap = "w,s,e,q,a,d";

        [SectionHeader("UI")] [HelpBox("Updates float parameters. \"Pitch\", \"Yaw\" and \"Roll\" when joystick mode. \"Slider X\", \"Slider Y\" and \"Slider Z\" when slider mode.")] public Animator[] animators;

        [HideInInspector] public Vector3 input;
        #endregion

        #region Logics
        float Clamp11(float value)
        {
            return Mathf.Clamp(value, -1.0f, 1.0f);
        }
        float RemapRadianInput(float radian, float max)
        {
            return Clamp11(radian * Mathf.Rad2Deg / max);
        }

        Quaternion rotationOffset;
        void JoystickUpdate(bool isFirstFrame)
        {
            var controllerRotation = Quaternion.Inverse(transform.rotation) * Networking.LocalPlayer.GetTrackingData(targetHand).rotation;
            if (isFirstFrame)
            {
                rotationOffset = Quaternion.Inverse(controllerRotation);
            }
            else
            {
                var localRotation = controllerRotation * rotationOffset;
                var forward = localRotation * Vector3.forward;
                var up = localRotation * Vector3.up;

                input.x = RemapRadianInput(Mathf.Atan2(up.z, up.y), maxValue.x); // Pitch
                input.y = RemapRadianInput(Mathf.Atan2(forward.x, forward.z), maxValue.y); // Yaw
                input.z = RemapRadianInput(-Mathf.Atan2(up.x, up.y), maxValue.z); // Roll
            }
        }

        Vector3 positionOffset;
        void SliderUpdate(bool isFirstFrame)
        {
            var controllerPosition = transform.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(targetHand).position);
            if (isFirstFrame)
            {
                positionOffset = controllerPosition;
            }
            else
            {
                var rawInput = Vector3.Scale(controllerPosition - positionOffset, inverseMaxValue);

                input.x = Clamp11(rawInput.x);
                input.y = Clamp11(rawInput.y);
                input.z = Clamp11(rawInput.z);
            }
        }

        bool gripped;
        void VRUpdate()
        {
            if (Input.GetAxis(gripAxis) > gripThreshold)
            {
                if (joystick) JoystickUpdate(!gripped);
                if (slider) SliderUpdate(!gripped);
                gripped = true;
            }
            else
            {
                gripped = false;
                input = Vector3.zero;
            }
        }

        void DesktopUpdate()
        {
            if (inputMask[0]) input.x = (Input.GetKey(keys[0]) ? 1 : 0) + (Input.GetKey(keys[1]) ? -1 : 0);
            if (inputMask[1]) input.y = (Input.GetKey(keys[2]) ? 1 : 0) + (Input.GetKey(keys[3]) ? -1 : 0);
            if (inputMask[2]) input.z = (Input.GetKey(keys[4]) ? 1 : 0) + (Input.GetKey(keys[5]) ? -1 : 0);
        }
        #endregion

        #region Unity Events
        string[] parameterNames;
        string[] keys;
        bool[] inputMask;
        void Start()
        {
            joystick = mode == "Joystick";
            slider = mode == "Slider";

            inverseMaxValue.x = 1.0f / maxValue.x;
            inverseMaxValue.y = 1.0f / maxValue.y;
            inverseMaxValue.z = 1.0f / maxValue.z;

            if (joystick) parameterNames = new[] { "Pitch", "Yaw", "Roll" };
            if (slider) parameterNames = new[] { "Slider X", "Slider Y", "Slider Z" };

            if (targetHand == VRCPlayerApi.TrackingDataType.LeftHand) gripAxis = "Oculus_CrossPlatform_PrimaryHandTrigger";
            if (targetHand == VRCPlayerApi.TrackingDataType.RightHand) gripAxis = "Oculus_CrossPlatform_SecondaryHandTrigger";

            var keysTmp = keymap.Split(',');
            keys = new string[6];
            for (int i = 0; i < Mathf.Min(keysTmp.Length, 6); i++) keys[i] = keysTmp[i];

            inputMask = new bool[3];
            for (int i = 0; i < 3; i++) inputMask[i] = !string.IsNullOrEmpty(keys[i * 2]) && !string.IsNullOrEmpty(keys[i * 2 + 1]);
        }

        Vector3 prevInput;
        void LateUpdate()
        {
            if (!active) return;

            if (vr) VRUpdate();
            else DesktopUpdate();

            if (animators != null && input != prevInput)
            {
                foreach (var animator in animators)
                {
                    animator.SetFloat(parameterNames[0], input.x);
                    animator.SetFloat(parameterNames[1], input.y);
                    animator.SetFloat(parameterNames[2], input.z);
                }
                prevInput = input;
            }
        }
        #endregion

        #region Udon Events
        bool vr;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                vr = player.IsUserInVR();
                Log("Info", $"VR: {vr}");
            }
        }
        #endregion
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        string[] GetModeList() => new[] { "Joystick", "Slider" };

        [Button("Preset: Joystick")] private void PresetJoystick()
        {
            this.UpdateProxy();

            mode = "Joystick";
            targetHand = VRCPlayerApi.TrackingDataType.RightHand;
            maxValue = Vector3.one * 30.0f;
            keymap = "w,s,e,q,a,d";

            this.ApplyProxyModifications();
        }

        [Button("Preset: Throttle")] private void PresetThlottle()
        {
            this.UpdateProxy();

            mode = "Slider";
            targetHand = VRCPlayerApi.TrackingDataType.LeftHand;
            maxValue = Vector3.one * 0.2f;
            keymap = "l,h,j,k,left shift,left ctrl";

            this.ApplyProxyModifications();
        }
#endif


        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            active = false;
            Log("Info", "Deactivated");
        }
        #endregion

        #region Logger
        [SectionHeader("Udon Logger")] public UdonLogger logger;
        private void Log(string level, string message)
        {
            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

    }
}
