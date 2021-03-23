
using System.ComponentModel;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

/*
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
*/

namespace UdonSpaceVehicles {
    [CustomName("USV Global Settings")]
    [HelpMessage("Put as a single GameObject named \"_USV_Global_Settings_\" to provide global setting values for other components.")]
    // [OnValuesChanged("PreCompute")]
    public class GlobalSettings : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("Orbital")]
        [Tooltip("kg")] public float planetMass = 5.9726e+24f;
        [Tooltip("m")] public float equatorialRadius = 6371e+3f;
        [Tooltip("m")] public float altitudeBias = 350e+3f;
        [Disabled] public Vector3 positionBias;
        [Tooltip("m/s")] public Vector3 velocityBias = Vector3.forward * 7990;
        public float G = 6.67430e-11f;
        [Disabled] public float planetCoG;
        #endregion

        #region Unity Events
        private void Start()
        {
            PreCompute();
            Log("Info", "Initialized");
        }
        #endregion

        #region Custom Events
        public void PreCompute()
        {
#if !COMPILER_UDONSHARP && UNITY_EDITOR
            this.UpdateProxy();
#endif


            positionBias = Vector3.up * (equatorialRadius + altitudeBias);
            planetCoG = G * planetMass;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
            this.ApplyProxyModifications();
#endif
        }
        #endregion

/*
        private GlobalSettings FindGlobalSettings()
        {
            var o = GameObject.Find("_USV_Global_Settings_");
            if (o == null) return null;
            return (GlobalSettings)o.GetComponent(typeof(UdonBehaviour));
        }
*/

        #region Logger
        [SectionHeader("Udon Logger")] public bool useGlobalLogger = true;
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
