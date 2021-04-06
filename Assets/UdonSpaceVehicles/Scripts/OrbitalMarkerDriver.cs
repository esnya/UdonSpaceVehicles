
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles {
    [CustomName("USV Orbital Marker Driver")]
    [HelpMessage("Rotates the attached object according to the direction of the orbit.")]
    public class OrbitalMarkerDriver : UdonSharpBehaviour
    {
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        [HelpBox("Set None to use the profile attached to \"_USV_Global_Profile_\"")] public GravityProfile gravityProfile;

        #region Gravitational Object
        Vector3 velocityBias;
        private void LoadGravityProfile(GravityProfile profile)
        {
            if (profile == null)
            {
                var globalProfileObject = GameObject.Find("_USV_Global_Profile_");
                if (globalProfileObject == null)
                {
                    Log("Error", "Failed to find global GravityProfile");
                    return;
                }
                profile = globalProfileObject.GetComponent<GravityProfile>();
            }
            if (profile == null)
            {
                Log("Error", "Failed to load GravityProfile");
                return;
            }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
            profile.GetUdonSharpComponent<GravityProfile>().UpdateProxy();
#endif
            velocityBias = profile.velocityBias;
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
            LoadGravityProfile(gravityProfile);
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
