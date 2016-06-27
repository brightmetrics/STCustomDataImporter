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
using MySql.Data.MySqlClient;

namespace ShoreTelCustomDataImporter
{
    public sealed class ShoreTelCDRConnection : ICDRConnection, IDisposable
    {
        class CallRecord : ICallRecord
        {
            public object CallId { get; set; }
            public string CallerId { get; set; }
        }

        private readonly string serverAddress;
        private MySqlConnection updateConnection;
        private MySqlCommand updateCommand;

        public ShoreTelCDRConnection(string serverAddress)
        {
            this.serverAddress = serverAddress;
            InitializeConnection();
            InitializeUpdateCommand();
        }

        public void Dispose()
        {
            if (updateCommand != null)
            {
                updateCommand.Dispose();
                updateCommand = null;
            }

            if (updateConnection != null)
            {
                updateConnection.Dispose();
                updateConnection = null;
            }
        }

        private MySqlConnection GetConnection()
        {
            var builder = new MySqlConnectionStringBuilder();
            builder.Server = serverAddress;
            builder.Port = 4309;
            builder.Database = "shorewarecdr";
            builder.UserID = "st_cdrupdate";
            builder.Password = "passwordcdrupdate";
            return new MySqlConnection(builder.GetConnectionString(true));
        }

        private void InitializeConnection()
        {
            updateConnection = GetConnection();
            updateConnection.Open();
        }

        private void InitializeUpdateCommand()
        {
            updateCommand = updateConnection.CreateCommand();
            updateCommand.CommandText = "UPDATE `call` SET BillingCode = @p1, FriendlyBillingCode = @p2 WHERE Id = @p3";
            updateCommand.Parameters.Add(new MySqlParameter("@p1", MySqlDbType.VarChar, 32));
            updateCommand.Parameters.Add(new MySqlParameter("@p2", MySqlDbType.VarChar, 50));
            updateCommand.Parameters.Add(new MySqlParameter("@p3", MySqlDbType.UInt32));
        }

        public IEnumerable<ICallRecord> GetCalls(DateTime? startTime, DateTime? endTime, bool updateExisting)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = GetCommandText("Id, CallerId", startTime, endTime, updateExisting);
                    cmd.CommandTimeout = 0;

                    using (var rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            yield return new CallRecord
                            {
                                CallId = rdr.GetValue(0),
                                CallerId = rdr.GetString(1)
                            };
                        }
                    }
                }
            }
        }

        private string GetCommandText(string fieldSpec, DateTime? startTime, DateTime? endTime, bool updateExisting)
        {
            var commandText = "SELECT " + fieldSpec + " FROM `call` WHERE calltype IN(2, 4)";
            if (startTime != null)
                commandText += " AND startTime >= '" + startTime.Value.ToString("s") + "'";
            if (endTime != null)
                commandText += " AND startTime < '" + endTime.Value.ToString("s") + "'";
            if (!updateExisting)
                commandText += " AND BillingCode = '' AND FriendlyBillingCode = ''";
            return commandText;
        }

        public int GetPotentialCallCount(DateTime? startTime, DateTime? endTime, bool updateExisting)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = GetCommandText("COUNT(*)", startTime, endTime, updateExisting);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public void UpdateCustomerInfo(object callId, CustomerInfo customerInfo)
        {
            updateCommand.Parameters[0].Value = customerInfo.CustomerId ?? string.Empty;
            updateCommand.Parameters[1].Value = customerInfo.CustomerName ?? string.Empty;
            updateCommand.Parameters[2].Value = callId;
            updateCommand.ExecuteNonQuery();
        }
    }
}
