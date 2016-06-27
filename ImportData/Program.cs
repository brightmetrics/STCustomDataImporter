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

namespace ImportData
{
    class Program
    {
        static void ShowUsage()
        {
            Console.WriteLine(@"
Usage: ImportData [options] inputData

-startDate <date|datetime>   The start datetime from which to process calls.

-endDate <date|datetime>     The end datetime (exclusive) to which to process
                             calls (optional).

-updateExisting              Process calls that already have account code
                             information populated.

-stserver <host>             The address of the ShoreTel HQ server.  Will
                             assume localhost if not specified.

-verbose                     Report each call as it is updated.

-h                           This help text.

-lookupProvider <TypeName>   Provide the assembly-qualified name of a .Net Type
                             to use an alternate lookup provider.  The
                             default is the built-in CSV file lookup provider.
                             Any additional command line arguments will be
                             passed to the lookup provider constructor.  For
                             the CSV lookup provider, only a single argument
                             naming the CSV file can be provided.  The file
                             should contain three columns: CallerID,
                             Customer ID, and Customer Name.
");
        }

        static void InitializationError(Exception ex)
        {
            Console.WriteLine("Unable to initialize data lookup provider:");
            Console.WriteLine(ex.Message);
            ShowUsage();
        }

        static void Main(string[] args)
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;
                var updateExisting = false;
                var verbose = false;
                var providerArgs = new List<object>();
                var lookupType = typeof(ShoreTelCustomDataImporter.CSVFileCustomerLookup);
                var stServerAddress = "localhost";

                for (var i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-lookupProvider")
                        lookupType = Type.GetType(args[++i], true);

                    else if (args[i] == "-startDate")
                        startDate = DateTime.Parse(args[++i]);

                    else if (args[i] == "-endDate")
                        endDate = DateTime.Parse(args[++i]);

                    else if (args[i] == "-updateExisting")
                        updateExisting = true;

                    else if (args[i] == "-verbose")
                        verbose = true;

                    else if (args[i] == "-stserver")
                        stServerAddress = args[++i];

                    else if (args[i] == "-h")
                    {
                        ShowUsage();
                        return;
                    }
                    else
                        providerArgs.Add(args[i]);
                }

                ShoreTelCustomDataImporter.ICustomerLookup lookup;
                try
                {
                    lookup = (ShoreTelCustomDataImporter.ICustomerLookup)Activator.CreateInstance(lookupType, providerArgs.ToArray());
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    InitializationError(ex.InnerException);
                    return;
                }
                catch (Exception ex)
                {
                    InitializationError(ex);
                    return;
                }

                int found = 0;
                int updated = 0;

                using (var cdr = new ShoreTelCustomDataImporter.ShoreTelCDRConnection(stServerAddress))
                {
                    foreach (var call in cdr.GetCalls(startDate, endDate, updateExisting))
                    {
                        found++;
                        var info = lookup.LookupCallerId(call.CallerId);
                        if (info != null)
                        {
                            cdr.UpdateCustomerInfo(call.CallId, info);
                            updated++;
                            if (verbose)
                            {
                                Console.WriteLine("Call {0} with CallerID {1} was set to customer id / name {2} / {3}",
                                    call.CallId, call.CallerId, info.CustomerId, info.CustomerName);
                            }
                        }
                    }
                }

                Console.WriteLine("{0} calls were updated out of {1} found", updated, found);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
