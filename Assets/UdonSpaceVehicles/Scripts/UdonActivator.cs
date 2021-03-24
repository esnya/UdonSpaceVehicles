
using JetBrains.Annotations;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Udon Activator")]
    public class UdonActivator : UdonSharpBehaviour
    {
        #region Public Variables
        public bool autoIncludeChildren;
        public bool takeOwnership;
        public GameObject[] targets = { };
        [HelpBox("Update bool parameter \"Active\" locally.")] public Animator[] animators = { };
        #endregion

        #region Logics
        private void BroadcastActivation(bool active)
        {
            var localPlayer = Networking.LocalPlayer;

            foreach (var obj in targetUdons)
            {
                if (obj == null) continue;

                var udon = (UdonBehaviour)obj;
                if (udon == null || udon.gameObject == gameObject) continue;

                if (active && takeOwnership) Networking.SetOwner(localPlayer, udon.gameObject);

                udon.SendCustomEvent(active ? "Activate" : "Deactivate");
            }

            foreach (var animator in animators) animator.SetBool("Active", active);
        }
        #endregion

        #region Unity Events
        private Component[] targetUdons = {};
        private void Start()
        {
            var children = autoIncludeChildren ? GetComponentsInChildren(typeof(UdonBehaviour), true) : new Component[0];

            targetUdons = new Component[targets.Length + children.Length];
            for (int i = 0; i < targets.Length; i++) targetUdons[i] = targets[i].GetComponent(typeof(UdonBehaviour));
            for (int i = 0; i < children.Length; i++) targetUdons[i + targets.Length] = children[i];

            Log("Info", $"Initialized with {targetUdons.Length} components");
        }
        #endregion

        #region Udon Events
        #endregion

        #region Activatable
        public void Activate()
        {
            Log("Info", $"Activate {targetUdons.Length} components");
            BroadcastActivation(true);
            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            Log("Info", $"Deactivate {targetUdons.Length} components");
            BroadcastActivation(false);
            Log("Info", "Deactivated");
        }
        #endregion


        #region Logger
        [SectionHeader("Udon Logger")] public bool useGlobalLogger = false;
        [HideIf("@useGlobalLogger")] public UdonLogger logger;

        private void Log(string level, string message)
        {
            if (logger == null && useGlobalLogger) logger = (UdonLogger)GameObject.Find("_USV_Global_Logger_").GetComponent(typeof(UdonBehaviour));

            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion
    }
}
