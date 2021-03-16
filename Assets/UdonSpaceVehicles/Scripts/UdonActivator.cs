
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
        [SectionHeader("Target UdonBehaviours")]  public Component[] targets = { };
        [Space] [SectionHeader("Configurations")]  public bool takeOwnership;
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
        }
        #endregion

        #region Unity Events
        private Component[] targetUdons;
        private void Start()
        {
            if (targets == null) targets = new Component[0];

            var children = autoIncludeChildren ? (Component[])GetComponentsInChildren(typeof(UdonBehaviour)) : new Component[] { };

            targetUdons = new Component[children.Length + targets.Length];
            System.Array.Copy(children, targetUdons, children.Length);

            for (int i = 0; i < targets.Length; i++)
            {
                var target = targets[i];
                if (target == null) continue;
                targetUdons[i] = target.GetComponent(typeof(UdonBehaviour));
            }

            Log($"Initialized with {targetUdons.Length} components");
        }
        #endregion

        #region Udon Events
        #endregion

        #region Activatable
        public void Activate()
        {
            Log($"Activate {targetUdons.Length} components");
            BroadcastActivation(true);
            Log("Activated");
        }

        public void Deactivate()
        {
            Log($"Deactivated {targetUdons.Length} components");
            BroadcastActivation(false);
            Log("Deactivated");
        }
        #endregion

        #region Logger
        private void Log(string log)
        {
            Debug.Log($"[{gameObject.name}] {log}");
        }
        #endregion
    }
}
