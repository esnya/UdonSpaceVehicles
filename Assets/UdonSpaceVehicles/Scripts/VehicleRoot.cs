
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
        public SyncManager syncManager;
        public uint syncManagerBank = 0u;
        [HelpBox("Updates bool parameter \"Power\"")][UTEditor] public Animator[] animators = {};
        #endregion

        #region Logics
        private void SetBool(string name, bool value) {
            foreach (var animator in animators) animator.SetBool(name, value);
        }

        private bool power;
        private void SetPower(bool value) {
            Log($"Power: {value}");
            power = value;
            syncManager.SetBool(syncManagerBank, 0, value);
            SetBool("Power", value);
        }

        private void SetKinematic(bool kinematic) {
            var localPlayer = Networking.LocalPlayer;
            foreach (var rigidbody in rigidbodies) {
                if (rigidbody.gameObject != gameObject) {
                    //rigidbody.isKinematic = kinematic;
                    if (!kinematic) Networking.SetOwner(localPlayer, rigidbody.gameObject);
                }
            }
        }
        #endregion

        #region Unity Events
        private Rigidbody[] rigidbodies;
        private void Start()
        {
            rigidbodies = GetComponentsInChildren<Rigidbody>();
            SetKinematic(true);
            Log("Initialized");
            Log($"{rigidbodies.Length} rigidbodies");
        }

        private void Update()
        {
            if (!active) return;
            if (!power && Networking.IsOwner(syncManager.gameObject)) SetPower(true);
        }
        #endregion

        #region Udon Events
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) {
                syncManager.AddEventListener(this, syncManagerBank, 0x01u, nameof(syncValue), nameof(prevValue), nameof(_SyncValueChanged));
            }
        }
        #endregion

        #region Custom Events
        [HideInInspector] public uint syncValue, prevValue;
        public void _SyncValueChanged()
        {
            SetBool("Power", UnpackBool(syncValue, 0));
        }
        #endregion

        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;
            Networking.SetOwner(Networking.LocalPlayer, syncManager.gameObject);
            SetKinematic(false);
            Log("Activated");
        }

        public void Deactivate()
        {
            active = false;

            SetKinematic(true);
            SetPower(false);

            Log("Deactivated");
        }
        #endregion

        #region Logger
        private void Log(string log)
        {
            Debug.Log($"[{gameObject.name}] {log}");
        }
        #endregion
        
        #region Value Packing
        private uint UnpackValue(uint packed, int byteOffset, uint bitmask)
        {
            return (packed >> byteOffset & bitmask);
        }
        private uint PackValue(uint packed, int byteOffset, uint bitmask, uint value)
        {
            var mask = bitmask << byteOffset;
            return packed & mask | value & bitmask << byteOffset;
        }

        private bool UnpackBool(uint packed, int byteOffset)
        {
            return UnpackValue(packed, byteOffset, 0x01) != 0;
        }
        private uint PackBool(uint packed, int byteOffset, bool value)
        {
            return PackValue(packed, byteOffset, 0x1, value ? 1u : 0u);
        }

        private byte UnpackByte(uint packed, int byteOffset)
        {
            return (byte)UnpackValue(packed, byteOffset, 0xff);
        }

        private uint PackByte(uint packed, int byteOffset, byte value)
        {
            return PackValue(packed, byteOffset, 0xff, value);
        }
        #endregion
    }
}
