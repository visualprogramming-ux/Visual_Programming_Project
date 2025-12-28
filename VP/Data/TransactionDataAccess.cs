using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class TransactionDataAccess
    {
        // Insert a new transaction (standalone, not linked to Sale or Installment)
        public static int InsertTransaction(int partyId, string transactionType, decimal amount, 
            DateTime transactionDate, string? description = null, int? saleId = null, int? installmentId = null)
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

            string columns = "[Date], [Amount], [Type], [SaleId], [InstallmentId], [Description], [CreatedAt], [UpdatedAt]";
            string values = "@Date, @Amount, @Type, @SaleId, @InstallmentId, @Description, GETDATE(), GETDATE()";
            
            if (partyIdExists)
            {
                columns = "[Date], [Amount], [Type], [PartyId], [SaleId], [InstallmentId], [Description], [CreatedAt], [UpdatedAt]";
                values = "@Date, @Amount, @Type, @PartyId, @SaleId, @InstallmentId, @Description, GETDATE(), GETDATE()";
            }

            string query = $@"
                INSERT INTO [dbo].[Transactions] 
                    ({columns})
                OUTPUT INSERTED.TransactionId
                VALUES 
                    ({values})";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                
                command.Parameters.AddWithValue("@Date", transactionDate.Date);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@Type", transactionType ?? "Debit");
                if (partyIdExists)
                {
                    command.Parameters.AddWithValue("@PartyId", partyId);
                }
                command.Parameters.AddWithValue("@SaleId", saleId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@InstallmentId", installmentId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Description", string.IsNullOrWhiteSpace(description) ? (object)DBNull.Value : description);

                connection.Open();
                return (int)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting transaction: {ex.Message}", ex);
            }
        }

        // Get all transactions with customer information
        public static List<TransactionInfo> GetAllTransactions()
        {
            var transactions = new List<TransactionInfo>();

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
                // Query with PartyId column
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Amount],
                        t.[Type],
                        t.[Description],
                        ISNULL(p.[Name], 'Unknown Customer') as CustomerName
                    FROM [dbo].[Transactions] t
                    LEFT JOIN [dbo].[Parties] p ON t.[PartyId] = p.[PartyId]
                    ORDER BY t.[Date] DESC, t.[TransactionId] DESC";
            }
            else
            {
                // Query without PartyId column (fallback)
                query = @"
                    SELECT 
                        t.[TransactionId],
                        t.[Date],
                        t.[Amount],
                        t.[Type],
                        t.[Description],
                        ISNULL(p.[Name], 'Standalone Transaction') as CustomerName
                    FROM [dbo].[Transactions] t
                    LEFT JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                    LEFT JOIN [dbo].[Parties] p ON (s.[BuyerId] = p.[PartyId] OR s.[SellerId] = p.[PartyId])
                    ORDER BY t.[Date] DESC, t.[TransactionId] DESC";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int transactionIdOrd = reader.GetOrdinal("TransactionId");
                    int dateOrd = reader.GetOrdinal("Date");
                    int amountOrd = reader.GetOrdinal("Amount");
                    int typeOrd = reader.GetOrdinal("Type");
                    int descriptionOrd = reader.GetOrdinal("Description");
                    int customerNameOrd = reader.GetOrdinal("CustomerName");

                    transactions.Add(new TransactionInfo
                    {
                        TransactionId = reader.GetInt32(transactionIdOrd),
                        TransactionDate = reader.IsDBNull(dateOrd) ? DateTime.Now : reader.GetDateTime(dateOrd),
                        Amount = reader.IsDBNull(amountOrd) ? 0 : reader.GetDecimal(amountOrd),
                        TransactionType = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd),
                        Description = reader.IsDBNull(descriptionOrd) ? "" : reader.GetString(descriptionOrd),
                        CustomerName = reader.IsDBNull(customerNameOrd) ? "" : reader.GetString(customerNameOrd)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading transactions: {ex.Message}", ex);
            }

            return transactions;
        }

        // Get transactions by PartyId (for a specific customer)
        public static List<TransactionInfo> GetTransactionsByPartyId(int partyId)
        {
            var transactions = new List<TransactionInfo>();

            string query = @"
                SELECT 
                    t.[TransactionId],
                    t.[Date],
                    t.[Amount],
                    t.[Type],
                    t.[Description],
                    t.[SaleId],
                    t.[InstallmentId],
                    p.[PartyId],
                    p.[Name] as CustomerName
                FROM [dbo].[Transactions] t
                LEFT JOIN [dbo].[Sales] s ON t.[SaleId] = s.[SaleId]
                LEFT JOIN [dbo].[Parties] p ON (s.[BuyerId] = p.[PartyId] OR s.[SellerId] = p.[PartyId])
                WHERE (s.[BuyerId] = @PartyId OR s.[SellerId] = @PartyId)
                
                ORDER BY t.[Date] DESC, t.[TransactionId] DESC";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PartyId", partyId);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int transactionIdOrd = reader.GetOrdinal("TransactionId");
                    int dateOrd = reader.GetOrdinal("Date");
                    int amountOrd = reader.GetOrdinal("Amount");
                    int typeOrd = reader.GetOrdinal("Type");
                    int descriptionOrd = reader.GetOrdinal("Description");
                    int customerNameOrd = reader.GetOrdinal("CustomerName");

                    transactions.Add(new TransactionInfo
                    {
                        TransactionId = reader.GetInt32(transactionIdOrd),
                        TransactionDate = reader.IsDBNull(dateOrd) ? DateTime.Now : reader.GetDateTime(dateOrd),
                        Amount = reader.IsDBNull(amountOrd) ? 0 : reader.GetDecimal(amountOrd),
                        TransactionType = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd),
                        Description = reader.IsDBNull(descriptionOrd) ? "" : reader.GetString(descriptionOrd),
                        CustomerName = reader.IsDBNull(customerNameOrd) ? "" : reader.GetString(customerNameOrd)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading transactions: {ex.Message}", ex);
            }

            return transactions;
        }

        public class TransactionInfo
        {
            public int TransactionId { get; set; }
            public DateTime TransactionDate { get; set; }
            public decimal Amount { get; set; }
            public string TransactionType { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
        }
    }
}

