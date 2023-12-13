// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.
using CardCheckAssistant.Services;
using System.ComponentModel;

namespace CardCheckAssistant.Models;

//[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum OrderStatus
{
    [Description("NotDefined")]
    NA,
    Created,
    OrderConfirmed,
    ExportReady,
    InProgress,
    RequestCustomerFeedback,
    CheckFinished,
    SendMail,
    ReadyForShipping,
    Closed,
    Cancelled
}
