using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Project.Pages;

namespace Project.Data
{
    public class PaymentPlanDataAccess
    {
        // Get all payment plans with buyer and plot information
        public static List<Page48Page.PaymentPlan> GetAllPaymentPlans()
        {
            var plans = new List<Page48Page.PaymentPlan>();

            // Check which columns exist in PaymentPlans table
            using var connection = DatabaseHelper.GetConnection();
            connection.Open();
            
            var existingColumns = new System.Collections.Generic.HashSet<string>();
            string checkColumnsQuery = @"
                SELECT COLUMN_NAME 
                FROM INFORMATION_SCHEMA.COLUMNS 
                WHERE TABLE_NAME = 'PaymentPlans' 
                AND COLUMN_NAME IN ('PlanType', 'Status', 'OverdueReminder', 'UpcomingReminder')";
            
            using var checkCmd = new SqlCommand(checkColumnsQuery, connection);
            using var checkReader = checkCmd.ExecuteReader();
            while (checkReader.Read())
            {
                existingColumns.Add(checkReader.GetString(0));
            }
            checkReader.Close();

            // Build query based on existing columns
            string query = @"
                SELECT 
                    pp.PaymentPlanId,
                    pp.TotalAmount,
                    pp.DownPayment,
                    pp.NumberOfInstallments,
                    pp.InstallmentAmount,
                    pp.StartDate,
                    s.SaleId,
                    s.Status as SaleStatus,
                    p.PartyId as BuyerId,
                    p.Name as BuyerName,
                    pl.PlotId,
                    pl.PlotNo,
                    pr.Name as ProjectName";
            
            if (existingColumns.Contains("PlanType"))
                query += ", pp.PlanType";
            if (existingColumns.Contains("Status"))
                query += ", pp.Status as PaymentPlanStatus";
            if (existingColumns.Contains("OverdueReminder"))
                query += ", pp.OverdueReminder";
            if (existingColumns.Contains("UpcomingReminder"))
                query += ", pp.UpcomingReminder";
            
            query += @"
                FROM PaymentPlans pp
                INNER JOIN Sales s ON pp.SaleId = s.SaleId
                INNER JOIN Parties p ON s.BuyerId = p.PartyId
                INNER JOIN Plots pl ON s.PlotId = pl.PlotId
                INNER JOIN Projects pr ON pl.ProjectId = pr.ProjectId
                ORDER BY pp.PaymentPlanId DESC";

            try
            {
                using var command = new SqlCommand(query, connection);
                using var reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    int paymentPlanIdOrd = reader.GetOrdinal("PaymentPlanId");
                    int buyerIdOrd = reader.GetOrdinal("BuyerId");
                    int buyerNameOrd = reader.GetOrdinal("BuyerName");
                    int plotIdOrd = reader.GetOrdinal("PlotId");
                    int plotNoOrd = reader.GetOrdinal("PlotNo");
                    int saleStatusOrd = reader.GetOrdinal("SaleStatus");
                    
                    string planType = "Monthly";
                    string status = "Active";
                    bool overdueReminder = true;
                    bool upcomingReminder = true;
                    
                    if (existingColumns.Contains("PlanType"))
                    {
                        int planTypeOrd = reader.GetOrdinal("PlanType");
                        planType = reader.IsDBNull(planTypeOrd) ? "Monthly" : reader.GetString(planTypeOrd);
                    }
                    
                    if (existingColumns.Contains("Status"))
                    {
                        int statusOrd = reader.GetOrdinal("PaymentPlanStatus");
                        status = reader.IsDBNull(statusOrd) ? "Active" : reader.GetString(statusOrd);
                    }
                    else
                    {
                        status = reader.IsDBNull(saleStatusOrd) ? "Active" : reader.GetString(saleStatusOrd);
                    }
                    
                    if (existingColumns.Contains("OverdueReminder"))
                    {
                        int overdueOrd = reader.GetOrdinal("OverdueReminder");
                        overdueReminder = !reader.IsDBNull(overdueOrd) && reader.GetBoolean(overdueOrd);
                    }
                    
                    if (existingColumns.Contains("UpcomingReminder"))
                    {
                        int upcomingOrd = reader.GetOrdinal("UpcomingReminder");
                        upcomingReminder = !reader.IsDBNull(upcomingOrd) && reader.GetBoolean(upcomingOrd);
                    }
                    
                    plans.Add(new Page48Page.PaymentPlan
                    {
                        PlanId = "PLN" + reader.GetInt32(paymentPlanIdOrd).ToString("D3"),
                        PaymentPlanId = reader.GetInt32(paymentPlanIdOrd),
                        BuyerId = reader.GetInt32(buyerIdOrd).ToString(),
                        BuyerName = reader.IsDBNull(buyerNameOrd) ? "" : reader.GetString(buyerNameOrd),
                        PlotId = reader.GetInt32(plotIdOrd).ToString(),
                        PlotNo = reader.IsDBNull(plotNoOrd) ? "" : reader.GetString(plotNoOrd),
                        PlanType = planType,
                        TotalAmount = reader.GetDecimal(reader.GetOrdinal("TotalAmount")),
                        DownPayment = reader.GetDecimal(reader.GetOrdinal("DownPayment")),
                        InstallmentCount = reader.GetInt32(reader.GetOrdinal("NumberOfInstallments")),
                        InstallmentAmount = reader.GetDecimal(reader.GetOrdinal("InstallmentAmount")),
                        StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                        Status = status,
                        OverdueReminder = overdueReminder,
                        UpcomingReminder = upcomingReminder
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading payment plans: {ex.Message}", ex);
            }

            return plans;
        }

        // Insert a new payment plan
        // Note: This assumes a Sale already exists. You may need to create a Sale first.
        public static int InsertPaymentPlan(int saleId, decimal totalAmount, decimal downPayment, 
            int numberOfInstallments, decimal installmentAmount, DateTime startDate, 
            string planType = "Monthly", string status = "Active", bool overdueReminder = true, bool upcomingReminder = true)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Check which columns exist
                var existingColumns = new System.Collections.Generic.HashSet<string>();
                string checkColumnsQuery = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PaymentPlans' 
                    AND COLUMN_NAME IN ('PlanType', 'Status', 'OverdueReminder', 'UpcomingReminder')";
                
                using var checkCmd = new SqlCommand(checkColumnsQuery, connection);
                using var checkReader = checkCmd.ExecuteReader();
                while (checkReader.Read())
                {
                    existingColumns.Add(checkReader.GetString(0));
                }
                checkReader.Close();
                
                // Build insert query based on existing columns
                string columns = "SaleId, TotalAmount, DownPayment, NumberOfInstallments, InstallmentAmount, StartDate";
                string values = "@SaleId, @TotalAmount, @DownPayment, @NumberOfInstallments, @InstallmentAmount, @StartDate";
                
                if (existingColumns.Contains("PlanType"))
                {
                    columns += ", PlanType";
                    values += ", @PlanType";
                }
                if (existingColumns.Contains("Status"))
                {
                    columns += ", Status";
                    values += ", @Status";
                }
                if (existingColumns.Contains("OverdueReminder"))
                {
                    columns += ", OverdueReminder";
                    values += ", @OverdueReminder";
                }
                if (existingColumns.Contains("UpcomingReminder"))
                {
                    columns += ", UpcomingReminder";
                    values += ", @UpcomingReminder";
                }
                
                string query = $@"
                    INSERT INTO PaymentPlans ({columns})
                    OUTPUT INSERTED.PaymentPlanId
                    VALUES ({values})";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleId", saleId);
                command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                command.Parameters.AddWithValue("@DownPayment", downPayment);
                command.Parameters.AddWithValue("@NumberOfInstallments", numberOfInstallments);
                command.Parameters.AddWithValue("@InstallmentAmount", installmentAmount);
                command.Parameters.AddWithValue("@StartDate", startDate.Date);
                
                if (existingColumns.Contains("PlanType"))
                    command.Parameters.AddWithValue("@PlanType", planType);
                if (existingColumns.Contains("Status"))
                    command.Parameters.AddWithValue("@Status", status);
                if (existingColumns.Contains("OverdueReminder"))
                    command.Parameters.AddWithValue("@OverdueReminder", overdueReminder);
                if (existingColumns.Contains("UpcomingReminder"))
                    command.Parameters.AddWithValue("@UpcomingReminder", upcomingReminder);

                var paymentPlanId = (int)command.ExecuteScalar();

                // Create installment records
                CreateInstallments(paymentPlanId, numberOfInstallments, installmentAmount, startDate);

                return paymentPlanId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting payment plan: {ex.Message}", ex);
            }
        }

