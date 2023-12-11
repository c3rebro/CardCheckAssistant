// This Source Code Form is subject to the terms of the MIT License.
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
public class CardCheckTextTemplate
{
    /// <summary>
    /// 
    /// </summary>
    public CardCheckTextTemplate()
    {
        TemplateTextName = "N/A";
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="templateTextName"></param>
    /// <param name="templateToolTip"></param>
    public CardCheckTextTemplate(string templateTextName)
    {
        TemplateTextName = templateTextName;
    }

    /// <summary>
    /// 
    /// </summary>
    public string TemplateTextName
    { 
        get; set; 
    }

    /// <summary>
    /// 
    /// </summary>
    public string TemplateToolTip
    {
        get { return TemplateTextContent; }
    }

    /// <summary>
    /// 
    /// </summary>
    public string TemplateTextContent
    {
        get; set;
    }
}


