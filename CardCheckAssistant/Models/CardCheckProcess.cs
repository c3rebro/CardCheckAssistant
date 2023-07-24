using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardCheckAssistant.Models;

public class CardCheckProcess : Customer
{
    public string ID
    {
        get; set; 
    }

    public string ReportLanguage 
    { 
        get; set; 
    }

    public int CurrentProcessNumber 
    { 
        get; set; 
    }

    public int NumberOfChipsToCheck 
    { 
        get; set; 
    }

    public int CurrentStep
    {
        get; set;
    }

    public string IsVisible
    {
        get; set;
    }
}
