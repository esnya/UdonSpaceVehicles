
using System;
using System.ComponentModel.Design;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonSpaceVehicles
{
    [CustomName("USV Thrust Vector Driver")]
    [HelpMessage("Manipulate the direction of the thrust vector of engines.")]

    public class ThrustVectorDriver : UdonSharpBehaviour
    {
        #region Public Variables
        [SectionHeader("References")]
        public ControllerInput joystickInput;
        [ListView("Engines")] public Transform[] engines = { };
        [ListView("Engines")] public Vector2[] maxAngles = { };
        public float speed = 1f;
        [UdonSynced] private uint syncValue;
        #endregion

        #region Logics
        private void SetAngle(int i, Vector2 angle)
        {
            engineAngles[i] = angle;
            engines[i].localRotation = Quaternion.Euler(angle) * engineInitialRotations[i];
        }

        private float DecodeValue(bool nonZero, bool minus)
        {
            if (!nonZero) return 0;
            return minus ? -1 : 1;
        }
        #endregion

        #region Unity Events
        private int engineCount;
        private Vector3[] engineVectorX, engineVectorY;
        private Vector2[] engineAngles, targetAngles;
        private Quaternion[] engineInitialRotations;
        private void Start()
        {
            var rigidbody = GetComponentInParent<Rigidbody>();
            var center = rigidbody.worldCenterOfMass;

            engineCount = Mathf.Min(engines.Length, maxAngles.Length);
            engineVectorX = new Vector3[engineCount];
            engineVectorY = new Vector3[engineCount];
            engineAngles = new Vector2[engineCount];
            targetAngles = new Vector2[engineCount];
            engineInitialRotations = new Quaternion[engineCount];
            for (int i = 0; i < engineCount; i++)
            {
                var position = engines[i].position - center;
                var sign = Mathf.Sign(Vector3.Dot(rigidbody.transform.forward, engines[i].forward));
                engineVectorX[i] = new Vector3(
                    Mathf.Sign(Vector3.Dot(position, rigidbody.transform.forward)),
                    0,
                    -Mathf.Sign(Vector3.Dot(position, rigidbody.transform.right))
                ) * sign;
                engineVectorY[i] = new Vector3(
                    0,
                    Mathf.Sign(Vector3.Dot(position, rigidbody.transform.forward)),
                    0
                ) * sign;
                engineAngles[i] = Vector2.zero;
                engineInitialRotations[i] = engines[i].localRotation;
                Debug.Log($"{i} {engineVectorX[i]} {engineVectorY[i]}");
            }

            Log("Info", $"Initialized with {engineCount} engines");
        }

        private void UpdateActive()
        {
            var input = joystickInput.input;
            for (int i = 0; i < engineCount; i++)
            {
                var targetAngle = Vector2.Scale(new Vector2(
                    Vector3.Dot(input, engineVectorX[i]),
                    Vector3.Dot(input, engineVectorY[i])
                ), maxAngles[i]);
                targetAngles[i] = targetAngle;

                syncValue = PackBool(syncValue, i * 4 , targetAngle.x != 0);
                syncValue = PackBool(syncValue, i * 4 + 1, targetAngle.x < 0);
                syncValue = PackBool(syncValue, i * 4 + 2, targetAngle.y != 0);
                syncValue = PackBool(syncValue, i * 4 + 3, targetAngle.y < 0);
            }
        }

        bool dirty;
        private void Update()
        {
            if (active) UpdateActive();

            if (active || dirty) {
                dirty = false;
                for (int i = 0; i < engineCount; i++)
                {
                    var currentAngle = engineAngles[i];
                    var targetAngle = targetAngles[i];
                    currentAngle += Vector2.ClampMagnitude(targetAngle - currentAngle, speed);
                    if ((currentAngle - targetAngle).sqrMagnitude > 0.01f) dirty = true;

                    SetAngle(i, currentAngle);
                }
            }
        }
        #endregion

        #region UdonEvents
        private uint prevValue;
        public override void OnDeserialization()
        {
            if (prevValue == syncValue) return;

            dirty = true;
            for (int i = 0; i < engineCount; i++)
            {
                var x = UnpackBool(syncValue, i * 4);
                var xSign = UnpackBool(syncValue, i * 4 + 1);
                var y = UnpackBool(syncValue, i * 4 + 2);
                var ySign = UnpackBool(syncValue, i * 4 + 3);

                SetAngle(i, Vector2.Scale(new Vector2(
                    DecodeValue(x, xSign),
                    DecodeValue(y, ySign)
                ), maxAngles[i]));
            }

            prevValue = syncValue;
        }
        #endregion

        #region Activatable
        private bool active;
        public void Activate()
        {
            active = true;
            Log("Info", "Activated");
        }

        public void Dectivate()
        {
            active = false;

            syncValue = 0;
            for (int i = 0; i < engineCount; i++) SetAngle(i, Vector2.zero);

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

        #region Value Packing
        private uint UnpackValue(uint packed, int byteOffset, uint bitmask)
        {
            return (packed >> byteOffset) & bitmask;
        }
        private uint PackValue(uint packed, int byteOffset, uint bitmask, uint value)
        {
            return packed & ~(bitmask << byteOffset) | (value & bitmask) << byteOffset;
        }

        private bool UnpackBool(uint packed, int byteOffset)
        {
            return UnpackValue(packed, byteOffset, 0x01) != 0;
        }
        private uint PackBool(uint packed, int byteOffset, bool value)
        {
            return PackValue(packed, byteOffset, 0x1, value ? 1u : 0u);
        }
        #endregion
    }
}