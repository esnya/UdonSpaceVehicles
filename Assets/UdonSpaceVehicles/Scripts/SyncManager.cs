
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonSpaceVehicles
{
    public class SyncManager : UdonSharpBehaviour
    {
        #region Public Variables
        [ListView("Event Listners")][UTEditor] public Object[] udonBehaviours = {};
        [ListView("Event Listners")][UTEditor] public uint[] bitmaskList = {};
        public string prevValueName = "prevValue";
        public string valueChangeEvent = "_SM_ValueChanged";
        public string nextValueName = "nextValue";
        public string ownershipTranssferedEvent = "_SM_OwnershipTranssfered";
        #endregion

        #region Internal Variables
        [UdonSynced][HideInInspector] public uint syncValue0;
        #endregion

        #region Unity Events
        #endregion

        #region Udon Events
        uint prevValue0;
        public override void OnDeserialization()
        {
            var length = GetEventListnerCount();
            for (int i = 0; i < length; i++) {
                var udon = (UdonBehaviour)udonBehaviours[i];
                var bitmask = bitmaskList[i];

                if ((prevValue0 & bitmask) == (syncValue0 & bitmask)) continue;

                udon.SetProgramVariable(prevValueName, prevValue0);
                udon.SetProgramVariable(nextValueName, syncValue0);
                udon.SendCustomEvent(valueChangeEvent);
            }

            prevValue0 = syncValue0;
        }

        public override void OnOwnershipTransferred()
        {
            var length = GetEventListnerCount();
            for (int i = 0; i < length; i++) {
                ((UdonBehaviour)udonBehaviours[i]).SendCustomEvent(ownershipTranssferedEvent);
            }
        }
        #endregion

        #region Custom Events
        public void AddEventListener(UdonBehaviour eventListener, uint bitmask)
        {
            var prevLength = GetEventListnerCount();
            var nextLength = prevLength + 1;
            var nextUdonBehavours = new Object[nextLength];
            var nextBitmaskList = new uint[nextLength];

            nextUdonBehavours[prevLength] = eventListener;
            nextBitmaskList[prevLength] = bitmask;
            for (int i = 0; i < prevLength; i++) {
                nextUdonBehavours[i] = udonBehaviours[i];
                nextBitmaskList[i] = bitmaskList[i];
            }

            udonBehaviours = nextUdonBehavours;
            bitmaskList = nextBitmaskList;
        }
        #endregion

        #region Internal Logics
        int GetEventListnerCount()
        {
            if (udonBehaviours == null || bitmaskList == null) return 0;
            return Mathf.Min(udonBehaviours.Length, bitmaskList.Length);
        }
        #endregion

        #region Accessor
        uint GetValue(uint packed, int byteOffset, uint bitmask)
        {
            return (packed >> byteOffset & bitmask);
        }
        uint SetValue(uint packed, int byteOffset, uint bitmask, uint value)
        {
            var mask = bitmask << byteOffset;
            return packed & mask | value & bitmask << byteOffset;
        }

        bool GetBool(uint packed, int byteOffset)
        {
            return GetValue(packed, byteOffset, 0x01) != 0;
        }
        uint SetBool(uint packed, int byteOffset, bool value)
        {
            return SetValue(packed, byteOffset, 0x1, value ? 1u : 0u);
        }

        byte GetByte(uint packed, int byteOffset)
        {
            return (byte)GetValue(packed, byteOffset, 0xff);
        }
        uint SetByte(uint packed, int byteOffset, byte value)
        {
            return SetValue(packed, byteOffset, 0xff, value);
        }
        #endregion

    }
}
