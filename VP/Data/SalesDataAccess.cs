using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class SalesDataAccess
    {
        // Get all sales with plot and party information
        public static List<SaleInfo> GetAllSales()
        {
            var sales = new List<SaleInfo>();

            string query = @"
                SELECT 
                    s.SaleId,
                    s.SaleDate,
                    s.SalePrice,
                    s.DownPayment,
                    s.Status,
                    ISNULL(s.Notes, '') as Notes,
                    pl.PlotId,
                    pl.PlotNo,
                    pr.ProjectId,
                    pr.Name as ProjectName,
                    buyer.PartyId as BuyerId,
                    buyer.Name as BuyerName,
                    buyer.Type as BuyerType,
                    seller.PartyId as SellerId,
                    seller.Name as SellerName,
                    seller.Type as SellerType
                FROM Sales s
                INNER JOIN Plots pl ON s.PlotId = pl.PlotId
                INNER JOIN Projects pr ON s.ProjectId = pr.ProjectId
                LEFT JOIN Parties buyer ON s.BuyerId = buyer.PartyId
                LEFT JOIN Parties seller ON s.SellerId = seller.PartyId
                ORDER BY s.SaleDate DESC";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int saleIdOrd = reader.GetOrdinal("SaleId");
                    int saleDateOrd = reader.GetOrdinal("SaleDate");
                    int salePriceOrd = reader.GetOrdinal("SalePrice");
                    int downPaymentOrd = reader.GetOrdinal("DownPayment");
                    int statusOrd = reader.GetOrdinal("Status");
                    int notesOrd = reader.GetOrdinal("Notes");
                    int plotIdOrd = reader.GetOrdinal("PlotId");
                    int plotNoOrd = reader.GetOrdinal("PlotNo");
                    int projectNameOrd = reader.GetOrdinal("ProjectName");
                    int buyerIdOrd = reader.GetOrdinal("BuyerId");
                    int buyerNameOrd = reader.GetOrdinal("BuyerName");
                    int buyerTypeOrd = reader.GetOrdinal("BuyerType");
                    int sellerIdOrd = reader.GetOrdinal("SellerId");
                    int sellerNameOrd = reader.GetOrdinal("SellerName");
                    int sellerTypeOrd = reader.GetOrdinal("SellerType");

                    string notes = reader.IsDBNull(notesOrd) ? "" : reader.GetString(notesOrd);

                    sales.Add(new SaleInfo
                    {
                        SaleId = reader.GetInt32(saleIdOrd),
                        SaleDate = reader.GetDateTime(saleDateOrd),
                        SalePrice = reader.IsDBNull(salePriceOrd) ? 0 : reader.GetDecimal(salePriceOrd),
                        DownPayment = reader.IsDBNull(downPaymentOrd) ? 0 : reader.GetDecimal(downPaymentOrd),
                        Status = reader.IsDBNull(statusOrd) ? "Active" : reader.GetString(statusOrd),
                        PlotId = reader.GetInt32(plotIdOrd),
                        PlotNo = reader.IsDBNull(plotNoOrd) ? "" : reader.GetString(plotNoOrd),
                        ProjectName = reader.IsDBNull(projectNameOrd) ? "" : reader.GetString(projectNameOrd),
                        BuyerId = reader.IsDBNull(buyerIdOrd) ? (int?)null : reader.GetInt32(buyerIdOrd),
                        BuyerName = reader.IsDBNull(buyerNameOrd) ? "" : reader.GetString(buyerNameOrd),
                        BuyerType = reader.IsDBNull(buyerTypeOrd) ? "" : reader.GetString(buyerTypeOrd),
                        SellerId = reader.IsDBNull(sellerIdOrd) ? (int?)null : reader.GetInt32(sellerIdOrd),
                        SellerName = reader.IsDBNull(sellerNameOrd) ? "" : reader.GetString(sellerNameOrd),
                        SellerType = reader.IsDBNull(sellerTypeOrd) ? "" : reader.GetString(sellerTypeOrd),
                        Notes = notes
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading sales: {ex.Message}", ex);
            }

            return sales;
        }

        // Insert a new sale (ownership mapping) and update plot owner
        public static int InsertSale(int projectId, int plotId, int? buyerId, int? sellerId, 
            decimal salePrice, decimal? downPayment, DateTime saleDate, string status, string notes = "")
        {
            string query = @"
                INSERT INTO Sales (ProjectId, PlotId, BuyerId, SellerId, SalePrice, DownPayment, SaleDate, Status, Notes, CreatedAt, UpdatedAt)
                OUTPUT INSERTED.SaleId
                VALUES (@ProjectId, @PlotId, @BuyerId, @SellerId, @SalePrice, @DownPayment, @SaleDate, @Status, @Notes, GETDATE(), GETDATE())";

            // Update plot owner if OwnerId column exists
            string updatePlotQuery = @"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Plots') AND name = 'OwnerId')
                BEGIN
                    UPDATE Plots
                    SET OwnerId = @BuyerId,
                        Status = CASE WHEN @BuyerId IS NOT NULL THEN 'Sold' ELSE Status END,
                        UpdatedAt = GETDATE()
                    WHERE PlotId = @PlotId
                END";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Insert the sale
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.Parameters.AddWithValue("@PlotId", plotId);
                command.Parameters.AddWithValue("@BuyerId", buyerId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SellerId", sellerId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SalePrice", salePrice);
                command.Parameters.AddWithValue("@DownPayment", downPayment ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SaleDate", saleDate.Date);
                command.Parameters.AddWithValue("@Status", status ?? "Active");
                command.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);

                int saleId = (int)command.ExecuteScalar();

                // Update plot owner if buyer exists
                if (buyerId.HasValue)
                {
                    using var updateCommand = new SqlCommand(updatePlotQuery, connection);
                    updateCommand.Parameters.AddWithValue("@PlotId", plotId);
                    updateCommand.Parameters.AddWithValue("@BuyerId", buyerId.Value);
                    updateCommand.ExecuteNonQuery();
                }

                return saleId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting sale: {ex.Message}", ex);
            }
        }

        // Update a sale and update plot owner
        public static void UpdateSale(int saleId, int? buyerId, int? sellerId, 
            decimal salePrice, decimal? downPayment, DateTime saleDate, string status, string notes = "")
        {
            // First get the PlotId for this sale
            string getPlotQuery = "SELECT PlotId FROM Sales WHERE SaleId = @SaleId";
            
            string query = @"
                UPDATE Sales
                SET BuyerId = @BuyerId,
                    SellerId = @SellerId,
                    SalePrice = @SalePrice,
                    DownPayment = @DownPayment,
                    SaleDate = @SaleDate,
                    Status = @Status,
                    Notes = @Notes,
                    UpdatedAt = GETDATE()
                WHERE SaleId = @SaleId";

            // Update plot owner if OwnerId column exists
            string updatePlotQuery = @"
                IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Plots') AND name = 'OwnerId')
                BEGIN
                    UPDATE Plots
                    SET OwnerId = @BuyerId,
                        Status = CASE WHEN @BuyerId IS NOT NULL THEN 'Sold' ELSE Status END,
                        UpdatedAt = GETDATE()
                    WHERE PlotId = @PlotId
                END";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                connection.Open();
                
                // Get PlotId
                int plotId = 0;
                using (var getCommand = new SqlCommand(getPlotQuery, connection))
                {
                    getCommand.Parameters.AddWithValue("@SaleId", saleId);
                    var result = getCommand.ExecuteScalar();
                    if (result != null)
                        plotId = (int)result;
                }

                // Update the sale
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleId", saleId);
                command.Parameters.AddWithValue("@BuyerId", buyerId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SellerId", sellerId ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SalePrice", salePrice);
                command.Parameters.AddWithValue("@DownPayment", downPayment ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@SaleDate", saleDate.Date);
                command.Parameters.AddWithValue("@Status", status ?? "Active");
                command.Parameters.AddWithValue("@Notes", string.IsNullOrWhiteSpace(notes) ? (object)DBNull.Value : notes);
                command.ExecuteNonQuery();

                // Update plot owner if buyer exists and this is the latest sale for the plot
                if (buyerId.HasValue && plotId > 0)
                {
                    // Check if this is the latest sale for this plot
                    string checkLatestQuery = @"
                        SELECT COUNT(*) 
                        FROM Sales 
                        WHERE PlotId = @PlotId 
                          AND (SaleDate > @SaleDate OR (SaleDate = @SaleDate AND SaleId > @SaleId))";
                    
                    using (var checkCommand = new SqlCommand(checkLatestQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@PlotId", plotId);
                        checkCommand.Parameters.AddWithValue("@SaleDate", saleDate.Date);
                        checkCommand.Parameters.AddWithValue("@SaleId", saleId);
                        int newerSales = (int)checkCommand.ExecuteScalar();
                        
                        // Only update if this is the latest sale
                        if (newerSales == 0)
                        {
                            using var updateCommand = new SqlCommand(updatePlotQuery, connection);
                            updateCommand.Parameters.AddWithValue("@PlotId", plotId);
                            updateCommand.Parameters.AddWithValue("@BuyerId", buyerId.Value);
                            updateCommand.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating sale: {ex.Message}", ex);
            }
        }

        // Delete a sale
        public static void DeleteSale(int saleId)
        {
            string query = "DELETE FROM Sales WHERE SaleId = @SaleId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleId", saleId);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting sale: {ex.Message}", ex);
            }
        }

        // Get sale by plot ID (to check if plot already has a sale)
        public static SaleInfo? GetSaleByPlotId(int plotId)
        {
            string query = @"
                SELECT TOP 1
                    s.SaleId,
                    s.SaleDate,
                    s.SalePrice,
                    s.DownPayment,
                    s.Status,
                    ISNULL(s.Notes, '') as Notes,
                    pl.PlotId,
                    pl.PlotNo,
                    pr.ProjectId,
                    pr.Name as ProjectName,
                    buyer.PartyId as BuyerId,
                    buyer.Name as BuyerName,
                    buyer.Type as BuyerType,
                    seller.PartyId as SellerId,
                    seller.Name as SellerName,
                    seller.Type as SellerType
                FROM Sales s
                INNER JOIN Plots pl ON s.PlotId = pl.PlotId
                INNER JOIN Projects pr ON s.ProjectId = pr.ProjectId
                LEFT JOIN Parties buyer ON s.BuyerId = buyer.PartyId
                LEFT JOIN Parties seller ON s.SellerId = seller.PartyId
                WHERE pl.PlotId = @PlotId
                ORDER BY s.SaleDate DESC";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                connection.Open();

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int saleIdOrd = reader.GetOrdinal("SaleId");
                    int saleDateOrd = reader.GetOrdinal("SaleDate");
                    int salePriceOrd = reader.GetOrdinal("SalePrice");
                    int downPaymentOrd = reader.GetOrdinal("DownPayment");
                    int statusOrd = reader.GetOrdinal("Status");
                    int notesOrd = reader.GetOrdinal("Notes");
                    int plotIdOrd = reader.GetOrdinal("PlotId");
                    int plotNoOrd = reader.GetOrdinal("PlotNo");
                    int projectNameOrd = reader.GetOrdinal("ProjectName");
                    int buyerIdOrd = reader.GetOrdinal("BuyerId");
                    int buyerNameOrd = reader.GetOrdinal("BuyerName");
                    int buyerTypeOrd = reader.GetOrdinal("BuyerType");
                    int sellerIdOrd = reader.GetOrdinal("SellerId");
                    int sellerNameOrd = reader.GetOrdinal("SellerName");
                    int sellerTypeOrd = reader.GetOrdinal("SellerType");

                    string notes = reader.IsDBNull(notesOrd) ? "" : reader.GetString(notesOrd);

                    return new SaleInfo
                    {
                        SaleId = reader.GetInt32(saleIdOrd),
                        SaleDate = reader.GetDateTime(saleDateOrd),
                        SalePrice = reader.IsDBNull(salePriceOrd) ? 0 : reader.GetDecimal(salePriceOrd),
                        DownPayment = reader.IsDBNull(downPaymentOrd) ? 0 : reader.GetDecimal(downPaymentOrd),
                        Status = reader.IsDBNull(statusOrd) ? "Active" : reader.GetString(statusOrd),
                        PlotId = reader.GetInt32(plotIdOrd),
                        PlotNo = reader.IsDBNull(plotNoOrd) ? "" : reader.GetString(plotNoOrd),
                        ProjectName = reader.IsDBNull(projectNameOrd) ? "" : reader.GetString(projectNameOrd),
                        BuyerId = reader.IsDBNull(buyerIdOrd) ? (int?)null : reader.GetInt32(buyerIdOrd),
                        BuyerName = reader.IsDBNull(buyerNameOrd) ? "" : reader.GetString(buyerNameOrd),
                        BuyerType = reader.IsDBNull(buyerTypeOrd) ? "" : reader.GetString(buyerTypeOrd),
                        SellerId = reader.IsDBNull(sellerIdOrd) ? (int?)null : reader.GetInt32(sellerIdOrd),
                        SellerName = reader.IsDBNull(sellerNameOrd) ? "" : reader.GetString(sellerNameOrd),
                        SellerType = reader.IsDBNull(sellerTypeOrd) ? "" : reader.GetString(sellerTypeOrd),
                        Notes = notes
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading sale by plot: {ex.Message}", ex);
            }

            return null;
        }

        public class SaleInfo
        {
            public int SaleId { get; set; }
            public DateTime SaleDate { get; set; }
            public decimal SalePrice { get; set; }
            public decimal DownPayment { get; set; }
            public string Status { get; set; } = string.Empty;
            public int PlotId { get; set; }
            public string PlotNo { get; set; } = string.Empty;
            public string ProjectName { get; set; } = string.Empty;
            public int? BuyerId { get; set; }
            public string BuyerName { get; set; } = string.Empty;
            public string BuyerType { get; set; } = string.Empty;
            public int? SellerId { get; set; }
            public string SellerName { get; set; } = string.Empty;
            public string SellerType { get; set; } = string.Empty;
            public string Notes { get; set; } = string.Empty;
        }
    }
}

