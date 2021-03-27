
using UdonSharp;
using UdonSpaceVehicles;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    [CustomName("USV Damage Manager")]
    [HelpMessage("Manages vehicle damage.")]
    public class DamageManager : UdonSharpBehaviour
    {
        #region Public Variables
        public VehicleRoot vehicleRoot;
        public Animator[] animators = { };
        public float maxHP = 5.0f;
        public float collisionDamage = 0.01f;
        public AudioSource audioSource, audioSource2d;
        public AudioClip onHit, onDead, onCollision;
        [UdonSynced] private bool syncDamaged;
        #endregion

        #region Logics
        private void SetDamaged(bool value)
        {
            foreach (var animator in animators) animator.SetBool("Damaged", value);
        }

        private void Dead()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayDeadSound));
            Log("Info", "Dead");

            vehicleRoot.Respawn();
        }
        #endregion

        #region Unity Events
        private float hp;
        private void Start()
        {
            hp = maxHP;
            SetDamaged(false);
            Log("Info", "Initialized");
        }
        #endregion

        #region Udon Events
        private bool prevDamaged;
        public override void OnDeserialization()
        {
            if (syncDamaged == prevDamaged) return;
            SetDamaged(syncDamaged);
            prevDamaged = syncDamaged;
        }
        #endregion

        #region Custom Events

        public void _Respawned()
        {
            hp = maxHP;
            SetDamaged(false);
            syncDamaged = false;
        }

        public void _Hit()
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayDamageSound));
            AddDamage(1.0f);
        }

        private Collision collision;
        public void _OnCollision()
        {
            if (collision == null) return;

            var damage = collision.impulse.magnitude * Time.fixedDeltaTime * collisionDamage;

            if (damage > 0.2f) SendCustomNetworkEvent(NetworkEventTarget.All, nameof(PlayCollisionSound));

            AddDamage(damage);
        }

        public void PlayDamageSound()
        {
            audioSource.PlayOneShot(onHit);
        }
        public void PlayDeadSound()
        {
            foreach (var animator in animators) animator.SetTrigger("Dead");
            audioSource2d.PlayOneShot(onDead);
        }

        public void PlayCollisionSound()
        {
            audioSource.PlayOneShot(onCollision);
        }

        public void AddDamage(float damage)
        {
            if (!Networking.IsOwner(vehicleRoot.gameObject)) return;

            hp -= damage;

            if (damage >= 0.2f)
            {
                Log("Info", $"Damaged {damage}");
            }

            syncDamaged = hp <= Mathf.Max(1.0f, maxHP * 0.25f);
            SetDamaged(syncDamaged);

            if (hp <= 0.0f) Dead();
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
            return (packed >> byteOffset & bitmask);
        }
        private uint PackValue(uint packed, int byteOffset, uint bitmask, uint value)
        {
            var mask = bitmask << byteOffset;
            return packed & mask | value & bitmask << byteOffset;
        }

        private bool UnpackBool(uint packed, int byteOffset)
        {
            return UnpackValue(packed, byteOffset, 0x01) != 0;
        }
        private uint PackBool(uint packed, int byteOffset, bool value)
        {
            return PackValue(packed, byteOffset, 0x1, value ? 1u : 0u);
        }

        private byte UnpackByte(uint packed, int byteOffset)
        {
            return (byte)UnpackValue(packed, byteOffset, 0xff);
        }

        private uint PackByte(uint packed, int byteOffset, byte value)
        {
            return PackValue(packed, byteOffset, 0xff, value);
        }
        #endregion
    }
}
