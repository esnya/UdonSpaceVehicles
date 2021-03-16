
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSpaceVehicles
{
    [RequireComponent(typeof(VRCStation))]
    [CustomName("USV Seat Controller")]
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
        private bool GetExitInput()
        {
            return vr ? Input.GetButton(exitButton) : Input.GetKey(exitKey);
        }

        private void GetOut()
        {
            station.ExitStation(Networking.LocalPlayer);
        }
        #endregion

        #region Unity Events
        VRCStation station;
        Vector3 initialPosition;
        Collider triggerCollider;
        private void Start()
        {
            station = (VRCStation)GetComponent(typeof(VRCStation));
            station.disableStationExit = true;
            triggerCollider = GetComponent<Collider>();
            initialPosition = transform.localPosition;

            Log("Info", "Initialized");
        }

        private void Update()
        {
            if (seated && GetExitInput()) GetOut();
        }

        private void LateUpdate()
        {
            if (seated && !adjusted)
            {
                var headPosition = viewPosition.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position);
                var diff = Vector3.Scale(-headPosition, adjustorAxis);
                transform.localPosition += diff;
                adjusted = diff.magnitude <= adjustorThreshold;
                Log("Info", $"Adjusting {diff}");
            }
        }
        #endregion

        #region Udon Events
        bool vr;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                vr = player.IsUserInVR();
                Log("Info", $"VR: {vr}");
            }
        }

        public override void Interact()
        {
            Networking.LocalPlayer.UseAttachedStation();
        }

        bool seated, adjusted;
        public override void OnStationEntered(VRCPlayerApi player)
        {
            Log("Info", "Entered");

            triggerCollider.enabled = false;

            if (player.isLocal)
            {
                seated = true;
                adjusted = false;

                activationTarget.Activate();
            }
        }

        public override void OnStationExited(VRCPlayerApi player)
        {
            Log("Info", "Exited");

            triggerCollider.enabled = true;

            if (player.isLocal)
            {
                activationTarget.Deactivate();

                seated = false;
                transform.localPosition = initialPosition;
            }
        }
        #endregion

        #region Custom Events
        public void _Respawned()
        {
            GetOut();
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
