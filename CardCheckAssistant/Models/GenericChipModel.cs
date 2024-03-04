using Elatec.NET;
using System.Collections.Generic;

namespace CardCheckAssistant.Models
{
    /// <summary>
    /// Description of chipUid.
    /// </summary>
    public class GenericChipModel
    {
        public GenericChipModel()
        {
            Childs = new List<GenericChipModel>();
        }

        public GenericChipModel(string uid, ChipType cardType)
        {
            Childs = new List<GenericChipModel>();
            UID = uid;
            TCard = new CType() { PrimaryType = cardType, SecondaryType = MifareChipSubType.Unspecified };
        }

        public GenericChipModel(string uid, ChipType cardType, string sak, string rats)
        {
            Childs = new List<GenericChipModel>();
            UID = uid;
            TCard = new CType() { PrimaryType = cardType, SecondaryType = MifareChipSubType.Unspecified };
            SAK = sak;
            RATS = rats;
        }

        public GenericChipModel(string uid, MifareChipSubType cardType, string sak, string rats)
        {
            Childs = new List<GenericChipModel>();
            UID = uid;
            TCard = new CType() { PrimaryType = ChipType.MIFARE, SecondaryType = cardType };
            SAK = sak;
            RATS = rats;
        }

        public GenericChipModel(string uid, ChipType cardType, string sak, string rats, string versionL4)
        {
            Childs = new List<GenericChipModel>();
            UID = uid;
            TCard = new CType() { PrimaryType = cardType, SecondaryType = MifareChipSubType.Unspecified };
            SAK = sak;
            RATS = rats;
            VersionL4 = versionL4;
        }

        public GenericChipModel(string uid, MifareChipSubType cardType, string sak, string rats, string versionL4)
        {
            Childs = new List<GenericChipModel>();
            UID = uid;
            TCard = new CType() { PrimaryType = ChipType.MIFARE, SecondaryType = cardType };
            SAK = sak;
            RATS = rats;
            VersionL4 = versionL4;
        }

        public GenericChipModel(GenericChipModel chip)
        {
            Childs = new List<GenericChipModel>();
            UID = chip.UID;
            TCard = new CType() { PrimaryType = chip.TCard.PrimaryType, SecondaryType = chip.TCard.SecondaryType };
            SAK = chip.SAK;
            RATS = chip.RATS;
            VersionL4 = chip.VersionL4;
        }

        public bool? HasChilds => Childs?.Any() ?? false;

        public List<GenericChipModel> Childs
        {
            get; set;
        }

        public string UID { get; set; }
        public CType TCard
        {
            get; private set;
        }
        public struct CType
        {
            public Elatec.NET.ChipType PrimaryType { get; set; }
            public Elatec.NET.MifareChipSubType SecondaryType { get; set; }
        }
        public string SAK { get; set; }
        public string RATS { get; set; }
        public string VersionL4 { get; set; }
    }
}