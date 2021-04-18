
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    public class HandFollower : UdonSharpBehaviour
    {
        public VRCPlayerApi.TrackingDataType hand = VRCPlayerApi.TrackingDataType.LeftHand;

        private void LateUpdate()
        {
            transform.position = Networking.LocalPlayer.GetTrackingData(hand).position;
        }
    }
}
