using CardCheckAssistant.Models;

using Elatec.NET;
using Elatec.NET.Cards;
using Elatec.NET.Cards.Mifare;
using Elatec.NET.Helpers.ByteArrayHelper.Extensions;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CardCheckAssistant.Services
{
    internal class ReaderService : IDisposable
    {
        private readonly List<TWN4ReaderDevice>? readerList = new();
        private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        private TWN4ReaderDevice? readerDevice;
        private GenericChipModel? hfTag;
        private GenericChipModel? lfTag;
        private GenericChipModel? legicTag;

        private static object syncRoot = new object();
        private static ReaderService instance;

        public string ReaderUnitName
        {
            get; set;
        }
        public string ReaderUnitVersion
        {
            get; set;
        }

        public static ReaderService Instance
        {
            get
            {
                lock (syncRoot)
                {
                    if (instance == null)
                    {
                        instance = new ReaderService();
                        return instance;
                    }
                    else
                    {
                        return instance;
                    }
                }
            }
        }

        private ReaderService()
        {
            try
            {
                readerList = TWN4ReaderDevice.Instance;

                if (readerList != null && readerList.Count > 0)
                {
                    readerDevice = readerList[0];
                }

                GenericChip ??= new GenericChipModel();
            }
            catch (Exception e)
            {
                eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
            }
        }

        public bool MoreThanOneReaderFound => readerList?.Count > 1;

        public bool? IsConnected => readerDevice != null ? readerDevice.IsConnected : null;

        public GenericChipModel? GenericChip
        {
            get; private set;
        }

        public async Task Disconnect()
        {
            if (readerDevice != null && readerDevice.IsConnected)
            {
                await readerDevice.DisconnectAsync();
            }
        }

        public async Task Connect()
        {
            if (readerDevice != null && !readerDevice.IsConnected)
            {
                await readerDevice.ConnectAsync();
            }
            else if (readerDevice == null)
            {
                var readerList = TWN4ReaderDevice.Instance;

                if (readerList != null && readerList.Count > 0)
                {
                    readerDevice = readerList[0];
                }

                GenericChip ??= new GenericChipModel();
            }
        }

        public async Task<int> ReadChipPublic()
        {
            try
            {
                if (readerDevice != null)
                {
                    if (!readerDevice.IsConnected)
                    {
                        await readerDevice.ConnectAsync();
                    }

                    await readerDevice.SetTagTypesAsync(LFTagTypes.NOTAG, HFTagTypes.AllHFTags & ~HFTagTypes.LEGIC);
                    var tmpTag = await readerDevice.GetSingleChipAsync();

                    if (tmpTag != null && tmpTag.ChipType == ChipType.MIFARE)
                    {

                        if ((MifareChipSubType)((byte)(tmpTag as MifareChip).SubType & 0xF0) == MifareChipSubType.MifareClassic)
                        {
                            hfTag = new GenericChipModel(tmpTag.UIDHexString, (tmpTag as MifareChip).SubType, ByteArrayConverter.GetStringFrom((tmpTag as MifareChip).SAK), ByteArrayConverter.GetStringFrom((tmpTag as MifareChip).ATS));
                        }
                        else if ((MifareChipSubType)((byte)(tmpTag as MifareChip).SubType & 0x40) == MifareChipSubType.DESFire)
                        {
                            hfTag = new GenericChipModel(tmpTag.UIDHexString, (tmpTag as MifareChip).SubType, ByteArrayConverter.GetStringFrom((tmpTag as MifareChip).SAK), ByteArrayConverter.GetStringFrom((tmpTag as MifareChip).ATS), ByteArrayConverter.GetStringFrom((tmpTag as MifareChip).VersionL4));
                        }
                        else
                        {
                            hfTag = new GenericChipModel(tmpTag.UIDHexString, tmpTag.ChipType);
                        }
                    }
                    else if (tmpTag == null)
                    {
                        hfTag = null;
                    }

                    await readerDevice.SetTagTypesAsync(LFTagTypes.AllLFTags, HFTagTypes.NOTAG);
                    tmpTag = await readerDevice.GetSingleChipAsync();
                    lfTag = tmpTag != null ? new GenericChipModel(tmpTag.UIDHexString, tmpTag.ChipType) : null;

                    await readerDevice.SetTagTypesAsync(LFTagTypes.NOTAG, HFTagTypes.LEGIC);
                    tmpTag = await readerDevice.GetSingleChipAsync();
                    legicTag = tmpTag != null ? new GenericChipModel(tmpTag.UIDHexString, tmpTag.ChipType) : null;

                    await readerDevice.SetTagTypesAsync(LFTagTypes.NOTAG, HFTagTypes.AllHFTags);

                    if (!string.IsNullOrWhiteSpace(hfTag?.UID) && GenericChip?.UID == hfTag.UID)
                    {
                        return 1;
                    }

                    if (!string.IsNullOrWhiteSpace(hfTag?.UID) && (GenericChip?.UID != hfTag.UID))
                    {
                        GenericChip = new GenericChipModel();

                        if (!string.IsNullOrWhiteSpace(hfTag?.UID))
                        {
                            GenericChip = new GenericChipModel(hfTag);

                            if (GenericChip.Childs == null)
                            {
                                GenericChip.Childs = new List<GenericChipModel>();
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(lfTag?.UID))
                    {
                        GenericChip?.Childs.Add(lfTag);
                    }

                    if (!string.IsNullOrEmpty(legicTag?.UID))
                    {
                        GenericChip?.Childs.Add(legicTag);
                    }
                    if(hfTag == null && lfTag == null && legicTag == null)
                    {
                        await readerDevice.BeepAsync(10, 2500, 50, 0);
                        GenericChip = null;

                        return 3;
                    }
                }

                else
                {
                    return 1;
                }
            }
            catch (Exception e)
            {
                if (readerDevice != null)
                {
                    readerDevice.Dispose();
                }

                eventLog.WriteEntry(e.Message, EventLogEntryType.Error);

                throw;
            }

            return 0;
        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (readerDevice != null && readerDevice.IsConnected)
                {
                    readerDevice.DisconnectAsync().GetAwaiter().GetResult();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            _disposed = false;
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool _disposed;
    }

}
