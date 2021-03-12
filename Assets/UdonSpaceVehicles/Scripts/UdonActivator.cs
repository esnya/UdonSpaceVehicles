
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
        [SectionHeader("Target UdonBehaviours")][UTEditor] public Component[] targets = {};
        public bool activateChildren;

        [Space][SectionHeader("Configurations")][UTEditor] public bool takeOwnership;

        [Space][SectionHeader("Animators")][UTEditor] public bool fireAnimatorTriggers = true;
        [ListView("Animator Trigger On Activate")][UTEditor] public Animator[] animatorsOnActivate;
        [ListView("Animator Trigger On Activate")][Popup("animator", "@animatorsOnActivate", true)][UTEditor] public string[] animatorTriggersOnActivate;
        [ListView("Animator Trigger On Deactivate")][UTEditor] public Animator[] animatorsOnDeactivate;
        [ListView("Animator Trigger On Deactivate")][Popup("animator", "@animatorsOnDeactivate", true)][UTEditor] public string[] animatorTriggersOnDeactivate;
        #endregion

        #region Logics
        private void BroadcastActivation(bool active) {
            var localPlayer = Networking.LocalPlayer;
            foreach (var obj in targetUdons) {
                if (obj == null) continue;

                var udon = (UdonBehaviour)obj;
                if (udon == null || udon.gameObject == gameObject) continue;

                if (active && takeOwnership) Networking.SetOwner(localPlayer, udon.gameObject);

                udon.SendCustomEvent(active ? "Activate" : "Deactivate");
            }

            if (fireAnimatorTriggers) {
                var animators = active ? animatorsOnActivate : animatorsOnDeactivate;
                var triggers = active ? animatorTriggersOnActivate : animatorTriggersOnDeactivate;

                var length = Mathf.Min(animators.Length, triggers.Length);
                for (int i = 0; i < length; i++) {
                    animators[i].SetTrigger(triggers[i]);
                }
            }
        }
        #endregion

        #region Unity Events
        private Component[] targetUdons;
        private void Start()
        {
            if (targets == null) targets = new Component[0];

            var children = activateChildren ? (Component[])GetComponentsInChildren(typeof(UdonBehaviour)) : new Component[] {};

            targetUdons = new Component[children.Length + targets.Length];
            System.Array.Copy(children, targetUdons, children.Length);

            for (int i = 0; i < targets.Length; i++) {
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
        private void Log(string log) {
            Debug.Log($"[{gameObject.name}] {log}");
        }
        #endregion
    }
}
