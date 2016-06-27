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
using CsvHelper;
using System.IO;

namespace ShoreTelCustomDataImporter
{
    public sealed class CSVFileCustomerLookup : ICustomerLookup
    {
        class CustomerInfoEntry
        {
            public string CallerId { get; set; }
            public string CustomerId { get; set; }
            public string CustomerName { get; set; }
        }

        ILookup<string, CustomerInfo> lookup;

        public CSVFileCustomerLookup(string fileName)
        {
            using (var csv = new CsvReader(new StreamReader(fileName)))
            {
                lookup = csv.GetRecords<CustomerInfoEntry>()
                    .ToLookup(x => x.CallerId,
                              x => new CustomerInfo(x.CustomerId, x.CustomerName));
            }

        }
        public CustomerInfo LookupCallerId(string callerId)
        {
            var entries = lookup[callerId];
            return entries == null ? null : entries.FirstOrDefault();
        }
    }
}