        // Create installment records for a payment plan
        private static void CreateInstallments(int paymentPlanId, int numberOfInstallments, 
            decimal installmentAmount, DateTime startDate)
        {
            string query = @"
                INSERT INTO Installments (PaymentPlanId, InstallmentNo, DueDate, Amount, Status)
                VALUES (@PaymentPlanId, @InstallmentNo, @DueDate, @Amount, 'Due')";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();

                for (int i = 1; i <= numberOfInstallments; i++)
                {
                    using var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@PaymentPlanId", paymentPlanId);
                    command.Parameters.AddWithValue("@InstallmentNo", i);
                    command.Parameters.AddWithValue("@DueDate", startDate.AddMonths(i - 1).Date);
                    command.Parameters.AddWithValue("@Amount", installmentAmount);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating installments: {ex.Message}", ex);
            }
        }

        // Update a payment plan
        public static void UpdatePaymentPlan(int paymentPlanId, decimal totalAmount, decimal downPayment,
            int numberOfInstallments, decimal installmentAmount, DateTime startDate,
            string planType = "Monthly", string status = "Active", bool overdueReminder = true, bool upcomingReminder = true)
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Check which columns exist
                var existingColumns = new System.Collections.Generic.HashSet<string>();
                string checkColumnsQuery = @"
                    SELECT COLUMN_NAME 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'PaymentPlans' 
                    AND COLUMN_NAME IN ('PlanType', 'Status', 'OverdueReminder', 'UpcomingReminder')";
                
