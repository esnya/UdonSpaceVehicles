
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [CustomName("USV Vehicle Root")]
    [HelpMessage("Manages collisions, and respawns, vehicle syncPower states. Attach to the root game object with a Rigidbody.")]
    [RequireComponent(typeof(Rigidbody))]
    public class VehicleRoot : UdonSharpBehaviour
    {
        #region Public Variables
        [HelpBox("Updates synced bool parameter \"Power\", sets trigger \"Collision\" on collision")] public Animator[] animators = { };

        [ListView("On Collision Event Targets")] public UdonSharpBehaviour[] onCollisionTargets = {};
        [ListView("On Collision Event Targets")] public string[] onCollisionVariableNames = {};
        [ListView("On Collision Event Targets")][Popup("behaviour", "@onCollisionTargets", true)] public string[] onCollisionEventNames = {};
        #endregion
        private const int Power = 0;

        #region Logics
        private void SetBool(string name, bool value)
        {
            foreach (var animator in animators) animator.SetBool(name, value);
        }

        private void SetTrigger(string name)
        {
            foreach (var animator in animators) animator.SetTrigger(name);
        }

        [UdonSynced] private bool syncPower;
        private void SetPower(bool value)
        {
            Log("Info", $"Power: {value}");
            syncPower = value;
            SetBool("Power", value);
        }

        private void BroadcastCustomEvent(string eventName)
        {
            foreach (var c in udonBehaviours)
            {
                var udon = (UdonBehaviour)c;
                if (udon == null || udon.gameObject == gameObject) continue;
                udon.SendCustomEvent(eventName);
            }
        }
        #endregion

        #region Unity Events
        private Rigidbody rootRigidbody;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Component[] udonBehaviours;
        private int onCollisionEventTargetCount;
        private void Start()
        {
            rootRigidbody = GetComponent<Rigidbody>();
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;

            udonBehaviours = GetComponentsInChildren(typeof(UdonBehaviour));

            onCollisionEventTargetCount = Mathf.Min(onCollisionTargets.Length, Mathf.Min(onCollisionVariableNames.Length, onCollisionEventNames.Length));

            Log("Info", $"Initialized with {udonBehaviours.Length - 1} child components");
        }

        private void Update()
        {
            if (!active) return;
            if (!syncPower && Networking.IsOwner(gameObject)) SetPower(true);
        }

        private void OnCollisionEnter(Collision collision) {
            if (!active) return;

            for (int i = 0; i < onCollisionEventTargetCount; i++)
            {
                var u = onCollisionTargets[i];
                if (u == null) continue;
                u.SetProgramVariable(onCollisionVariableNames[i], collision);
                u.SendCustomEvent(onCollisionEventNames[i]);
            }
        }
        #endregion

        #region Udon Events
        private bool prevPower;
        public override void OnDeserialization()
        {
            if (prevPower == syncPower) return;
            SetBool("Power", syncPower);
            prevPower = syncPower;
        }
        #endregion

        #region Custom Events
        public void Respawn()
        {
            rootRigidbody.velocity = Vector3.zero;
            rootRigidbody.angularVelocity = Vector3.zero;
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;

            Deactivate();

            BroadcastCustomEvent("_Respawned");

            Log("Info", "Respawned");
        }

        public void Hit()
        {
            BroadcastCustomEvent("_Hit");
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
    }
}
