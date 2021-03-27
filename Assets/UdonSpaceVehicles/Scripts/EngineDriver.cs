
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSpaceVehicles
{
    [CustomName("USV Engine Driver")]
    [HelpMessage("Applies the power of the main engine and animates  The ControllerInput is required as a throttle.")]
    public class EngineDriver : UdonSharpBehaviour
    {
        #region Public Variables
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        public ControllerInput controllerInput;

        [ListView("Engines / Powers")] public Transform[] engines = { };
        [ListView("Engines / Powers")] public float[] powers = { };
        [RangeSlider(0.0f, 1.0f)] public float remoteAnimationThreshold = 0.1f;
        [HelpBox("Updates float parameter \"Engine Power\" with max value of engine powers.")] public Animator[] animators;
        [UdonSynced] private uint syncValue;
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
            target.AddForceAtPosition(worldForce, engine.position, ForceMode.Force);

            SetAnimation(index, power);
            syncValue = PackBool(syncValue, index, power > remoteAnimationThreshold);
        }

        private void UpdateAnimatiors(float power)
        {
            foreach (var animator in animators) animator.SetFloat("Engine Power", power);
            syncValue = PackBool(syncValue, 31, power > remoteAnimationThreshold);
        }
        #endregion

        #region Unity Events
        int engineCount;
        Animator[] engineAnimators;
        Vector3[] axises;
        void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            engineCount = Mathf.Min(engines.Length, powers.Length);

            axises = new Vector3[engineCount];
            engineAnimators = new Animator[engineCount];
            for (int i = 0; i < engineCount; i++)
            {
                var engine = engines[i];
                axises[i] = transform.InverseTransformVector(engine.forward);

                var animator = engine.GetComponentInChildren<Animator>();
                engineAnimators[i] = animator ?? null;
            }

            Log("Info", "Initialized");
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
        private uint prevValue;
        public override void OnDeserialization()
        {
            if (syncValue == prevValue) return;

            for (int i = 0; i < engineCount; i++)
            {
                var b = UnpackBool(syncValue, i);
                if (b == UnpackBool(prevValue, i)) continue;
                SetAnimation(i, b ? 100.0f : 0.0f);
            }

            var globalValue = UnpackBool(syncValue, 31);
            if (globalValue != UnpackBool(prevValue, 31)) UpdateAnimatiors(globalValue ? 1.0f : 0.0f);

            prevValue = syncValue;
        }
        #endregion

        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;

            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            active = false;

            for (int i = 0; i < engineCount; i++) SetPower(i, 0.0f);
            UpdateAnimatiors(0.0f);

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
            return (packed >> byteOffset) & bitmask;
        }
        private uint PackValue(uint packed, int byteOffset, uint bitmask, uint value)
        {
            return packed & ~(bitmask << byteOffset) | (value & bitmask) << byteOffset;
        }

        private bool UnpackBool(uint packed, int byteOffset)
        {
            return UnpackValue(packed, byteOffset, 0x01) != 0;
        }
        private uint PackBool(uint packed, int byteOffset, bool value)
        {
            return PackValue(packed, byteOffset, 0x1, value ? 1u : 0u);
        }

        #endregion
    }
}
