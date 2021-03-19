
using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using JetBrains.Annotations;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV HUD Text Driver")][OnAfterEditor("OnAfterEditor")]
    public class HUDTextDriver : UdonSharpBehaviour
    {

        #region Public Variables
        [SectionHeader("References")] public TextMeshPro text;
        public Rigidbody target;
        [SectionHeader("Mode")][Popup("GetModes")] public string mode;
        [HideIf("HideForward")] public bool signed = true;
        [HideIf("HideForward")] public Vector3 forward = Vector3.forward;
        [HideIf("HideForward")] public Vector3 velocityBias;
        [HideIf("HIdeForward")] public Vector3 axisScale = Vector3.one;
        [HideIf("HideAxis")] public Vector3 axis = Vector3.forward;
        [HideIf("HidePositionOffset")] public Vector3 positionOffset;
        [SectionHeader("Format")] public string prefix = "";
        public string suffix = " <size=75%>{}</size>";
        #endregion

        #region Logics
        private const int Speed = 0, Velocity = 1, Altitude = 2, Gravity = 3;
        private string[] GetModes() => new[] {
            "Speed",
            "Axis Speed",
            "Altitude",
            "Gravity",
        };
        private string format = "f";
        readonly private string[] units = {
            "KMpH",
            "KMpH",
            "m",
            "G",
        };
        readonly private string[] formats = {
            "f2",
            "f2",
            "f0",
            "f2",
        };
        private void Initialize()
        {
            var modes = GetModes();
            for (int i = 0; i < modes.Length; i++) {
                if (mode == modes[i]) {
                    _mode = i;
                    break;
                }
            }
            _suffix = suffix.Replace("{}", units[_mode]);
            format = formats[_mode];
            value = -999.99f;
            UpdateText();
        }

        private void UpdateText()
        {
            text.text = $"{prefix}{value.ToString(format)}{_suffix}";
        }
        #endregion

        #region Unity Events
        private int _mode;
        private Transform targetTransform;
        private string _suffix;
        private void Start()
        {
            targetTransform = target.transform;
            Initialize();

            Log("Info", "Initialized");
        }

        private float value;
        private Vector3 prevVelocity;
        private void FixedUpdate()
        {
            if (!active) return;

            switch (_mode)
            {
                case Speed:
                    var velocity = Vector3.Scale(target.velocity + velocityBias, axisScale);
                    value = velocity.magnitude * (signed ? Mathf.Sign(Vector3.Dot(targetTransform.forward, velocity)) : 1.0f) * 3.6f;
                    break;
                case Velocity:
                    value = Vector3.Dot(targetTransform.TransformDirection(axis), target.velocity) * 3.6f;
                    break;
                case Altitude:
                    value = targetTransform.position.y + positionOffset.y;
                    break;
                case Gravity:
                    var a = target.velocity - prevVelocity;
                    value = a.magnitude * Mathf.Sign(a.y) / Time.fixedDeltaTime / 9.8f;
                    prevVelocity = target.velocity;
                    break;
            }
        }

        private void Update()
        {
            if (active) UpdateText();
         }
        #endregion

        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            active = false;
            Initialize();
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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private bool HideForward() => mode != "Speed";
        private bool HideAxis() => mode != "Axis Speed";
        private bool HidePositionOffset() => mode != "Altitude";

        private void OnAfterEditor() {
            this.UpdateProxy();
            Initialize();
        }
#endif
    }
}
