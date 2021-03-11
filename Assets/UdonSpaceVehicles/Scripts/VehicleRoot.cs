
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{

    public class VehicleRoot : UdonSharpBehaviour
    {
        #region Public Variables
        public Animator[] animators = {};
        #endregion

        #region Logics
        void BroadcastCustomEvent(string eventName)
        {
            foreach (var component in components) {
                if (component != this) ((UdonBehaviour)component).SendCustomEvent(eventName);
            }
        }
        #endregion

        #region Unity Events
        Object[] components;
        void Start()
        {
            components = GetComponentsInChildren(typeof(UdonBehaviour));
        }
        #endregion

        #region Udon Events
        #endregion

        #region Custom Events
        #endregion

        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;

            foreach (var component in components) {
                if (component == null) continue;
                Networking.SetOwner(Networking.LocalPlayer, ((UdonBehaviour)component).gameObject);
            }

            BroadcastCustomEvent(nameof(Activate));
            foreach (var animator in animators) animator.SetBool("Active", true);
        }

        public void Deactivate()
        {
            foreach (var animator in animators) animator.SetBool("Active", false);
            BroadcastCustomEvent(nameof(Deactivate));
            active = false;
        }
        #endregion
    }
}
