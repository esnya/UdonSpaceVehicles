using TMPro;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Linq;
using UdonSharpEditor;
#endif

namespace UdonSpaceVehicles
{
    [CustomName("USV HUD Text Driver")]
    [HelpMessage("Drives text for the instruments.")]
    [RequireComponent(typeof(TextMeshPro))]
    public class HUDTextDriver : UdonSharpBehaviour
    {

        #region Public Variables
        public bool findTargetFromParent = true;
        [HideIf("@findTargetFromParent")] public Rigidbody target;
        [HelpBox("Set None to use the profile attached to \"_USV_Global_Profile_\"")] public GravityProfile gravityProfile;
        [Popup("GetModes")] public string[] enableValues = { "SPEED" };

        [TextArea] public string format = "{SPEED}<size=75%>KMpH</size>";
        #endregion

        #region Gravitational Object
        float standardGravitationalParameter, altitudeBias;
        Vector3 positionBias, velocityBias;
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
            standardGravitationalParameter = profile.GetStandardGravitationalParameter();
            positionBias = profile.GetPositionBias();
            velocityBias = profile.velocityBias;
            altitudeBias = profile.altitudeBias;
        }
        #endregion

        #region Logics
        private readonly string[] definitions = {
            "SPEED", "f2",
            "X_SPEED", "f2",
            "Y_SPEED", "f2",
            "Z_SPEED", "f2",
            "ALTITUDE","f0",
            "ACCELERATION", "f2",
            "ORBITAL_SPEED", "f2",
            "SEMI_MAJOR_AXIS", "f0",
            "ORBITAL_INCLINATION", "f4",
            "ORBITAL_ECCENTRICITY", "f2",
            "PERICENTER_ALTITUDE", "f0",
            "APOCENTER_ALTITUDE", "f0",
        };
        private int definitionCount;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private string[] GetModes() => Enumerable.Range(0, definitions.Length / 2).Select(i => definitions[i * 2]).ToArray();
        private string GetFormatPlaceholders()
        {
            return string.Join("\n", GetModes().Select(mode => $"%{mode}%"));
        }
#endif

        private bool[] useFlags;
        private float[] values;
        private TextMeshPro text;
        private void Initialize()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            targetTransform = target.transform;

            text = GetComponent<TextMeshPro>();

            definitionCount = definitions.Length / 2;
            values = new float[definitionCount];
            useFlags = new bool[definitionCount];
            for (int i = 0; i < definitionCount; i++) useFlags[i] = false;
            foreach (var name in enableValues)
            {
                for (int j = 0; j < definitionCount; j++) {
                    if (definitions[j * 2] == name)
                    {
                        useFlags[j] = true;
                        break;
                    }
                }
            }

            UpdateText();
        }

        private void UpdateText()
        {
            var tmp = format;

            for (int i = 0; i < definitionCount; i++)
            {
                if (!useFlags[i]) continue;
                tmp = tmp.Replace($"%{definitions[i * 2]}%", values[i].ToString(definitions[i * 2 + 1]));
            }

            text.text = tmp;
        }

        private Vector3 prevVelocity;
        private float CalcAccelerationG()
        {
            var a = target.velocity - prevVelocity;
            var value = a.magnitude * Mathf.Sign(a.y) / Time.fixedDeltaTime / 9.8f;
            prevVelocity = target.velocity;
            return value;
        }

        private float CalcStandardGravitionalParameter() => standardGravitationalParameter; // μ
        private Vector3 CalcVelocity() => target.velocity + velocityBias; // v
        private Vector3 CalcPosition() => target.position + positionBias;
        private Vector3 CalcDistanceVector() => Vector3.up * CalcPosition().y; // r
        private Vector3 CalcSpecificAngularMomentum() => Vector3.Cross(CalcVelocity(), CalcDistanceVector()); // h
        private float CalcSpecificOrbitalEnergy() => CalcVelocity().sqrMagnitude / 2 - CalcStandardGravitionalParameter() / CalcPosition().y; // ε
        private float CalcSemiMajorAxis() => - CalcStandardGravitionalParameter() / (2 * CalcSpecificOrbitalEnergy()); // a
        private float CalcOrbitalEccentricity() => Mathf.Sqrt(1 + 2 * CalcSpecificOrbitalEnergy() * CalcSpecificAngularMomentum().sqrMagnitude / Mathf.Pow(CalcStandardGravitionalParameter(), 2)); // e
        private float CalcPericenterAltitude() => (1 - CalcOrbitalEccentricity()) * CalcSemiMajorAxis() - positionBias.y + altitudeBias; // r_per
        private float CalcApocenterAltitude() => (1 + CalcOrbitalEccentricity()) * CalcSemiMajorAxis() - positionBias.y + altitudeBias; // r_ap
        private float CalcOrbitalInclination()
        {
            var v = target.velocity + velocityBias;
            return Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
        }
        #endregion

        #region Unity Events
        private Transform targetTransform;
        private void Start()
        {
            Initialize();

            Log("Info", "Initialized");
        }

        private void FixedUpdate()
        {
            if (!active) return;

            if (useFlags[0]) values[0] = target.velocity.magnitude * Mathf.Sign(Vector3.Dot(targetTransform.forward, target.velocity)) * 3.6f;
            if (useFlags[1]) values[1] = Vector3.Dot(targetTransform.right, target.velocity) * 3.6f;
            if (useFlags[2]) values[2] = Vector3.Dot(targetTransform.up, target.velocity) * 3.6f;
            if (useFlags[3]) values[3] = Vector3.Dot(targetTransform.forward, target.velocity) * 3.6f;
            if (useFlags[4]) values[4] = targetTransform.position.y + altitudeBias;
            if (useFlags[5]) values[5] = CalcAccelerationG();
            if (useFlags[6]) values[6] = CalcVelocity().magnitude * 3.6f;
            if (useFlags[7]) values[7] = CalcSemiMajorAxis();
            if (useFlags[8]) values[8] = CalcOrbitalInclination();
            if (useFlags[9]) values[9] = CalcOrbitalEccentricity();
            if (useFlags[10]) values[10] = CalcPericenterAltitude();
            if (useFlags[11]) values[11] = CalcApocenterAltitude();
        }

        private void Update()
        {
            if (active) UpdateText();
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
            Initialize();
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
        private void OnDrawGizmosSelected() {
            this.UpdateProxy();
            if (text == null || target == null) Initialize();
            LoadGravityProfile(gravityProfile);
            FixedUpdate();
            UpdateText();
        }
#endif
    }
}
