/* Copyright 2016 Brightmetrics, Inc.
 * This file is part of ShoreTelCustomDataImporter.  It is subject to the license terms
 * in the LICENSE file found in the top-level directory of this distribution and at
 * https://github.com/brightmetrics/STCustomDataImporter/blob/master/LICENSE.  No part
 * of ShoreTelCustomDataImporter, including this file, may be copied, modified,
 * propagated, or distributed except according to the terms contained in the LICENSE file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShoreTelCustomDataImporter
{
    /// <summary>
    /// A DTO representing the customer information that will be inserted
    /// into the ShoreTel CDR data.  CustomerId will be written to the
    /// BillingCode field and CustomerName will be written to the
    /// FriendlyBillingCode field.  These are also known as "Account Code"
    /// and "Account Code Name", respectively.  BillingCode (and therefore
    /// CustomerId) is limited to 32 characters.  FriendlyBillingCode (and
    /// therefore CustomerName) is limited to 50 characters.  The intended
    /// usage is that CustomerId represents a short identifier and
    /// CustomerName represents a longer description, but one could just
    /// as well use CustomerId to represent the organization and
    /// CustomerName to represent an individual or sub-department of the
    /// organization, according to how the data will be utilized for
    /// reporting.
    /// </summary>
    public sealed class CustomerInfo
    {
        public readonly string CustomerId;
        public readonly string CustomerName;

        public CustomerInfo(string CustomerId, string CustomerName)
        {
            this.CustomerId = CustomerId;
            this.CustomerName = CustomerName;
        }
    }
}
