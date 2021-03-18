
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("Gravity Source")]
    public class GravitySource : UdonSharpBehaviour
    {
        #region Public Variables
        public bool active;
        [Tooltip("kg")] public double mass = 5.972e+24;
        [Tooltip("m")] public Vector3 centerOffset = Vector3.down * 350000; // LEO
        [Tooltip("m/s")] public Vector3 velocityBias = Vector3.forward * 8000;
        public float timeScale = 1.0f, lengthScale = 1.0f;
        public Transform findTargetsFrom;
        public bool ownerOnly;
        #endregion

        #region Logics
        private Rigidbody[] GetTargets()
        {
            return findTargetsFrom.GetComponentsInChildren<Rigidbody>();
        }

        private Vector3 CalculateAccelaration(Rigidbody target)
        {
            var diff = (transform.position + centerOffset - target.position) * lengthScale;
            var sqrR = diff.sqrMagnitude;
            var a = (float)(gm / sqrR) * diff.normalized;
            return a * (Mathf.Pow(timeScale, 2.0f) / lengthScale);
        }
        #endregion

        #region Unity Events
        private Rigidbody[] targets;
        private int targetCount;
        private const double G = 6.67430e-11;
        private double gm;
        private void Start()
        {
            targets = GetTargets();
            targetCount = targets.Length;

            gm = G * mass;

            Log("Info", $"Initialized with {targetCount} targets");
            if (active) Activate();
        }

        private void FixedUpdate()
        {
            if (!active) return;

            for (int i = 0; i < targetCount; i++)
            {
                var target = targets[i];
                if (target.isKinematic || target.useGravity || ownerOnly && !Networking.IsOwner(target.gameObject)) continue;

                target.AddForce(CalculateAccelaration(target), ForceMode.Acceleration);
            }
        }
        #endregion

        #region Udon Events
        #endregion

        #region Custom Events
        #endregion

        #region Internal Logics
        #endregion

        #region Activatable
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            active = false;
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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            this.UpdateProxy();

            Gizmos.color = Color.white;
            foreach (var target in GetTargets()) {
                var a = CalculateAccelaration(target);
                Gizmos.DrawRay(target.position, a * (lengthScale / Mathf.Pow(timeScale, 2.0f)));
            }
        }
#endif
    }
}