                using var checkCmd = new SqlCommand(checkColumnsQuery, connection);
                using var checkReader = checkCmd.ExecuteReader();
                while (checkReader.Read())
                {
                    existingColumns.Add(checkReader.GetString(0));
                }
                checkReader.Close();
                
                // Build update query
                string setClause = @"
                    TotalAmount = @TotalAmount,
                    DownPayment = @DownPayment,
                    NumberOfInstallments = @NumberOfInstallments,
                    InstallmentAmount = @InstallmentAmount,
                    StartDate = @StartDate";
                
                if (existingColumns.Contains("PlanType"))
                    setClause += ", PlanType = @PlanType";
                if (existingColumns.Contains("Status"))
                    setClause += ", Status = @Status";
                if (existingColumns.Contains("OverdueReminder"))
                    setClause += ", OverdueReminder = @OverdueReminder";
                if (existingColumns.Contains("UpcomingReminder"))
                    setClause += ", UpcomingReminder = @UpcomingReminder";
                
                string query = $@"
                    UPDATE PaymentPlans
                    SET {setClause}
                    WHERE PaymentPlanId = @PaymentPlanId";

                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentPlanId", paymentPlanId);
                command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                command.Parameters.AddWithValue("@DownPayment", downPayment);
                command.Parameters.AddWithValue("@NumberOfInstallments", numberOfInstallments);
                command.Parameters.AddWithValue("@InstallmentAmount", installmentAmount);
                command.Parameters.AddWithValue("@StartDate", startDate.Date);
                
