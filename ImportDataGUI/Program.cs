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
using System.Windows.Forms;

namespace ImportDataGUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
