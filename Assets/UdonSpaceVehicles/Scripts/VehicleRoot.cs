
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
        [HelpBox("Updates synced bool parameter \"Power\"")] public Animator[] animators = { };
        #endregion

        #region Logics
        private void SetBool(string name, bool value)
        {
            foreach (var animator in animators) animator.SetBool(name, value);
        }

        private bool power;
        private void SetPower(bool value)
        {
            Log("Info", $"Power: {value}");
            power = value;
            syncManager.SetBool(syncManagerBank, 0, value);
            SetBool("Power", value);
        }
        #endregion

        #region Unity Events
        private Rigidbody rootRigidbody;
        private void Start()
        {
            rootRigidbody = GetComponent<Rigidbody>();
            Log("Info", "Initialized");
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
            if (player.isLocal)
            {
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
            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            active = false;

            SetPower(false);

            Log("Info", "Deactivated");
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
