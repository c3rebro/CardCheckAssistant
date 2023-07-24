// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

namespace CardCheckAssistant.Models;

public class Customer
{
    public string JobNr { get; set; }
    public string ChipNumber { get; set; }
    public string OmniLink { get; set; }
    public string Date { get; set; }
    public string CName { get; set; }
    public string EditorName { get; set; }
    public bool IsMember { get; set; }
    public OrderStatus Status { get; set; }
}
