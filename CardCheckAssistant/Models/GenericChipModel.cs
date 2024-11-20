using Elatec.NET;
using System.Collections.Generic;

namespace CardCheckAssistant.Models
{
    /// <summary>
    /// Description of chipUid.
    /// </summary>
    public class GenericChipModel
    {
        public string UID
        {
            get; set;
        }
        public CCChipType ChipType
        {
            get; set;
        }

        public string SAK
        {
            get; set;
        }
        public string RATS
        {
            get; set;
        }
        public string VersionL4
        {
            get; set;
        }
    }
}