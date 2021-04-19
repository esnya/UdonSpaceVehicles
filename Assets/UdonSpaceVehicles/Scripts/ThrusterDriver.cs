
using UdonSharp;
using UdonToolkit;
using UnityEngine;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using System.Collections.Generic;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV Thruster Driver")]
    [HelpMessage("Applies thruster force to the vehicle, and animate them. Updates an animator float parameter \"Power\" on children with sync.")]
    public class ThrusterDriver : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("References")]
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        public RCSController rcsController;

        [Space] [SectionHeader("Configurations")]  public Transform[] thrusters;
        [Tooltip("N")]public float thrustPower = 400.0f;
        [Range(0.0f, 1.0f)] public float thrustThreshold = 0.5f;
        #endregion

        #region Internal Variables
        private int thrusterCount;
        private Vector3[] thrusterRotationAxises, thrusterTranslationAxises;
        private Animator[] thrusterAnimators;
        [UdonSynced] private uint syncValue;
        #endregion

        #region Logics
        private void SetThrustAnimation(int i, bool thrust) {
            if (thrusterAnimators[i] == null) return;
            thrusterAnimators[i].SetFloat("Power", thrust ? 1 : 0);
        }

        private void SetThrust(int i, bool thrust)
        {
            if (thrust) {
                var thruster = thrusters[i];
                var worldForce = thruster.forward * thrustPower;
                target.AddForceAtPosition(worldForce, thruster.position, ForceMode.Force);
            }
            SetThrustAnimation(i, thrust);
            syncValue = PackBool(syncValue, i, thrust);
        }
        #endregion

        #region Unity Events
        private void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            var center = target.worldCenterOfMass;

            thrusterCount = thrusters.Length;
            thrusterAnimators = new Animator[thrusterCount];
            thrusterRotationAxises = new Vector3[thrusterCount];
            thrusterTranslationAxises = new Vector3[thrusterCount];
            for (int i = 0; i < thrusterCount; i++)
            {
                var thruster = thrusters[i];

                var centerToThruster = (thruster.position - center).normalized;
                var thrusterForward = thruster.TransformDirection(Vector3.forward);
                thrusterRotationAxises[i] = -transform.InverseTransformDirection(Vector3.Cross(thrusterForward, centerToThruster).normalized);
                thrusterTranslationAxises[i] = transform.InverseTransformDirection(thrusterForward.normalized);

                thrusterAnimators[i] = thruster.GetComponentInChildren<Animator>();
            }

            Log("Info", "Initialized");
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
        private uint prevValue;
        public override void OnDeserialization()
        {
            if (syncValue == prevValue) return;
            for (int i = 0; i < thrusterCount; i++) {
                var thrust = UnpackBool(syncValue, i);
                if (thrust == UnpackBool(prevValue, i)) continue;
                SetThrustAnimation(i, thrust);
            }
            prevValue = syncValue;
        }
        #endregion


        #region Value Packer
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

        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            active = false;

            for (int i = 0; i < thrusterCount; i++)
            {
                SetThrust(i, false);
            }

            Log("Info", "Deactivated");
        }
        #endregion

        #region Logger
        [Space][SectionHeader("Udon Logger")] public UdonLogger logger;
        private void Log(string level, string message)
        {
            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private IEnumerable<Transform> GetTargets()
        {

            return thrusters == null
                ? Enumerable.Empty<Transform>()
                : thrusters.Where(t => t != null);
        }
        private void OnDrawGizmos() {
            foreach (var thruster in GetTargets())
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(thruster.position, -thruster.forward);
            }
        }

        private void OnDrawGizmosSelected() {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            if (target == null) return;
            foreach (var thruster in GetTargets())
            {
                Gizmos.color = Color.white * 0.75f;
                Gizmos.DrawLine(thruster.position, target.worldCenterOfMass);
            }
        }
#endif
    }
}
