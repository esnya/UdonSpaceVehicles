
using System;
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;

namespace UdonSpaceVehicles
{

    public class HUDTextDriver : UdonSharpBehaviour
    {

        #region Public Variables
        [Popup("GetModes")] public string mode;
        public TextMeshPro text;
        public Rigidbody target;
        [HideIf("HideAxis")] public Vector3 axis = Vector3.forward;
        public string suffix = "<size=75%>KMpH</size>";
        #endregion

        #region Logics
        private void UpdateText(float value)
        {
            text.text = $"{value * 3.6f:f2} {suffix}";
        }

        private void UpdateSpeed()
        {
            var velocity = target.velocity;
            var speed = velocity.magnitude * Mathf.Sign(Vector3.Dot(targetTransform.forward, velocity));
            UpdateText(speed);
        }

        private void UpdateVelocity()
        {
            var speed = Vector3.Dot(targetTransform.TransformDirection(axis), target.velocity);
            UpdateText(speed);
        }
        #endregion

        #region Unity Events
        private bool speed, velocity;
        private Transform targetTransform;
        private void Start()
        {
            targetTransform = target.transform;
            speed = mode == "Speed";
            velocity = mode == "Axis Speed";
        }
        private void Update()
        {
            if (!active) return;

            if (speed) UpdateSpeed();
            else if (velocity) UpdateVelocity();
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

            text.text = $"--.--  {suffix}";

            Log("Info", "Deactivated");
        }
        #endregion

        #region Logger
        [Space] [SectionHeader("Udon Logger")] public UdonLogger logger;
        private void Log(string level, string message)
        {
            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private string[] GetModes() => new[] {
            "Speed",
            "Axis Speed",
        };
        private bool HideAxis() => mode != "Axis Speed";
#endif
    }
}
