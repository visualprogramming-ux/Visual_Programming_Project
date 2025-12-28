using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class PlotManagementDataAccess
    {
        // Get all plots with project information and current owner
        public static List<PlotInfo> GetAllPlots()
        {
            var plots = new List<PlotInfo>();

            // Check if OwnerId column exists
            bool ownerIdExists = false;
            try
            {
                using var checkConnection = DatabaseHelper.GetConnection();
                using var checkCommand = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Plots') AND name = 'OwnerId'", checkConnection);
                checkConnection.Open();
                ownerIdExists = ((int)checkCommand.ExecuteScalar()) > 0;
            }
            catch
            {
                ownerIdExists = false;
            }

            // Build query based on whether OwnerId column exists
            string query;
            if (ownerIdExists)
            {
                query = @"
                    SELECT pl.PlotId, pl.PlotNo, pl.SizeMarla, pl.Price, pl.Status, pl.ProjectId, 
                           pr.Name as ProjectName, 
                           ISNULL(pl.OwnerId, 0) as OwnerId, 
                           ISNULL(pt.Name, '') as OwnerName
                    FROM Plots pl
                    INNER JOIN Projects pr ON pl.ProjectId = pr.ProjectId
                    LEFT JOIN Parties pt ON pl.OwnerId = pt.PartyId
                    ORDER BY pl.PlotNo";
            }
            else
            {
                query = @"
                    SELECT pl.PlotId, pl.PlotNo, pl.SizeMarla, pl.Price, pl.Status, pl.ProjectId, 
                           pr.Name as ProjectName
                    FROM Plots pl
                    INNER JOIN Projects pr ON pl.ProjectId = pr.ProjectId
                    ORDER BY pl.PlotNo";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int plotIdOrd = reader.GetOrdinal("PlotId");
                    int plotNoOrd = reader.GetOrdinal("PlotNo");
                    int sizeMarlaOrd = reader.GetOrdinal("SizeMarla");
                    int priceOrd = reader.GetOrdinal("Price");
                    int statusOrd = reader.GetOrdinal("Status");
                    int projectIdOrd = reader.GetOrdinal("ProjectId");
                    int projectNameOrd = reader.GetOrdinal("ProjectName");
                    
                    // Get OwnerId and OwnerName if column exists
                    int? ownerId = null;
                    string ownerName = "";
                    if (ownerIdExists)
                    {
                        try
                        {
                            int ownerIdOrd = reader.GetOrdinal("OwnerId");
                            int ownerNameOrd = reader.GetOrdinal("OwnerName");
                            int ownerIdValue = reader.GetInt32(ownerIdOrd);
                            ownerId = ownerIdValue == 0 ? null : ownerIdValue;
                            ownerName = reader.IsDBNull(ownerNameOrd) ? "" : reader.GetString(ownerNameOrd);
                        }
                        catch
                        {
                            // Fallback if something goes wrong
                            ownerId = null;
                            ownerName = "";
                        }
                    }

                    plots.Add(new PlotInfo
                    {
                        PlotId = reader.GetInt32(plotIdOrd),
                        PlotNo = reader.IsDBNull(plotNoOrd) ? "" : reader.GetString(plotNoOrd),
                        SizeMarla = reader.IsDBNull(sizeMarlaOrd) ? 0 : reader.GetDecimal(sizeMarlaOrd),
                        Price = reader.IsDBNull(priceOrd) ? 0 : reader.GetDecimal(priceOrd),
                        Status = reader.IsDBNull(statusOrd) ? "Available" : reader.GetString(statusOrd),
                        ProjectId = reader.GetInt32(projectIdOrd),
                        ProjectName = reader.IsDBNull(projectNameOrd) ? "" : reader.GetString(projectNameOrd),
                        OwnerId = ownerId,
                        OwnerName = ownerName
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading plots: {ex.Message}", ex);
            }

            return plots;
        }

        private static bool CheckOwnerIdColumnExists()
        {
            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(@"
                    SELECT COUNT(*) FROM sys.columns 
                    WHERE object_id = OBJECT_ID('dbo.Plots') AND name = 'OwnerId'", connection);
                connection.Open();
                return ((int)command.ExecuteScalar()) > 0;
            }
            catch
            {
                return false;
            }
        }

        // Insert a new plot
        public static int InsertPlot(int projectId, string plotNo, decimal sizeMarla, decimal price, string status, int? ownerId = null)
        {
            bool ownerIdExists = CheckOwnerIdColumnExists();
            
            string query = @"
                INSERT INTO Plots (ProjectId, PlotNo, SizeMarla, Price, Status";
            
            if (ownerIdExists && ownerId.HasValue)
            {
                query += ", OwnerId, CreatedAt, UpdatedAt) OUTPUT INSERTED.PlotId VALUES (@ProjectId, @PlotNo, @SizeMarla, @Price, @Status, @OwnerId, GETDATE(), GETDATE())";
            }
            else
            {
                query += ", CreatedAt, UpdatedAt) OUTPUT INSERTED.PlotId VALUES (@ProjectId, @PlotNo, @SizeMarla, @Price, @Status, GETDATE(), GETDATE())";
            }

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.Parameters.AddWithValue("@PlotNo", plotNo);
                command.Parameters.AddWithValue("@SizeMarla", sizeMarla);
                command.Parameters.AddWithValue("@Price", price);
                command.Parameters.AddWithValue("@Status", status ?? "Available");
                if (ownerIdExists && ownerId.HasValue)
                {
                    command.Parameters.AddWithValue("@OwnerId", ownerId.Value);
                }

                connection.Open();
                return (int)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inserting plot: {ex.Message}", ex);
            }
        }

        // Update a plot
        public static void UpdatePlot(int plotId, int projectId, string plotNo, decimal sizeMarla, decimal price, string status, int? ownerId = null)
        {
            bool ownerIdExists = CheckOwnerIdColumnExists();
            
            string query = @"
                UPDATE Plots
                SET ProjectId = @ProjectId,
                    PlotNo = @PlotNo,
                    SizeMarla = @SizeMarla,
                    Price = @Price,
                    Status = @Status";
            
            if (ownerIdExists)
            {
                query += ", OwnerId = @OwnerId";
            }
            
            query += @",
                    UpdatedAt = GETDATE()
                WHERE PlotId = @PlotId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                command.Parameters.AddWithValue("@ProjectId", projectId);
                command.Parameters.AddWithValue("@PlotNo", plotNo);
                command.Parameters.AddWithValue("@SizeMarla", sizeMarla);
                command.Parameters.AddWithValue("@Price", price);
                command.Parameters.AddWithValue("@Status", status ?? "Available");
                
                if (ownerIdExists)
                {
                    command.Parameters.AddWithValue("@OwnerId", ownerId ?? (object)DBNull.Value);
                }

                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating plot: {ex.Message}", ex);
            }
        }

        // Check if plot has related sales
        public static bool HasRelatedSales(int plotId)
        {
            string query = "SELECT COUNT(1) FROM Sales WHERE PlotId = @PlotId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                connection.Open();
                return (int)command.ExecuteScalar() > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking related sales: {ex.Message}", ex);
            }
        }

        // Get count of related sales
        public static int GetSaleCount(int plotId)
        {
            string query = "SELECT COUNT(1) FROM Sales WHERE PlotId = @PlotId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                connection.Open();
                return (int)command.ExecuteScalar();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting sale count: {ex.Message}", ex);
            }
        }

        // Delete a plot
        public static void DeletePlot(int plotId)
        {
            // First check if plot has related sales
            if (HasRelatedSales(plotId))
            {
                int saleCount = GetSaleCount(plotId);
                throw new Exception($"Cannot delete plot. This plot has {saleCount} sale(s) associated with it. Please delete the sales first.");
            }

            string query = "DELETE FROM Plots WHERE PlotId = @PlotId";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PlotId", plotId);
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // If it's already our custom exception, rethrow it
                if (ex.Message.Contains("Cannot delete plot"))
                {
                    throw;
                }
                throw new Exception($"Error deleting plot: {ex.Message}", ex);
            }
        }

        public class PlotInfo
        {
            public int PlotId { get; set; }
            public string PlotNo { get; set; } = string.Empty;
            public decimal SizeMarla { get; set; }
            public decimal Price { get; set; }
            public string Status { get; set; } = string.Empty;
            public int ProjectId { get; set; }
            public string ProjectName { get; set; } = string.Empty;
            public int? OwnerId { get; set; }
            public string OwnerName { get; set; } = string.Empty;
        }
    }
}

