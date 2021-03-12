
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [RequireComponent(typeof(VRCStation))][CustomName("USV Seat Controller")]
    public class SeatController : UdonSharpBehaviour
    {
        #region Public Variables
        public UdonActivator activationTarget;
        public Transform viewPosition;
        public Vector3 adjustorAxis = new Vector3(0.0f, 1.0f, 1.0f);
        public float adjustorThreshold = 0.05f;
        public string exitButton = "Oculus_CrossPlatform_Button4";
        public KeyCode exitKey = KeyCode.Return;
        #endregion

        #region Logics
        bool GetExitInput()
        {
            return vr ? Input.GetButton(exitButton) : Input.GetKey(exitKey);
        }
        #endregion

        #region Unity Events
        VRCStation station;
        Vector3 initialPosition;
        void Start()
        {
            station = (VRCStation)GetComponent(typeof(VRCStation));
            station.disableStationExit = true;
            initialPosition = transform.localPosition;
        }

        void Update()
        {
            if (seated && GetExitInput()) station.ExitStation(Networking.LocalPlayer);
        }

        void LateUpdate()
        {
            if (seated && !adjusted) {
                var headPosition = viewPosition.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
                var diff = Vector3.Scale(-headPosition, adjustorAxis);
                transform.localPosition += diff;
                adjusted = diff.magnitude <= adjustorThreshold;
            }
        }
        #endregion

        #region Udon Events
        bool vr;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) {
                vr = player.IsUserInVR();
            }
        }

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        bool seated, adjusted;
        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (player.isLocal) {
                seated = true;
                adjusted = false;

                activationTarget.Activate();
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            if (player.isLocal) {
                activationTarget.Deactivate();

                seated = false;
                transform.localPosition = initialPosition;
            }
        }
        #endregion
    }
}
