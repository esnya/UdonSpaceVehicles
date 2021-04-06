
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Security;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV Gravity Controller")]
    [HelpMessage("Adds an unprojected gravitational force to target objects. Time and length can be scaled.")]
    [DefaultExecutionOrder(-5)]
    public class GravityController : UdonSharpBehaviour
    {
        #region Public Variables
        public bool active;

        [HelpBox("Set None to use transform of \"_USV_Global_Profile_\" and the profile attached to it.")][ListView("Gravity Srouces")] public Transform[] gravitySources = { null };
        [ListView("Gravity Srouces")] public GravityProfile[] profiles = { null };
        public float timeScale = 1.0f, lengthScale = 1.0f;
        [HelpBox("High-precision gravity calculation. When enabled, velocity bias of gravity sources will be ignored.")] public bool highPrecisionMode;

        public bool findTargetsFromChildren;
        [HideIf("@!findTargetsFromChildren")] public Transform findTargetsFrom;
        [HideIf("@findTargetsFromChildren")] public Rigidbody[] targets = {};
        [HelpBox("Enable to position synced targets.")] public bool ownerOnly = true;
        #endregion

        #region Logics
        private void Initialize()
        {
            targets = GetTargets();
            targetCount = targets.Length;
            targetObjects = new GameObject[targetCount];
            for (int i = 0; i < targetCount; i++)
            {
                targetObjects[i] = targets[i].gameObject;
            }

            sourceCount = Mathf.Min(gravitySources.Length, profiles.Length);
            for (int i = 0; i < sourceCount; i++)
            {
                if (gravitySources[i] == null)
                {
                    var globalProfile = GameObject.Find("_USV_Global_Profile_");
                    if (globalProfile == null)
                    {
                        Log("Error", "Failed to find global GravityProfile");
                        return;
                    }
                    gravitySources[i] = globalProfile.transform;
                }
            }
            standardGravitationalParameter = new float[sourceCount];
            positionBias = new Vector3[sourceCount];
            velocityBias = new Vector3[sourceCount];
        }

        private Rigidbody[] GetTargets()
        {
            return findTargetsFromChildren ? findTargetsFrom.GetComponentsInChildren<Rigidbody>() : targets;
        }

        private void LoadGravityProfiles()
        {
            for (int i = 0; i < sourceCount; i++) {
                var profile = profiles[i];
                if (profile == null)
                {
                    var globalProfileObject = GameObject.Find("_USV_Global_Profile_");
                    if (globalProfileObject == null)
                    {
                        Log("Error", "Failed to find global GravityProfile");
                        continue;
                    }
                    profile = globalProfileObject.GetComponent<GravityProfile>();
                }
                if (profile == null)
                {
                    Log("Error", "Failed to load GravityProfile");
                    continue;
                }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
                profile.GetUdonSharpComponent<GravityProfile>().UpdateProxy();
#endif
                standardGravitationalParameter[i] = profile.GetStandardGravitationalParameter();
                positionBias[i] = profile.GetPositionBias();
                velocityBias[i] = profile.velocityBias;
            }
        }

        private Vector3 CalculateAccelaration(int sourceIndex, Rigidbody target)
        {
            var aScale = Mathf.Pow(timeScale, 2.0f) / lengthScale;

            var sourcePosition = gravitySources[sourceIndex].position;
            var r = (sourcePosition - (target.worldCenterOfMass + positionBias[sourceIndex])) * lengthScale;
            var rr = r.sqrMagnitude;
            var ga = (float)(standardGravitationalParameter[sourceIndex] / rr) * r.normalized;
            if (highPrecisionMode) return ga * aScale;

            var vScale = timeScale * lengthScale;
            var v = (target.velocity + velocityBias[sourceIndex]) / vScale;
            var w = 1 / rr * Vector3.Cross(-r, v);
            var ca = -Vector3.Cross(w, Vector3.Cross(w, -r));
            return (ca + ga) * aScale;
        }

        private Vector3 CalculateTargetAccelaration(Rigidbody target)
        {
            var accelaration = Vector3.zero;

            for (int j = 0; j < sourceCount; j++)
            {
                var sourceTransform = gravitySources[j];
                if (sourceTransform == null) continue;
                accelaration += CalculateAccelaration(j, target);
            }

            return accelaration;
        }
        #endregion

        #region Unity Events
        private GameObject[] targetObjects;
        private int targetCount, sourceCount;
        private float[] standardGravitationalParameter;
        private Vector3[] positionBias, velocityBias;
        private void Start()
        {
            Initialize();
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

                target.AddForce(CalculateTargetAccelaration(target), ForceMode.Acceleration);
            }
        }
        #endregion

        #region Udon Events
        #endregion

        #region Custom Events
        public void RespawnTargets()
        {
            foreach (var target in targets)
            {
                var udon = (UdonBehaviour)target.GetComponent(typeof(UdonBehaviour));
                if (udon == null) continue;
                udon.SendCustomEvent("Respawn");
            }
        }
        #endregion

        #region Activatable
        public void Activate()
        {
            active = true;
            LoadGravityProfiles();
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
            if (logger == null && useGlobalLogger) logger = GameObject.Find("_USV_Global_Logger_").GetComponent<UdonLogger>();

            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private void OnDrawGizmos()
        {
            this.UpdateProxy();
            Initialize();
            LoadGravityProfiles();

            Gizmos.color = Color.white;
            foreach (var target in GetTargets()) {
                var a = CalculateTargetAccelaration(target);
                Gizmos.DrawRay(target.worldCenterOfMass, a);
                Gizmos.DrawWireSphere(target.worldCenterOfMass + a, 0.01f);
            }
        }
#endif
    }
}
