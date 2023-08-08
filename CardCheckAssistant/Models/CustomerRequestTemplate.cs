// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using CardCheckAssistant.Services;
using System.ComponentModel;

namespace CardCheckAssistant.Models;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum CustomerRequestTemplate
{
    [Description("CustomerTextTemplate")]
    NA,
    Ask_4PICC_MK,
    Ask_4ClassicKeys,
    Ask_NoMemory,
    Ask_Reserved1,
    Ask_Reserved2,
    Ask_Reserved3,
    Ask_Reserved4,
    Ask_Reserved5,
    Ask_Reserved6,
    Ask_Reserved7,
    Ask_Reserved8
}
