﻿// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using CardCheckAssistant.Services;
using System.ComponentModel;
using System.Security.Policy;

namespace CardCheckAssistant.Models;

/// <summary>
/// 
/// </summary>
public class LSMCardTemplate
{
    /// <summary>
    /// 
    /// </summary>
    public LSMCardTemplate()
    {
        TemplateText = "";
        TemplateToolTip = "";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="templateText"></param>
    /// <param name="templateToolTip"></param>
    public LSMCardTemplate(string templateText, string templateToolTip)
    {
        TemplateText = templateText;
        TemplateToolTip = templateToolTip;
    }

    /// <summary>
    /// 
    /// </summary>
    public string TemplateText
    { 
        get; set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public string TemplateToolTip
    {
        get; set;
    }
}

