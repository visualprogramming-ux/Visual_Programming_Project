using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class LedgerDataAccess
    {
        // Get ledger entries for a specific customer (PartyId)
        // Returns transactions with Debit/Credit calculated based on Type
        public static List<LedgerEntryInfo> GetLedgerEntriesByPartyId(int partyId)
        {
            var entries = new List<LedgerEntryInfo>();

            // Check if PartyId column exists in Transactions
            bool partyIdExists = false;
            try
            {
                using var checkConnection = DatabaseHelper.GetConnection();
                using var checkCommand = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Transactions') AND name = 'PartyId'", checkConnection);
                checkConnection.Open();
                partyIdExists = ((int)checkCommand.ExecuteScalar()) > 0;
            }
            catch
            {
                partyIdExists = false;
            }

            string query;
            if (partyIdExists)
            {
                // Query with PartyId column
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        t.[Description],
                        DATEDIFF(DAY, t.[Date], GETDATE()) as DaysAged
                    FROM [dbo].[Transactions] t
                    WHERE t.[PartyId] = @PartyId
                    ORDER BY t.[Date] ASC, t.[TransactionId] ASC";
            }
            else
            {
                // Fallback: Get transactions through Sales
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        t.[Description],
                        DATEDIFF(DAY, t.[Date], GETDATE()) as DaysAged
                    FROM [dbo].[Transactions] t
                    INNER JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                    WHERE s.[BuyerId] = @PartyId OR s.[SellerId] = @PartyId
                    ORDER BY t.[Date] ASC, t.[TransactionId] ASC";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PartyId", partyId);
                connection.Open();

                using var reader = command.ExecuteReader();
                decimal runningBalance = 0;

                while (reader.Read())
                {
                    int transactionIdOrd = reader.GetOrdinal("TransactionId");
                    int dateOrd = reader.GetOrdinal("Date");
                    int typeOrd = reader.GetOrdinal("Type");
                    int amountOrd = reader.GetOrdinal("Amount");
                    int descriptionOrd = reader.GetOrdinal("Description");
                    int daysAgedOrd = reader.GetOrdinal("DaysAged");

                    DateTime transactionDate = reader.IsDBNull(dateOrd) ? DateTime.Now : reader.GetDateTime(dateOrd);
                    string transactionType = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd);
                    decimal amount = reader.IsDBNull(amountOrd) ? 0 : reader.GetDecimal(amountOrd);
                    string description = reader.IsDBNull(descriptionOrd) ? "" : reader.GetString(descriptionOrd);
                    int daysAged = reader.IsDBNull(daysAgedOrd) ? 0 : reader.GetInt32(daysAgedOrd);

                    // Extract Reference from Description (format: "Ref: XXX - Description")
                    string reference = "";
                    if (description.StartsWith("Ref:", StringComparison.OrdinalIgnoreCase))
                    {
                        int dashIndex = description.IndexOf(" - ");
                        if (dashIndex > 0)
                        {
                            reference = description.Substring(4, dashIndex - 4).Trim();
                        }
                    }

                    // Calculate Debit and Credit based on Type
                    decimal debit = 0;
                    decimal credit = 0;
                    
                    if (transactionType.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                    {
                        debit = amount;
                        runningBalance += amount; // Debit increases balance (customer owes more)
                    }
                    else if (transactionType.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        credit = amount;
                        runningBalance -= amount; // Credit decreases balance (customer pays)
                    }

                    // Calculate aging
                    string aging = "";
                    if (daysAged <= 30)
                        aging = $"{daysAged} days";
                    else if (daysAged <= 60)
                        aging = $"{daysAged} days";
                    else if (daysAged <= 90)
                        aging = $"{daysAged} days";
                    else
                        aging = $"{daysAged} days";

                    entries.Add(new LedgerEntryInfo
                    {
                        TransactionId = reader.GetInt32(transactionIdOrd),
                        Date = transactionDate,
                        Description = description,
                        Reference = reference,
                        Debit = debit,
                        Credit = credit,
                        Balance = runningBalance,
                        Aging = aging
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading ledger entries: {ex.Message}", ex);
            }

            return entries;
        }

        // Get ledger summary for a customer (Running Balance, Outstanding, Total Credit)
        public static LedgerSummaryInfo GetLedgerSummary(int partyId)
        {
            var entries = GetLedgerEntriesByPartyId(partyId);
            
            decimal runningBalance = entries.Count > 0 ? entries.Last().Balance : 0;
            decimal outstandingAmount = runningBalance < 0 ? Math.Abs(runningBalance) : 0;
            decimal totalCredit = entries.Sum(e => e.Credit);
            decimal totalDebit = entries.Sum(e => e.Debit);

            return new LedgerSummaryInfo
            {
                RunningBalance = runningBalance,
                OutstandingAmount = outstandingAmount,
                TotalCredit = totalCredit,
                TotalDebit = totalDebit
            };
        }

        // Get receivables aging report for all customers or a specific customer
        // Calculates net outstanding (Debit - Credit) per customer, aged by oldest unpaid debit
        public static List<AgingReportInfo> GetReceivablesAgingReport(DateTime asOfDate, int? partyId = null)
        {
            var report = new List<AgingReportInfo>();

            // Check if PartyId column exists
            bool partyIdExists = false;
            try
            {
                using var checkConnection = DatabaseHelper.GetConnection();
                using var checkCommand = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Transactions') AND name = 'PartyId'", checkConnection);
                checkConnection.Open();
                partyIdExists = ((int)checkCommand.ExecuteScalar()) > 0;
            }
            catch
            {
                partyIdExists = false;
            }

            string query;
            if (partyIdExists)
            {
                // Get all transactions for customers with outstanding balances
                query = @"
                    SELECT 
                        p.[PartyId],
                        p.[Name] as CustomerName,
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        DATEDIFF(DAY, t.[Date], @AsOfDate) as DaysAged
                    FROM [dbo].[Transactions] t
                    INNER JOIN [dbo].[Parties] p ON t.[PartyId] = p.[PartyId]
                    WHERE t.[Date] <= @AsOfDate
                      AND (@PartyId IS NULL OR t.[PartyId] = @PartyId)
                    ORDER BY p.[PartyId], t.[Date] ASC, t.[TransactionId] ASC";
            }
            else
            {
                query = @"
                    SELECT 
                        p.[PartyId],
                        p.[Name] as CustomerName,
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        DATEDIFF(DAY, t.[Date], @AsOfDate) as DaysAged
                    FROM [dbo].[Transactions] t
                    INNER JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                    INNER JOIN [dbo].[Parties] p ON (s.[BuyerId] = p.[PartyId] OR s.[SellerId] = p.[PartyId])
                    WHERE t.[Date] <= @AsOfDate
                      AND (@PartyId IS NULL OR (s.[BuyerId] = @PartyId OR s.[SellerId] = @PartyId))
                    ORDER BY p.[PartyId], t.[Date] ASC, t.[TransactionId] ASC";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@AsOfDate", asOfDate.Date);
                command.Parameters.AddWithValue("@PartyId", partyId ?? (object)DBNull.Value);
                connection.Open();

                using var reader = command.ExecuteReader();
                var customerData = new Dictionary<int, CustomerAgingData>();

                while (reader.Read())
                {
                    int partyIdOrd = reader.GetOrdinal("PartyId");
                    int customerNameOrd = reader.GetOrdinal("CustomerName");
                    int dateOrd = reader.GetOrdinal("Date");
                    int typeOrd = reader.GetOrdinal("Type");
                    int amountOrd = reader.GetOrdinal("Amount");
                    int daysAgedOrd = reader.GetOrdinal("DaysAged");

                    int customerPartyId = reader.GetInt32(partyIdOrd);
                    string customerName = reader.IsDBNull(customerNameOrd) ? "" : reader.GetString(customerNameOrd);
                    DateTime transactionDate = reader.IsDBNull(dateOrd) ? DateTime.Now : reader.GetDateTime(dateOrd);
                    string transactionType = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd);
                    decimal amount = reader.IsDBNull(amountOrd) ? 0 : reader.GetDecimal(amountOrd);
                    int daysAged = reader.IsDBNull(daysAgedOrd) ? 0 : reader.GetInt32(daysAgedOrd);

                    if (!customerData.ContainsKey(customerPartyId))
                    {
                        customerData[customerPartyId] = new CustomerAgingData
                        {
                            PartyId = customerPartyId,
                            CustomerName = customerName,
                            OldestInvoiceDate = asOfDate,
                            Transactions = new List<(DateTime Date, string Type, decimal Amount, int DaysAged)>()
                        };
                    }

                    var data = customerData[customerPartyId];
                    data.Transactions.Add((transactionDate, transactionType, amount, daysAged));
                }
                reader.Close();

                // Process transactions for each customer to calculate aging buckets
                foreach (var kvp in customerData)
                {
                    var data = kvp.Value;
                    decimal runningBalance = 0;
                    DateTime? oldestDebitDate = null;

                    // Sort transactions by date
                    data.Transactions.Sort((a, b) => a.Date.CompareTo(b.Date));

                    foreach (var trans in data.Transactions)
                    {
                        // Calculate running balance
                        if (trans.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance += trans.Amount;
                            if (oldestDebitDate == null || trans.Date < oldestDebitDate.Value)
                                oldestDebitDate = trans.Date;
                        }
                        else if (trans.Type.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                        {
                            runningBalance -= trans.Amount;
                        }

                        // Age outstanding Debit transactions
                        if (trans.Type.Equals("Debit", StringComparison.OrdinalIgnoreCase) && runningBalance > 0)
                        {
                            decimal outstandingAmount = Math.Min(trans.Amount, runningBalance);
                            
                            if (trans.DaysAged <= 30)
                                data.Current += outstandingAmount;
                            else if (trans.DaysAged <= 60)
                                data.Days31to60 += outstandingAmount;
                            else if (trans.DaysAged <= 90)
                                data.Days61to90 += outstandingAmount;
                            else
                                data.Over90 += outstandingAmount;
                        }
                    }

                    data.TotalOutstanding = runningBalance;
                    data.OldestInvoiceDate = oldestDebitDate ?? asOfDate;

                    // Only add customers with outstanding balances
                    if (data.TotalOutstanding > 0)
                    {
                        report.Add(new AgingReportInfo
                        {
                            PartyId = data.PartyId,
                            CustomerName = data.CustomerName,
                            TotalOutstanding = data.TotalOutstanding,
                            Current = data.Current,
                            Days31to60 = data.Days31to60,
                            Days61to90 = data.Days61to90,
                            Over90 = data.Over90,
                            OldestInvoiceDate = data.OldestInvoiceDate
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading receivables aging report: {ex.Message}", ex);
            }

            return report;
        }

        // Get customer statement transactions
        public static List<StatementTransactionInfo> GetStatementTransactions(int partyId, DateTime fromDate, DateTime toDate)
        {
            var transactions = new List<StatementTransactionInfo>();

            // Check if PartyId column exists
            bool partyIdExists = false;
            try
            {
                using var checkConnection = DatabaseHelper.GetConnection();
                using var checkCommand = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Transactions') AND name = 'PartyId'", checkConnection);
                checkConnection.Open();
                partyIdExists = ((int)checkCommand.ExecuteScalar()) > 0;
            }
            catch
            {
                partyIdExists = false;
            }

            string query;
            if (partyIdExists)
            {
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        t.[Description]
                    FROM [dbo].[Transactions] t
                    WHERE t.[PartyId] = @PartyId
                      AND t.[Date] >= @FromDate
                      AND t.[Date] <= @ToDate
                    ORDER BY t.[Date] ASC, t.[TransactionId] ASC";
            }
            else
            {
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Type],
                        t.[Amount],
                        t.[Description]
                    FROM [dbo].[Transactions] t
                    INNER JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                    WHERE (s.[BuyerId] = @PartyId OR s.[SellerId] = @PartyId)
                      AND t.[Date] >= @FromDate
                      AND t.[Date] <= @ToDate
                    ORDER BY t.[Date] ASC, t.[TransactionId] ASC";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PartyId", partyId);
                command.Parameters.AddWithValue("@FromDate", fromDate.Date);
                command.Parameters.AddWithValue("@ToDate", toDate.Date);
                connection.Open();

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int transactionIdOrd = reader.GetOrdinal("TransactionId");
                    int dateOrd = reader.GetOrdinal("Date");
                    int typeOrd = reader.GetOrdinal("Type");
                    int amountOrd = reader.GetOrdinal("Amount");
                    int descriptionOrd = reader.GetOrdinal("Description");

                    DateTime transactionDate = reader.IsDBNull(dateOrd) ? DateTime.Now : reader.GetDateTime(dateOrd);
                    string transactionType = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd);
                    decimal amount = reader.IsDBNull(amountOrd) ? 0 : reader.GetDecimal(amountOrd);
                    string description = reader.IsDBNull(descriptionOrd) ? "" : reader.GetString(descriptionOrd);

                    // Extract Reference from Description
                    string reference = "";
                    if (description.StartsWith("Ref:", StringComparison.OrdinalIgnoreCase))
                    {
                        int dashIndex = description.IndexOf(" - ");
                        if (dashIndex > 0)
                        {
                            reference = description.Substring(4, dashIndex - 4).Trim();
                        }
                    }

                    decimal debit = 0;
                    decimal credit = 0;

                    if (transactionType.Equals("Debit", StringComparison.OrdinalIgnoreCase))
                    {
                        debit = amount;
                    }
                    else if (transactionType.Equals("Credit", StringComparison.OrdinalIgnoreCase))
                    {
                        credit = amount;
                    }

                    transactions.Add(new StatementTransactionInfo
                    {
                        TransactionId = reader.GetInt32(transactionIdOrd),
                        TransactionDate = transactionDate,
                        Description = description,
                        Reference = reference,
                        Debit = debit,
                        Credit = credit,
                        Balance = 0 // Will be calculated in the page code
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading statement transactions: {ex.Message}", ex);
            }

            return transactions;
        }

        // Get opening balance for a customer (balance before fromDate)
        public static decimal GetOpeningBalance(int partyId, DateTime fromDate)
        {
            // Check if PartyId column exists
            bool partyIdExists = false;
            try
            {
                using var checkConnection = DatabaseHelper.GetConnection();
                using var checkCommand = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Transactions') AND name = 'PartyId'", checkConnection);
                checkConnection.Open();
                partyIdExists = ((int)checkCommand.ExecuteScalar()) > 0;
            }
            catch
            {
                partyIdExists = false;
            }

            string query;
            if (partyIdExists)
            {
                query = @"
                    SELECT 
                        SUM(CASE WHEN [Type] = 'Debit' THEN [Amount] ELSE 0 END) - 
                        SUM(CASE WHEN [Type] = 'Credit' THEN [Amount] ELSE 0 END) as OpeningBalance
                    FROM [dbo].[Transactions]
                    WHERE [PartyId] = @PartyId
                      AND [Date] < @FromDate";
            }
            else
            {
                query = @"
                    SELECT 
                        SUM(CASE WHEN t.[Type] = 'Debit' THEN t.[Amount] ELSE 0 END) - 
                        SUM(CASE WHEN t.[Type] = 'Credit' THEN t.[Amount] ELSE 0 END) as OpeningBalance
                    FROM [dbo].[Transactions] t
                    INNER JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                    WHERE (s.[BuyerId] = @PartyId OR s.[SellerId] = @PartyId)
                      AND t.[Date] < @FromDate";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PartyId", partyId);
                command.Parameters.AddWithValue("@FromDate", fromDate.Date);
                connection.Open();

                var result = command.ExecuteScalar();
                return result != null && result != DBNull.Value ? Convert.ToDecimal(result) : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating opening balance: {ex.Message}", ex);
            }
        }

        public class LedgerEntryInfo
        {
            public int TransactionId { get; set; }
            public DateTime Date { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Balance { get; set; }
            public string Aging { get; set; } = string.Empty;
        }

        public class LedgerSummaryInfo
        {
            public decimal RunningBalance { get; set; }
            public decimal OutstandingAmount { get; set; }
            public decimal TotalCredit { get; set; }
            public decimal TotalDebit { get; set; }
        }

        public class AgingReportInfo
        {
            public int PartyId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal TotalOutstanding { get; set; }
            public decimal Current { get; set; }
            public decimal Days31to60 { get; set; }
            public decimal Days61to90 { get; set; }
            public decimal Over90 { get; set; }
            public DateTime OldestInvoiceDate { get; set; }
        }

        public class StatementTransactionInfo
        {
            public int TransactionId { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Description { get; set; } = string.Empty;
            public string Reference { get; set; } = string.Empty;
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal Balance { get; set; }
        }

        private class CustomerAgingData
        {
            public int PartyId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal TotalOutstanding { get; set; }
            public decimal Current { get; set; }
            public decimal Days31to60 { get; set; }
            public decimal Days61to90 { get; set; }
            public decimal Over90 { get; set; }
            public DateTime OldestInvoiceDate { get; set; }
            public List<(DateTime Date, string Type, decimal Amount, int DaysAged)> Transactions { get; set; } = new();
        }
    }
}

