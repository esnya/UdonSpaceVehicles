
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Thruster Driver")]
    public class ThrusterDriver : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("References")] public VehicleRoot vehicleRoot;
        public RCSController rcsController;
        public SyncManager syncManager;
        public uint syncManagerBank = 2u;

        [Space] [SectionHeader("Configurations")]  public Transform[] thrusters;
        public float thrustPower = 20.0f;
        [Range(0.0f, 1.0f)] public float thrustThreshold = 0.1f;
        #endregion

        #region Internal Variables
        private int thrusterCount;
        private Vector3[] thrusterRotationAxises, thrusterTranslationAxises;
        private Animator[] thrusterAnimators;
        #endregion

        #region Logics
        private void SetThrustAnimation(int i, bool thrust) {

            if (thrusterAnimators[i] == null) return;
            thrusterAnimators[i].SetFloat("Power", thrust ? 1 : 0);
        }

        private void SetThrust(int i, bool thrust)
        {
            // thrusters[i].relativeForce = thrust ? -Vector3.forward * thrustPower : Vector3.zero;
            if (thrust) {
                var thruster = thrusters[i];
                var worldForce = -thruster.forward * thrustPower;
                rootRigidbody.AddForceAtPosition(worldForce, thruster.position, ForceMode.Force);
            }
            SetThrustAnimation(i, thrust);
            syncManager.SetBool(syncManagerBank, i, thrust);
        }
        #endregion

        #region Unity Events
        private Rigidbody rootRigidbody;
        private void Start()
        {
            rootRigidbody = vehicleRoot.GetComponent<Rigidbody>();
            var center = transform.position;

            thrusterCount = thrusters.Length;
            thrusterAnimators = new Animator[thrusterCount];
            thrusterRotationAxises = new Vector3[thrusterCount];
            thrusterTranslationAxises = new Vector3[thrusterCount];
            for (int i = 0; i < thrusterCount; i++)
            {
                var thruster = thrusters[i];

                var centerToThruster = (thruster.position - center).normalized;
                var thrusterForward = thruster.TransformDirection(Vector3.forward);
                thrusterRotationAxises[i] = transform.InverseTransformDirection(Vector3.Cross(thrusterForward, centerToThruster).normalized);
                thrusterTranslationAxises[i] = -transform.InverseTransformDirection(thrusterForward.normalized);

                thrusterAnimators[i] = thruster.GetComponentInChildren<Animator>();
            }

            Log("Initialized");
        }

        private void Update()
        {
            if (!active) return;

            var rotationInput = rcsController.rotation;
            var translationInput = rcsController.translation;

            for (int i = 0; i < thrusterCount; i++)
            {
                var rotation = Mathf.Clamp01(Vector3.Dot(thrusterRotationAxises[i], rotationInput));
                var translation = Mathf.Clamp01(Vector3.Dot(thrusterTranslationAxises[i], translationInput));

                var thrust = rotation + translation >= thrustThreshold;
                SetThrust(i, thrust);
            }
        }
        #endregion

        #region Udon Events
        public override void OnPlayerJoined(VRCPlayerApi player) {
            if (player.isLocal) {
                syncManager.AddEventListener(this, syncManagerBank, ~0u, nameof(syncValue), nameof(prevValue), nameof(_SyncValueChanged));
            }
        }
        #endregion

        #region Custom Events
        [HideInInspector] public uint syncValue, prevValue;
        public void _SyncValueChanged()
        {
            for (int i = 0; i < thrusterCount; i++) {
                var b = UnpackBool(syncValue, i);
                if (b != UnpackBool(prevValue, i)) SetThrustAnimation(i, b);
            }
        }
        #endregion

        #region Value Packer
        uint UnpackValue(uint packed, int bitOffset, uint bitmask)
        {
            return (packed >> bitOffset & bitmask);
        }
        uint PackValue(uint packed, int bitOffset, uint bitmask, uint value)
        {
            var mask = bitmask << bitOffset;
            return packed & mask | value & bitmask << bitOffset;
        }

        bool UnpackBool(uint packed, int bitOffset)
        {
            return UnpackValue(packed, bitOffset, 0x01) != 0;
        }
        uint PackBool(uint packed, int bitOffset, bool value)
        {
            return PackValue(packed, bitOffset, 0x1, value ? 1u : 0u);
        }
        #endregion

        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            Log("Activated");
        }

        public void Dectivate()
        {
            active = false;

            for (int i = 0; i < thrusterCount; i++)
            {
                SetThrust(i, false);
            }

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
