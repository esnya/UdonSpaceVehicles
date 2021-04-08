
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonToolkit;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    public class UdonEventRelay : UdonSharpBehaviour
    {
        [ListView("Targets")] public UdonSharpBehaviour[] targets = {};
        public bool networked = false;
        public NetworkEventTarget networkEventTarget = NetworkEventTarget.All;


        private void BroadcastCustomEvent(string[] events)
        {
            var length = Mathf.Min(targets.Length, events.Length);
            for (int i = 0; i < length; i++)
            {
                var target = targets[i];
                if (target != null)
                {
                    if (networked) target.SendCustomNetworkEvent(networkEventTarget, events[i]);
                    else target.SendCustomEvent(events[i]);
                }
            }
        }

        public bool fireOnPickup = false;
        [ListView("Targets")][Popup("behaviour", "@targets")] public string[] onPickup = {};
        public override void OnPickup()
        {
            if (fireOnPickup) BroadcastCustomEvent(onPickup);
        }

        public bool fireOnDrop = false;
        [ListView("Targets")][Popup("behaviour", "@targets")] public string[] onDrop = {};
        public override void OnDrop()
        {
            if (fireOnDrop) BroadcastCustomEvent(onDrop);
        }
    }
}
