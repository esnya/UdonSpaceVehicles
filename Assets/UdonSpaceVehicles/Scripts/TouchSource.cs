
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class TouchSource : UdonSharpBehaviour
    {
        public string variableName = "touchSource";
        public string onEnter = "TouchEnter";
        public string onExit = "TouchExit";

        public VRC_Pickup.PickupHand hand;
        [Range(0.0f, 1.0f)] public float hapticDuration = 1.0f;
        [Range(0.0f, 1.0f)] public float hapticStrength = 0.2f;
        public float hapticFrequency = 320.0f;
        private const float strengthCorrection = 0.06f;

        private void Start()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            foreach (var collider in GetComponents<Collider>()) collider.isTrigger = true;
        }

        private void SendTouchEvent(string eventName, Collider other)
        {
            if (other == null) return;
            var udon = (UdonBehaviour)other.GetComponent(typeof(UdonBehaviour));
            if (udon == null) return;

            udon.SetProgramVariable(variableName, this);
            udon.SendCustomEvent(eventName);
        }

        private void OnTriggerEnter(Collider other)
        {
            SendTouchEvent(onEnter, other);
        }
        private void OnTriggerExit(Collider other)
        {
            SendTouchEvent(onExit, other);
        }

        public void PlayHaptic(float strength)
        {
            Networking.LocalPlayer.PlayHapticEventInHand(hand, hapticDuration, hapticStrength * strengthCorrection * strength, hapticFrequency);
        }
    }
}
