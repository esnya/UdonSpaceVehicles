
using System;
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
        [SectionHeader("Mode")][Popup("GetModes")] public string mode;
        [HideIf("HideForward")] public bool signed = true;
        [HideIf("HideForward")] public Vector3 forward = Vector3.forward;
        [HideIf("HIdeForward")] public Vector3 axisScale = Vector3.one;
        [HideIf("HideAxis")] public Vector3 axis = Vector3.forward;
        [SectionHeader("Format")] public string prefix = "";
        public string suffix = " <size=75%>{}</size>";
        #endregion

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
        }
        #endregion

        #region Logics
        private const int Speed = 0, Velocity = 1, Altitude = 2, Gravity = 3, OrbitalSpeed = 4, SemiMajorAxis = 5, OrbitalInclination = 6, OrbitalEccentricity = 7, PericenterAltitude = 8, ApocenterAltitude = 9;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private string[] GetModes() => Enumerable.Range(0, GetModeTable().Length / 3).Select(i => GetModeTable()[i * 3]).ToArray();
#endif

        private string[] GetModeTable() => new[] {
            "Speed",                "KMpH", "f2",
            "Axis Speed",           "KMpH", "f2",
            "Altitude",             "m",    "f0",
            "Gravity",              "G",    "f2",
            "Orbital Speed",        "KMpH", "f2",
            "Semi Major Axis",      "m",    "f0",
            "Orbital Inclination",  "°",   "f2",
            "Orbital Eccentricity", "",     "f2",
            "Pericenter Altitude",  "m",    "f0",
            "Apocenter Altitude",   "m",    "f0",
        };
        private string format = "f";
        private TextMeshPro text;
        private void Initialize()
        {
            text = GetComponent<TextMeshPro>();

            var table = GetModeTable();
            for (int i = 0; i < table.Length / 3; i++) {
                if (mode == table[i * 3]) {
                    _mode = i;
                    break;
                }
            }
            _suffix = suffix.Replace("{}", table[_mode * 3 + 1]);
            format = table[_mode * 3 + 2];
            value = -999.99f;
            UpdateText();
        }

        private void UpdateText()
        {
            text.text = $"{prefix}{value.ToString(format)}{_suffix}";
        }

        private float CalcStandardGravitionalParameter() => G * planetMass; // μ
        private Vector3 CalcVelocity() => target.velocity + velocityBias; // v
        private Vector3 CalcPosition() => target.position + positionBias;
        private Vector3 CalcDistanceVector() => Vector3.up * CalcPosition().y; // r
        private Vector3 CalcSpecificAngularMomentum() => Vector3.Cross(CalcVelocity(), CalcDistanceVector()); // h
        private float CalcSpecificOrbitalEnergy() => CalcVelocity().sqrMagnitude / 2 - CalcStandardGravitionalParameter() / CalcPosition().y; // ε
        private float CalcSemiMajorAxis() => - CalcStandardGravitionalParameter() / (2 * CalcSpecificOrbitalEnergy()); // a
        private float CalcOrbitalEccentricity() => Mathf.Sqrt(1 + 2 * CalcSpecificOrbitalEnergy() * CalcSpecificAngularMomentum().sqrMagnitude / Mathf.Pow(CalcStandardGravitionalParameter(), 2)); // e
        private float CalcPericenterAltitude() => (1 - CalcOrbitalEccentricity()) * CalcSemiMajorAxis() - positionBias.y + altitudeBias; // r_per
        private float CalcApocenterAltitude() => (1 + CalcOrbitalEccentricity()) * CalcSemiMajorAxis() - positionBias.y + altitudeBias; // r_ap
        #endregion

        #region Unity Events
        private int _mode;
        private Transform targetTransform;
        private string _suffix;
        private void Start()
        {
            if (findTargetFromParent) target = GetComponentInParent<Rigidbody>();
            targetTransform = target.transform;
            Initialize();

            Log("Info", "Initialized");
        }

        private float value;
        private Vector3 prevVelocity;
        private void FixedUpdate()
        {
            if (!active) return;

            switch (_mode)
            {
                case Speed:                 value = target.velocity.magnitude * (signed ? Mathf.Sign(Vector3.Dot(targetTransform.forward, target.velocity)) : 1.0f) * 3.6f; break;
                case Velocity:              value = Vector3.Dot(targetTransform.TransformDirection(axis), target.velocity) * 3.6f; break;
                case Altitude:              value = targetTransform.position.y + altitudeBias; break;
                case Gravity:
                    var a = target.velocity - prevVelocity;
                    value = a.magnitude * Mathf.Sign(a.y) / Time.fixedDeltaTime / 9.8f;
                    prevVelocity = target.velocity;
                    break;
                case OrbitalSpeed:          value = CalcVelocity().magnitude; break;
                case SemiMajorAxis:         value = CalcSemiMajorAxis(); break;
                case OrbitalInclination:
                    var v = target.velocity + velocityBias;
                    value = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
                    break;
                case OrbitalEccentricity:   value = CalcOrbitalEccentricity(); break;
                case PericenterAltitude:    value = CalcPericenterAltitude(); break;
                case ApocenterAltitude:     value = CalcApocenterAltitude(); break;
            }
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
            OrbitalObject_Activate();
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
            if (logger == null && useGlobalLogger) logger = (UdonLogger)GameObject.Find("_USV_Global_Logger_").GetComponent(typeof(UdonBehaviour));

            if (logger != null) logger.Log(level, gameObject.name, message);
            else Debug.Log($"{level} [{gameObject.name}] {message}");
        }
        #endregion

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private bool HideForward() => mode != "Speed";
        private bool HideAxis() => mode != "Axis Speed";
        private bool HidePositionOffset() => mode != "Altitude";
#endif
    }
}
