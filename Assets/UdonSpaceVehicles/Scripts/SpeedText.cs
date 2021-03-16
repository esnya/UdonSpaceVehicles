
using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;

namespace UdonSpaceVehicles
{

    public class SpeedText : UdonSharpBehaviour
    {
        #region Public Variables
        public TextMeshPro text;
        public Rigidbody target;
        public string suffix = "<size=75%>KMpH</size>";
        #endregion

        #region Unity Events
        private void Update()
        {
            if (!active) return;

            var velocity = target.velocity;
            var speed = velocity.magnitude * 3.6f * Mathf.Sign(Vector3.Dot(target.transform.forward, velocity));
            text.text = $"{speed:f2} {suffix}";
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
    }
}
