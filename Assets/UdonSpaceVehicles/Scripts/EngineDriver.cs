
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Engine Driver")]
    public class EngineDriver : UdonSharpBehaviour
    {
        #region Public Variables
        public VehicleRoot vehicleRoot;
        public ControllerInput controllerInput;
        public SyncManager syncManager;
        public uint syncManagerBank = 1u;

        [ListView("Engines / Powers")] public Transform[] engines = { };
        [ListView("Engines / Powers")] public float[] powers = { };
        [RangeSlider(0.0f, 1.0f)] public float remoteAnimationThreshold = 0.1f;
        [HelpBox("Updates float parameter \"Engine Power\" with max value of engine powers.")] public Animator[] animators;
        #endregion

        #region Logics
        private void SetAnimation(int index, float power)
        {
            var animator = engineAnimators[index];
            if (animator != null) animator.SetFloat("Power", power);
        }

        private void SetPower(int index, float power)
        {
            var engine = engines[index];
            var worldForce = engine.forward * powers[index] * power;
            rootRigidbody.AddForceAtPosition(worldForce, engine.position, ForceMode.Force);

            SetAnimation(index, power);
            syncManager.SetBool(syncManagerBank, index, power > remoteAnimationThreshold);
        }

        private void UpdateAnimatiors(float power)
        {
            foreach (var animator in animators) animator.SetFloat("Engine Power", power);
            syncManager.SetBool(syncManagerBank,31, power > remoteAnimationThreshold);
        }
        #endregion

        #region Unity Events
        Rigidbody rootRigidbody;
        int engineCount, animatorCount;
        Animator[] engineAnimators;
        Vector3[] axises;
        void Start()
        {
            rootRigidbody = vehicleRoot.GetComponent<Rigidbody>();
            engineCount = Mathf.Min(engines.Length, powers.Length);
            animatorCount = animators.Length;

            axises = new Vector3[engineCount];
            engineAnimators = new Animator[engineCount];
            for (int i = 0; i < engineCount; i++)
            {
                var engine = engines[i];
                axises[i] = transform.InverseTransformVector(engine.forward);

                var animator = engine.GetComponentInChildren<Animator>();
                engineAnimators[i] = animator ?? null;
            }

            Log("Initialized");
        }

        private void Update()
        {
            if (!active) return;

            var input = controllerInput.input;
            var maxPower = 0.0f;
            for (int i = 0; i < engineCount; i++)
            {
                var power = Vector3.Dot(input, axises[i]);
                maxPower = Mathf.Max(maxPower, power);
                SetPower(i, power);
            }

            UpdateAnimatiors(maxPower);
        }
        #endregion

        #region Udon Events
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal) {
                syncManager.AddEventListener(this, syncManagerBank,  ~0u, nameof(syncValue), nameof(prevValue), nameof(_SyncValueChanged));
            }
        }
        #endregion

        #region Custom Events
        [HideInInspector] public uint syncValue, prevValue;
        public void _SyncValueChanged() {
            for (int i = 0; i < engineCount; i++) {
                var b = UnpackBool(syncValue, i);
                if (b == UnpackBool(prevValue, i)) continue;
                SetAnimation(i, b ? 100.0f : 0.0f);
            }

            var globalValue = UnpackBool(syncValue, 31);
            if (globalValue != UnpackBool(prevValue, 31)) UpdateAnimatiors(globalValue ? 1.0f : 0.0f);
        }
        #endregion

        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;

            Log("Activated");
        }

        public void Deactivate()
        {
            active = false;

            for (int i = 0; i < engineCount; i++) SetPower(i, 0.0f);
            UpdateAnimatiors(0.0f);

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
