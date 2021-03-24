
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Laser Gun")]
    [HelpMessage("The gun.")]
    [RequireComponent(typeof(ParticleSystem))]
    public class LaserGun : UdonSharpBehaviour
    {
        #region Public Variables
        public VehicleRoot vehicleRoot;
        public string vrButton = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        public KeyCode desktopKey = KeyCode.Space;
        public AudioSource audioSource;
        public AudioClip audioClip;
        [Tooltip("Interval in frames")] public int fireInterval = 90 / 2;
        #endregion

        #region Logics
        private bool GetTrigger()
        {
            if (vr) return Input.GetAxis(vrButton) > 0.5f;
            else return Input.GetKey(KeyCode.Space);
        }
        #endregion

        #region Unity Events
        private new ParticleSystem particleSystem;
        private void Start()
        {
            particleSystem = GetComponent<ParticleSystem>();
            Log("Info", "Initialized");
        }

        private void Update()
        {
            if (!active) return;

            if (Time.frameCount % fireInterval == 0 && GetTrigger())
            {
                SendCustomNetworkEvent(NetworkEventTarget.All, nameof(Fire));
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!active || other == null || vehicleRoot != null && other == vehicleRoot.gameObject) return;
            var udon = (UdonBehaviour)other.GetComponent(typeof(UdonBehaviour));
            if (udon == null) return;
            udon.SendCustomNetworkEvent(NetworkEventTarget.Owner, "Hit");
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
        #endregion

        #region Custom Events
        public void Fire()
        {
            particleSystem.Emit(1);
            if (audioSource != null) audioSource.PlayOneShot(audioClip);
        }
        #endregion


        #region Activatable
        bool active = false;

        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            active = false;
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