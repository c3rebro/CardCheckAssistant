using CardCheckAssistant.Models;

using Elatec.NET;
using Elatec.NET.Model;

using Log4CSharp;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace CardCheckAssistant.Services
{
    internal class ReaderService : IDisposable
    {
        private readonly TWN4ReaderDevice? readerDevice;

        private ChipModel? hfTag;
        private ChipModel? lfTag;
        private ChipModel? legicTag;

        public ReaderService()
        {
            try
            {
                readerDevice = TWN4ReaderDevice.Instance ?? new TWN4ReaderDevice(0);
            }
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(e);
            }
        }

        public bool MoreThanOneReaderFound
        {
            get { return readerDevice?.MoreThanOneReader ?? true; }
        }

        public bool ReaderPortBusy
        {
            get { return readerDevice?.PortAccessDenied ?? true; }
        }

        public GenericChipModel? GenericChip
        {
            get; private set;
        }

        public async Task<int> ReadChipPublic()
        {
            ChipModel tmpTag;

            return await Task.Run(async () => 
            {
                try
                {
                    if (GenericChip != null)
                    {
                        GenericChip = new GenericChipModel();
                    }

                    if (readerDevice != null)
                    {
                        if (!readerDevice.IsConnected)
                        {
                            await readerDevice.ConnectAsync();
                            if (readerDevice.PortAccessDenied == true)
                            {
                                return -1;
                            }
                        }
                        tmpTag = await readerDevice.GetSingleChipAsync(true);

                        hfTag = new ChipModel(tmpTag.UID, tmpTag.CardType, tmpTag.SAK, tmpTag.RATS, tmpTag.VersionL4);

                        tmpTag = await readerDevice.GetSingleChipAsync(true, true);
                        legicTag = new ChipModel(tmpTag.UID, tmpTag.CardType);

                        tmpTag = await readerDevice.GetSingleChipAsync(false);
                        lfTag = new ChipModel(tmpTag.UID, tmpTag.CardType);

                        await readerDevice.GetSingleChipAsync(true);

                        if (
                                !(
                                    string.IsNullOrWhiteSpace(hfTag?.UID) &&
                                    string.IsNullOrWhiteSpace(lfTag?.UID) &&
                                    string.IsNullOrWhiteSpace(legicTag?.UID)
                                )
                            )
                        {
                            try
                            {

                                await readerDevice.GreenLEDAsync(true);
                                await readerDevice.RedLEDAsync(false);


                                GenericChip = new GenericChipModel(hfTag?.UID ?? "",
                                    hfTag?.CardType ?? ChipType.NOTAG,
                                    hfTag?.SAK ?? "",
                                    hfTag?.RATS ?? "",
                                    hfTag?.VersionL4 ?? ""
                                    );

                                if (lfTag != null && lfTag?.CardType != ChipType.NOTAG)
                                {
                                    if(GenericChip != null && GenericChip.CardType != ChipType.NOTAG)
                                    {
                                        GenericChip.Child = new GenericChipModel(lfTag?.UID ?? "", lfTag?.CardType ?? ChipType.NOTAG);
                                    }
                                    else
                                    {
                                        GenericChip = new GenericChipModel(lfTag?.UID ?? "", lfTag?.CardType ?? ChipType.NOTAG);
                                    }
                                }
                                else if (legicTag != null && legicTag?.CardType != ChipType.NOTAG)
                                {
                                    if (GenericChip != null && GenericChip.CardType != ChipType.NOTAG)
                                    {
                                        GenericChip.Child = new GenericChipModel(legicTag?.UID ?? "", legicTag?.CardType ?? ChipType.NOTAG);
                                    }
                                    else
                                    {
                                        GenericChip = new GenericChipModel(legicTag?.UID ?? "", legicTag?.CardType ?? ChipType.NOTAG);
                                    }
                                }

                                return 0;
                            }
                            catch (Exception e)
                            {
                                LogWriter.CreateLogEntry(e);
                                return 1;
                            }
                        }
                        else
                        {
                            await readerDevice.BeepAsync(1, 25, 600, 100);
                            await readerDevice.RedLEDAsync(true);
                            await readerDevice.GreenLEDAsync(false);
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

                    LogWriter.CreateLogEntry(e);

                    return 2;
                }
            });
        }

        protected async Task Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
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
