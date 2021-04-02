
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles {
    [CustomName("USV Orbital Object")]
    [HelpMessage("Simulates the gravitational force on an orbiting object.The xz plane will be projected as perpendicular to the orbital plane and y will be the altitude.")]
    public class OrbitalObject : UdonSharpBehaviour
    {
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        public bool forceActive = true;
        public bool ownerOnly = true;

        [SectionHeader("Orbital Settings")]
        public bool useGlobalSettings = true;
        [HideIf("@useGlobalSettings")] public float planetMass;
        [HideIf("@useGlobalSettings")] public Vector3 positionBias;
        [HideIf("@useGlobalSettings")] public Vector3 velocityBias;
        [HideIf("@useGlobalSettings")] public float G = 6.67430e-11f;

        private float planetCoG;

        #region Logics

        private void OrbitalObject_Activate()
        {
            if (useGlobalSettings)
            {
                var globalSettings = (UdonBehaviour)GameObject.Find("_USV_Global_Settings_").GetComponent(typeof(UdonBehaviour));
                planetMass = (float)globalSettings.GetProgramVariable(nameof(GlobalSettings.planetMass));
                positionBias = (Vector3)globalSettings.GetProgramVariable(nameof(GlobalSettings.positionBias));
                velocityBias = (Vector3)globalSettings.GetProgramVariable(nameof(GlobalSettings.velocityBias));
            }
            planetCoG = G * planetMass;
        }

        private void OrbitalObject_OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal && forceActive) Activate();
        }

        Vector3 CalculateAccelaration()
        {
            var v = target.velocity + velocityBias;
            var r = target.worldCenterOfMass + positionBias;
            var sqrRadius = r.sqrMagnitude;
            var w = 1 / sqrRadius * Vector3.Cross(r, v);
            var ar = -Vector3.Cross(w, Vector3.Cross(w, r));
            var g = -(planetCoG / sqrRadius) * r.normalized;
            return ar + g;
        }
        #endregion

        #region Unity Events
        private void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!active || ownerOnly && !Networking.IsOwner(target.gameObject)) return;

            target.AddForce(CalculateAccelaration(), ForceMode.Acceleration);
        }
        #endregion

        #region Udon Events
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            OrbitalObject_OnPlayerJoined(player);
        }
        #endregion


        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            OrbitalObject_Activate();
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            if (forceActive) return;
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

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            this.UpdateProxy();

            if (target == null) return;

            if (useGlobalSettings) {
                var settings = GameObject.Find("_USV_Global_Settings_").GetUdonSharpComponent<GlobalSettings>();
                planetMass = settings.planetMass;
                positionBias = settings.positionBias;
                velocityBias = settings.velocityBias;
                planetCoG = G * planetMass;
            }

            Gizmos.color = Color.white;
            var a = CalculateAccelaration();
            Gizmos.DrawRay(target.position, a * 1.0f);
        }
#endif
    }
}
