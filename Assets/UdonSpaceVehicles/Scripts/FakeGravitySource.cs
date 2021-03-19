
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif


namespace UdonSpaceVehicles
{
    [CustomName("USV Fake Gravity Source")]
    public class FakeGravitySource : UdonSharpBehaviour
    {
        #region Public Variables
        public bool active;
        [Tooltip("kg")] public float mass = 5.9726e+24f;
        [Tooltip("m")] public Vector3 positionOffset = Vector3.down * 6728137;
        [Tooltip("m/s")] public Vector3 velocityBias = Vector3.forward * 7990;
        public GameObject findTargetsFrom;
        public bool ownerOnly = true;
        #endregion

        #region Logics
        private Rigidbody[] GetTargets()
        {
            return findTargetsFrom.GetComponentsInChildren<Rigidbody>();
        }

        Vector3 CalculateAccelaration(Rigidbody target)
        {
            var v = target.velocity + velocityBias;
            var r = target.position - (transform.position + positionOffset);
            var sqrRadius = r.sqrMagnitude;
            var w = 1 / sqrRadius * Vector3.Cross(r, v);
            var ar = -Vector3.Cross(w, Vector3.Cross(w, r));
            var g = -(gm / sqrRadius) * r.normalized;
            return ar + g;
        }
        #endregion

        #region Unity Events
        private Rigidbody[] targets;
        private int targetCount;
        private const float G = 6.67430e-11f;
        private float gm;
        private void Start()
        {
            targets = findTargetsFrom.GetComponentsInChildren<Rigidbody>();
            targetCount = targets.Length;

            gm = G * mass;

            Log("Info", $"Initialized with {targetCount} targets");
            if (active) Activate();
        }

        private void FixedUpdate()
        {
            if (!active) return;

            foreach (var target in targets)
            {
                if (ownerOnly && !Networking.IsOwner(target.gameObject)) continue;

                target.AddForce(CalculateAccelaration(target), ForceMode.Acceleration);
            }
        }
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
        [SectionHeader("Udon Logger")] public UdonLogger logger;
        private void Log(string level, string message)
        {
            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            this.UpdateProxy();
            if (findTargetsFrom == null) return;
            gm = G * mass;

            Gizmos.color = Color.white;
            foreach (var target in GetTargets()) {
                var a = CalculateAccelaration(target);
                Gizmos.DrawRay(target.position, a * 1.0f);
            }
        }
#endif
    }
}
