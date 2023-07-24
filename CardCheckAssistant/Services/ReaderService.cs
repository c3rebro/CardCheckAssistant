using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO.Ports;

using CardCheckAssistant.Model;

using Elatec.NET;
using Elatec.NET.Model;

using Log4CSharp;

namespace CardCheckAssistant.Services
{
    internal class ReaderService : IDisposable
    {
        private readonly TWN4ReaderDevice readerDevice;

        private ChipModel hfTag;
        private ChipModel lfTag;
        private ChipModel legicTag;

        public ReaderService()
        {
            try
            {
                readerDevice = new TWN4ReaderDevice(0);
            }
            catch (Exception e)
            {
                LogWriter.CreateLogEntry(e, App.Current.GetType().Name);
            }
        }

        public bool MoreThanOneReaderFound
        {
            get { return readerDevice.MoreThanOneReader; }
        }

        public GenericChipModel GenericChip
        {
            get; private set;
        }

        public Task ReadChipPublic()
        {
            return Task.Run(() =>
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
                            readerDevice.Connect();
                        }
                        var tmpTag = readerDevice.GetSingleChip(true);
                        hfTag = new ChipModel(tmpTag.UID, tmpTag.CardType, tmpTag.SAK, tmpTag.RATS, tmpTag.VersionL4);

                        tmpTag = readerDevice.GetSingleChip(true, true);
                        legicTag = new ChipModel(tmpTag.UID, tmpTag.CardType);

                        tmpTag = readerDevice.GetSingleChip(false);
                        lfTag = new ChipModel(tmpTag.UID, tmpTag.CardType);

                        readerDevice.GetSingleChip(true);

                        if (
                                !(
                                    string.IsNullOrWhiteSpace(hfTag?.UID) &
                                    string.IsNullOrWhiteSpace(lfTag?.UID) &
                                    string.IsNullOrWhiteSpace(legicTag?.UID)
                                )
                            )
                        {
                            try
                            {

                                readerDevice.GreenLED(true);
                                readerDevice.RedLED(false);


                                GenericChip = new GenericChipModel(hfTag.UID,
                                    hfTag.CardType,
                                    hfTag.SAK,
                                    hfTag.RATS,
                                    hfTag.VersionL4
                                    );

                                if (lfTag != null && lfTag?.CardType != ChipType.NOTAG)
                                {
                                    GenericChip.Child = new GenericChipModel(lfTag.UID, lfTag.CardType);
                                }
                                else if (legicTag != null && legicTag?.CardType != ChipType.NOTAG)
                                {
                                    GenericChip.Child = new GenericChipModel(legicTag.UID, legicTag.CardType);
                                }
                                //readerDevice.GetSingleChip(true);

                                return 0;
                            }
                            catch (Exception e)
                            {
                                LogWriter.CreateLogEntry(e, App.Current.GetType().Name);
                                return 1;
                            }
                        }
                        else
                        {
                            readerDevice.Beep(1, 25, 600, 100);
                            readerDevice.RedLED(true);
                            readerDevice.GreenLED(false);
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

                    LogWriter.CreateLogEntry(e, App.Current.GetType().Name);

                    return 2;
                }
            });

        }

        protected void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    readerDevice.GreenLED(false);
                    readerDevice.RedLED(false);
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
