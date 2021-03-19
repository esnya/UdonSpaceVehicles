
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace UdonSpaceVehicles
{
    [CustomName("USV RCS Controller")]
    public class RCSController : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("Contoller Inputs")]

        public ControllerInput joystick;
        public ControllerInput throttle;
        public string vrTranslationButton = "Oculus_CrossPlatform_PrimaryIndexTrigger";

        [Space]
        [SectionHeader("Attitude Stabilizer")]
        public bool enableAttitudeStabilizer = true;
        [RangeSlider(0.0f, 1.0f)] public float rotationStabilizer = 1.0f;
        [Horizontal("Rotation Filter")] [Toggle] public bool stabilizeRotationX = true, stabilizeRotationY = true, stabilizeRotationZ = true;
        [RangeSlider(0.0f, 1.0f)] public float translationStabilizer = 0.7f;
        [Horizontal("Translation Filter")] [Toggle] public bool stabilizeTranslationX = true, stabilizeTranslationY = true, stabilizeTranslationZ = false;
        [RangeSlider(0.0f, 1.0f)] public float inputDeadZone = 0.1f;
        [Horizontal("Feedback Gain")] public float pGain = 100.0f; //, dGain = 0.0f;


        [HideInInspector] public Vector3 rotation;
        [HideInInspector] public Vector3 translation;
        #endregion

        #region Logics
        private Vector3 GetStabilizationFilter(bool x, bool y, bool z, float scale)
        {
            return new Vector3(x ? 1.0f : 0.0f, y ? 1.0f : 0.0f, z ? 1.0f : 0.0f) * scale;
        }

        private float Clamp11(float value)
        {
            return Mathf.Clamp(value, -1.0f, 1.0f);
        }

        private float StabilizeAxis(float input, float velocity)
        {

            if (Mathf.Abs(input) >= inputDeadZone) return input;
            return -velocity * pGain;
        }

        private Vector3 Stabilize(Vector3 input, Vector3 velocity, Vector3 filter)
        {
            return new Vector3(
                Clamp11(StabilizeAxis(input.x, velocity.x)) * filter.x,
                Clamp11(StabilizeAxis(input.y, velocity.y)) * filter.y,
                Clamp11(StabilizeAxis(input.z, velocity.z)) * filter.z
            );
        }

        private readonly Vector3 joystickTranslationFilter = new Vector3(1, 1, 0);
        private void StabilizerUpdate()
        {
            var joystickInput=joystick.input;
            var vrTranslate = vr && Input.GetAxis(vrTranslationButton) > 0.5f;
            rotation = Stabilize(vrTranslate ? Vector3.Scale(joystickInput, Vector3.up) : joystickInput, transform.InverseTransformVector(rootRigidbody.angularVelocity), rotationFilter);
            translation = Stabilize(vrTranslate ? (throttle.input - new Vector3(joystickInput.z, joystickInput.x, 0)) : throttle.input, transform.InverseTransformVector(rootRigidbody.velocity), translationFilter);
        }
        #endregion

        #region Unity Events
        private Vector3 rotationFilter, translationFilter;
        private Rigidbody rootRigidbody;
        private void Start()
        {
            rotationFilter = GetStabilizationFilter(stabilizeRotationX, stabilizeRotationY, stabilizeRotationZ, rotationStabilizer);
            translationFilter = GetStabilizationFilter(stabilizeTranslationX, stabilizeTranslationY, stabilizeTranslationZ, translationStabilizer);
            rootRigidbody = GetComponentInParent<Rigidbody>();

            Log("Info", "Initialized");
        }

        private void Update()
        {
            if (!active) return;
            if (enableAttitudeStabilizer) StabilizerUpdate();
            else
            {
                rotation = Vector3.zero;
                translation = Vector3.zero;
            }
        }
        #endregion

        #region Udon Events
        private bool vr;
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (player.isLocal)
            {
                vr = player.IsUserInVR();
                Log("Info", $"VR: {vr}");
            }
        }
        #endregion

        #region Custom Events
        #endregion

        #region Internal Logics
        #endregion

        #region Activatable
        bool active = false;

        public void Activate()
        {
            active = true;

            Log("Info", "Activated");
        }

        public void Deactivate()
        {
            active = false;

            rotation = Vector3.zero;
            translation = Vector3.zero;

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
    }
}
