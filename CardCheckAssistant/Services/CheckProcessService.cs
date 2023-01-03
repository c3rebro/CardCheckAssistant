using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CardCheckAssistant.Models;

namespace CardCheckAssistant.Services;

static class CheckProcessService
{
    public static Customer CurrentCustomer
    {
        get; set; }

    public static CardCheckProcess CurrentCardCheckProcess { get; set; }
}