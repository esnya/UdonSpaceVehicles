
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.Udon;

namespace UdonSpaceVehicles {
    [CustomName("USV Gravity Profile")]
    [HelpMessage("Provide parameters of a gravitational object such as a planet.")]
    [DefaultExecutionOrder(-10)]
    public class GravityProfile : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("Orbital")]
        [Tooltip("kg")] public float mass = 5.9726e+24f;
        [Tooltip("m")] public float equatorialRadius = 6371e+3f;
        [Tooltip("m")] public float altitudeBias = 350e+3f;
        [Tooltip("m/s")] public Vector3 velocityBias = Vector3.forward * 7701;
        public float G = 6.67430e-11f;
        #endregion

        #region Unity Events
        #endregion

        #region Custom Events
        public Vector3 GetPositionBias()
        {
            return Vector3.up * (equatorialRadius + altitudeBias);
        }

        public float GetStandardGravitationalParameter()
        {
            return G * mass;
        }
        #endregion

/*
        [HelpBox("Set None to use the profile attached to \"_USV_Global_Profile_\"")] public GravityProfile gravityProfile;

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
*/
    }
}
