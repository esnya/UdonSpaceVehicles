
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles {
    [CustomName("USV Orbital Marker Driver")]
    public class OrbitalMarkerDriver : UdonSharpBehaviour
    {
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;

        #region Orbital Object
        [SectionHeader("Orbital Settings")]
        public bool useGlobalSettings = true;
        [HideIf("@useGlobalSettings")] public float planetMass;
        [HideIf("@useGlobalSettings")] public float altitudeBias = 350e+3f;
        [HideIf("@useGlobalSettings")] public Vector3 positionBias;
        [HideIf("@useGlobalSettings")] public Vector3 velocityBias;
        [HideIf("@useGlobalSettings")] public float G = 6.67430e-11f;
        private float planetCoG;

        private void OrbitalObject_Activate()
        {
            if (useGlobalSettings)
            {
                var globalSettings = (UdonBehaviour)GameObject.Find("_USV_Global_Settings_").GetComponent(typeof(UdonBehaviour));
                if (globalSettings == null) return;
                planetMass = (float)globalSettings.GetProgramVariable(nameof(GlobalSettings.planetMass));
                altitudeBias = (float)globalSettings.GetProgramVariable(nameof(GlobalSettings.altitudeBias));
                positionBias = (Vector3)globalSettings.GetProgramVariable(nameof(GlobalSettings.positionBias));
                velocityBias = (Vector3)globalSettings.GetProgramVariable(nameof(GlobalSettings.velocityBias));
                G = (float)globalSettings.GetProgramVariable(nameof(GlobalSettings.G));
            }
            planetCoG = G * planetMass;
        }
        #endregion

        #region Unity Events
        private void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
        }

        private readonly Vector3 xzScaler = Vector3.one - Vector3.up;
        private void Update()
        {
            if (!active) return;
            transform.rotation = Quaternion.FromToRotation(Vector3.forward, Vector3.Scale(target.velocity + velocityBias, xzScaler).normalized);
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
    }
}