                if (existingColumns.Contains("PlanType"))
                    command.Parameters.AddWithValue("@PlanType", planType);
                if (existingColumns.Contains("Status"))
                    command.Parameters.AddWithValue("@Status", status);
                if (existingColumns.Contains("OverdueReminder"))
                    command.Parameters.AddWithValue("@OverdueReminder", overdueReminder);
                if (existingColumns.Contains("UpcomingReminder"))
                    command.Parameters.AddWithValue("@UpcomingReminder", upcomingReminder);

                command.ExecuteNonQuery();

                // Delete old installments and create new ones
                DeleteInstallments(paymentPlanId);
                CreateInstallments(paymentPlanId, numberOfInstallments, installmentAmount, startDate);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating payment plan: {ex.Message}", ex);
            }
        }

        // Delete installments for a payment plan
        private static void DeleteInstallments(int paymentPlanId)
        {
            string query = "DELETE FROM Installments WHERE PaymentPlanId = @PaymentPlanId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentPlanId", paymentPlanId);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting installments: {ex.Message}", ex);
            }
        }

        // Delete a payment plan
        public static void DeletePaymentPlan(int paymentPlanId)
        {
            // Delete installments first (foreign key constraint)
            DeleteInstallments(paymentPlanId);

            string query = "DELETE FROM PaymentPlans WHERE PaymentPlanId = @PaymentPlanId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PaymentPlanId", paymentPlanId);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting payment plan: {ex.Message}", ex);
            }
        }

        // Get or create a Sale for a buyer and plot
        // This is a helper method since PaymentPlans require a SaleId
        public static int GetOrCreateSale(int buyerId, int plotId, decimal salePrice, decimal downPayment)
        {
            // First, try to find an existing sale
            string findQuery = @"
                SELECT TOP 1 SaleId 
                FROM Sales 
                WHERE BuyerId = @BuyerId AND PlotId = @PlotId 
                ORDER BY SaleId DESC";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();

                using var findCommand = new SqlCommand(findQuery, connection);
                findCommand.Parameters.AddWithValue("@BuyerId", buyerId);
                findCommand.Parameters.AddWithValue("@PlotId", plotId);

                var existingSaleId = findCommand.ExecuteScalar();
                if (existingSaleId != null)
                {
                    return (int)existingSaleId;
                }

                // If no sale exists, create one
                // Get ProjectId from Plot
                string getProjectQuery = "SELECT ProjectId FROM Plots WHERE PlotId = @PlotId";
                using var getProjectCommand = new SqlCommand(getProjectQuery, connection);
                getProjectCommand.Parameters.AddWithValue("@PlotId", plotId);
                var projectId = getProjectCommand.ExecuteScalar();

                if (projectId == null)
                {
                    throw new Exception("Plot not found");
                }

                string insertQuery = @"
                    INSERT INTO Sales (ProjectId, PlotId, BuyerId, SalePrice, DownPayment, SaleDate, Status)
                    OUTPUT INSERTED.SaleId
                    VALUES (@ProjectId, @PlotId, @BuyerId, @SalePrice, @DownPayment, @SaleDate, 'Active')";

                using var insertCommand = new SqlCommand(insertQuery, connection);
                insertCommand.Parameters.AddWithValue("@ProjectId", projectId);
                insertCommand.Parameters.AddWithValue("@PlotId", plotId);
                insertCommand.Parameters.AddWithValue("@BuyerId", buyerId);
                insertCommand.Parameters.AddWithValue("@SalePrice", salePrice);
                insertCommand.Parameters.AddWithValue("@DownPayment", downPayment);
                insertCommand.Parameters.AddWithValue("@SaleDate", DateTime.Now.Date);

                return (int)insertCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting or creating sale: {ex.Message}", ex);
            }
        }
    }
}

