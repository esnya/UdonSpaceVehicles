
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
        #endregion

        #region Internal Variables
        [UdonSynced] [HideInInspector] public uint syncValue0, syncValue1, syncValue2;
        #endregion

        #region Logics
        private Object[] AppendObject(Object[] array, Object item) {
            var resized = new Object[array.Length + 1];
            System.Array.Copy(array, resized, array.Length);
            resized[array.Length] = item;
            return resized;
        }
        private string[] AppendString(string[] array, string item) {
            var resized = new string[array.Length + 1];
            System.Array.Copy(array, resized, array.Length);
            resized[array.Length] = item;
            return resized;
        }
        private uint[] AppendUint(uint[] array, uint item) {
            var resized = new uint[array.Length + 1];
            System.Array.Copy(array, resized, array.Length);
            resized[array.Length] = item;
            return resized;
        }

        private void SetSyncValue(uint bank, uint value)
        {
            // Log($"Set sync value bank:{bank} value:{value}");
            switch (bank) {
                case 0:
                    syncValue0 = value;
                    break;
                case 1:
                    syncValue1 = value;
                    break;
                case 2:
                    syncValue2 = value;
                    break;
            }
        }
        private void SetPrevValue(uint bank, uint value)
        {
            switch (bank) {
                case 0:
                    prevValue0 = value;
                    break;
                case 1:
                    prevValue1 = value;
                    break;
                case 2:
                    prevValue2 = value;
                    break;
            }
        }
        private uint GetSyncValue(uint bank)
        { 
            switch (bank) {
                case 0:
                    return syncValue0;
                case 1:
                    return syncValue1;
                case 2:
                    return syncValue2;
            }
            return 0;
        }
        private uint GetPrevValue(uint bank)
        {
            switch (bank) {
                case 0:
                    return prevValue0;
                case 1:
                    return prevValue1;
                case 2:
                    return prevValue2;
            }
            return 0;
        }
        #endregion

        #region Unity Events
        #endregion

        #region Udon Events
        uint prevValue0, prevValue1, prevValue2;
        public override void OnDeserialization()
        {
            // Log($"{syncValue0} {syncValue1} {syncValue2}");
            for (int i = 0; i < eventListenerCount; i++)
            {
                var udon = (UdonBehaviour)eventListeners[i];
                if (udon == null) continue;

                var bank = banks[i];
                var bitmask = bitmaskList[i];

                var syncValue = GetSyncValue(bank);
                var prevValue = GetPrevValue(bank);

                if ((prevValue & bitmask) == (syncValue & bitmask)) continue;

                udon.SetProgramVariable(prevValueNames[i], prevValue);
                udon.SetProgramVariable(syncValueNames[i], syncValue);
                udon.SendCustomEvent(valueChangeEvents[i]);
            }

            SetPrevValue(0, GetSyncValue(0));
            SetPrevValue(1, GetSyncValue(1));
            SetPrevValue(2, GetSyncValue(2));
        }

        #endregion

        #region Custom Events
        private int eventListenerCount;
        private Component[] eventListeners = {};
        private uint[] banks = {}, bitmaskList = {};
        private string[] syncValueNames = {}, prevValueNames = {}, valueChangeEvents = {};
        public void AddEventListener(UdonSharpBehaviour eventListener, uint bank, uint bitmask, string syncValueName, string prevValueName, string valueChangeEvent)
        {
            eventListeners = (Component[])AppendObject(eventListeners, eventListener);
            banks = AppendUint(banks, bank);
            bitmaskList = AppendUint(bitmaskList, bitmask);
            syncValueNames = AppendString(syncValueNames, syncValueName);
            prevValueNames = AppendString(prevValueNames, prevValueName);
            valueChangeEvents = AppendString(valueChangeEvents, valueChangeEvent);

            eventListenerCount = eventListeners.Length;

            Log($"{eventListener} listening bank:{bank} bitmask:{bitmask}");;
        }

        public bool GetBool(uint bank, int byteOffset)
        {
            return UnpackBool(GetSyncValue(bank), byteOffset);
        }
        public void SetBool(uint bank, int byteOffset, bool value)
        {
            // Log($"Set sync value bank:{bank} byteOffset:{value} value:{value}");
            SetSyncValue(bank, PackBool(GetSyncValue(bank), byteOffset, value));
        }

        public byte GetByte(uint bank, int byteOffset)
        {
            return (byte)UnpackValue(GetSyncValue(bank), byteOffset, 0xff);
        }
        public void SetByte(uint bank, int byteOffset, byte value)
        {
            SetSyncValue(bank, PackValue(GetSyncValue(bank), byteOffset, 0xff, value));
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

        private byte UnpackByte(uint packed, int byteOffset)
        {
            return (byte)UnpackValue(packed, byteOffset, 0xff);
        }

        private uint PackByte(uint packed, int byteOffset, byte value)
        {
            return PackValue(packed, byteOffset, 0xff, value);
        }
        #endregion

        
        #region Activatable
        bool active;
        public void Activate()
        {
            active = true;
            Log("Activated");
        }

        public void Deactivate()
        {
            active = false;
            Log("Deactivated");
        }
        #endregion

        #region Logger
        private void Log(string log)
        {
            Debug.Log($"[{gameObject.name}] {log}");
        }
        #endregion
    }
}
