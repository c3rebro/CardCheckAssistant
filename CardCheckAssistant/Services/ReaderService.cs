using CardCheckAssistant.Models;

using Elatec.NET;
using Elatec.NET.Cards;
using Elatec.NET.Cards.Mifare;

using Log4CSharp;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CardCheckAssistant.Services
{
    public class ReaderService : IDisposable
    {
        private readonly EventLog eventLog = new("Application", ".", Assembly.GetEntryAssembly().GetName().Name);

        public string ReaderUnitName
        {
            get; set;
        }
        public string ReaderUnitVersion
        {
            get; set;
        }

        private TWN4ReaderDevice readerDevice;

        private GenericChipModel hfTag;
        private GenericChipModel lfTag;
        private GenericChipModel legicTag;

        private bool _disposed;

        #region Constructor

        public ReaderService()
        {
            ReaderUnitName = "";
            ReaderUnitVersion = "1.0";
            hfTag = new GenericChipModel();
            lfTag = new GenericChipModel();
            legicTag = new GenericChipModel();
            readerDevice ??= TWN4ReaderDevice.Instance[0];
            DetectedChips ??= new List<GenericChipModel>();
        }

        private async Task Initialize()
        {
            if (DetectedChips == null)
            {
                DetectedChips = new List<GenericChipModel>();
            }

            try
            {
                var readerList = TWN4ReaderDevice.Instance;

                if (readerList != null && readerList.Count > 0)
                {
                    readerDevice = readerList.FirstOrDefault();

                    if (readerDevice.IsConnected)
                    {
                        ReaderUnitVersion = await readerDevice.GetVersionStringAsync();
                    }
                    else if (readerDevice != null && readerDevice.AvailableReadersCount >= 1 && !readerDevice.IsConnected)
                    {
                        if (await readerDevice.ConnectAsync())
                        {
                            ReaderUnitVersion = await readerDevice.GetVersionStringAsync();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
            }
        }

        #endregion

        #region Common

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected => readerDevice?.IsConnected == true;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReadChipPublic()
        {
            try
            {
                if (readerDevice == null)
                {
                    await Initialize();
                }

                if (readerDevice != null && !readerDevice.IsConnected)
                {
                    await readerDevice.ConnectAsync();
                }

                // Clear previous detections
                DetectedChips.Clear();

                // Read different tag types and add detected chips to the list
                await ReadAndAddDetectedChips(LFTagTypes.NOTAG, HFTagTypes.AllHFTags & ~HFTagTypes.LEGIC);
                await ReadAndAddDetectedChips(LFTagTypes.AllLFTags, HFTagTypes.NOTAG);
                await ReadAndAddDetectedChips(LFTagTypes.NOTAG, HFTagTypes.LEGIC);

                return 0;
            }
            catch (Exception e)
            {
                eventLog.WriteEntry(e.Message, EventLogEntryType.Error);
                return 1;
            }
        }

        private async Task ReadAndAddDetectedChips(LFTagTypes lfTagType, HFTagTypes hfTagType)
        {
            await readerDevice.SetTagTypesAsync(lfTagType, hfTagType);
            var tmpTag = await readerDevice.GetSingleChipAsync();

            if (tmpTag != null)
            {
                CCChipType chipType = CCChipType.NOTAG;

                if (tmpTag is MifareChip mifareChip)
                {
                    chipType = TranslateMifareSubTypeToCCChipType(mifareChip.SubType);
                }
                else
                {
                    chipType = (CCChipType)((int)tmpTag.ChipType);
                }

                var chipModel = new GenericChipModel
                {
                    UID = tmpTag.UIDHexString,
                    ChipType = chipType
                };

                // Check if the tag is a MifareChip and extract relevant properties
                if (tmpTag is MifareChip mChip)
                {
                    chipModel.SAK = ByteArrayConverter.GetStringFrom(mChip.SAK);
                    chipModel.RATS = ByteArrayConverter.GetStringFrom(mChip.ATS);
                    chipModel.VersionL4 = ByteArrayConverter.GetStringFrom(mChip.VersionL4);
                }

                DetectedChips.Add(chipModel);
            }
        }

        private CCChipType TranslateMifareSubTypeToCCChipType(MifareChipSubType subType)
        {
            // Implement the translation logic based on your enum definitions
            return (CCChipType)((int)subType << 8); // Example translation
        }

        public bool MoreThanOneReaderFound => TWN4ReaderDevice.Instance.Count > 1;

        public bool ReaderPortBusy => TWN4ReaderDevice.Instance[0] == null;

        public List<GenericChipModel>? DetectedChips
        {
            get; private set;
        }

        #endregion

        protected async virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    await TWN4ReaderDevice.Instance[0].DisconnectAsync();
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
    }
}
