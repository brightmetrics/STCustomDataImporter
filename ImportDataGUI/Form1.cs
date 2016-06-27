/* Copyright 2016 Brightmetrics, Inc.
 * This file is part of ShoreTelCustomDataImporter.  It is subject to the license terms
 * in the LICENSE file found in the top-level directory of this distribution and at
 * https://github.com/brightmetrics/STCustomDataImporter/blob/master/LICENSE.  No part
 * of ShoreTelCustomDataImporter, including this file, may be copied, modified,
 * propagated, or distributed except according to the terms contained in the LICENSE file.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ImportDataGUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var lastFile = (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Brightmetrics\\ShoreTelDataImporter", "LastImportFile", null);
            if (!string.IsNullOrEmpty(lastFile))
            {
                textBox1.Text = lastFile;
                openFileDialog1.FileName = lastFile;
            }
            var lastServerAddress = (string)Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Software\\Brightmetrics\\ShoreTelDataImporter", "LastServerAddress", null);
            if (!string.IsNullOrEmpty(lastServerAddress))
            {
                textBox2.Text = lastServerAddress;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(openFileDialog1.FileName))
                MessageBox.Show("A file must be selected first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                button1.Enabled = false;
                var filename = textBox1.Text;
                var serverAddress = textBox2.Text;
                Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\Software\\Brightmetrics\\ShoreTelDataImporter", "LastImportFile", filename);
                Microsoft.Win32.Registry.SetValue("HKEY_CURRENT_USER\\Software\\Brightmetrics\\ShoreTelDataImporter", "LastServerAddress", serverAddress);
                var importer = new BackgroundWorker();
                importer.WorkerReportsProgress = true;
                importer.DoWork += Importer_DoWork;
                importer.ProgressChanged += Importer_ProgressChanged;
                importer.RunWorkerCompleted += Importer_RunWorkerCompleted;
                importer.RunWorkerAsync(new Options
                {
                    ImportFile = filename,
                    ServerAddress = serverAddress,
                    StartTime = dateTimePicker1.Value,
                    EndTime = dateTimePicker2.Value.AddDays(1.0),
                    UpdateExisting = checkBox1.Checked
                });
            }
        }

        private void Importer_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;            
        }

        private void Importer_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Value = 0;
            button1.Enabled = true;
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                var result = (Tuple<int, int>)e.Result;
                MessageBox.Show("Updated " + result.Item2 + " of " + result.Item1 + " calls");
            }
        }

        private void Importer_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var opts = (Options)e.Argument;
            var lookup = new ShoreTelCustomDataImporter.CSVFileCustomerLookup(opts.ImportFile);
            var found = 0;
            var updated = 0;
            var lastProgressUpdate = 0;
            using (var cdr = new ShoreTelCustomDataImporter.ShoreTelCDRConnection(opts.ServerAddress))
            {
                var total = cdr.GetPotentialCallCount(opts.StartTime, opts.EndTime, opts.UpdateExisting);
                foreach (var call in cdr.GetCalls(opts.StartTime, opts.EndTime, opts.UpdateExisting))
                {
                    found++;
                    var info = lookup.LookupCallerId(call.CallerId);
                    if (info != null)
                    {
                        cdr.UpdateCustomerInfo(call.CallId, info);
                        updated++;
                    }
                    var progress = found * 100 / total;
                    if (progress > lastProgressUpdate)
                    {
                        lastProgressUpdate = progress;
                        worker.ReportProgress(progress, updated);
                    }
                }                    
            }
            e.Result = Tuple.Create(found, updated);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            textBox1.Text = openFileDialog1.FileName;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}
