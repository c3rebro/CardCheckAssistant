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

        }

        public GenericChipModel(string uid, ChipType cardType)
        {
            UID = uid;
            CardType = cardType;
        }

        public GenericChipModel(string uid, ChipType cardType, string sak, string rats)
        {
            UID = uid;
            CardType = cardType;
            SAK = sak;
            RATS = rats;
        }

        public GenericChipModel(string uid, ChipType cardType, string sak, string rats, string versionL4)
        {
            UID = uid;
            CardType = cardType;
            SAK = sak;
            RATS = rats;
            VersionL4 = versionL4;
        }

        public string UID { get; set; }
        public ChipType CardType { get; set; }
        public string SAK { get; set; }
        public string RATS { get; set; }
        public string VersionL4 { get; set; }
        public GenericChipModel Child { get; set; }
        public GenericChipModel GrandChild { get; set; }
    }
}