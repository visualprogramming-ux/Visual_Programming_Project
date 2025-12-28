using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Project.Data
{
    public class PartyDataAccess
    {
        // Get all buyers (Parties with Type = 'Buyer')
        public static List<BuyerInfo> GetAllBuyers()
        {
            var buyers = new List<BuyerInfo>();

            string query = @"
                SELECT PartyId, Name, ContactPhone, ContactEmail
                FROM Parties
                WHERE Type = 'Buyer' AND Status = 'Active'
                ORDER BY Name";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    buyers.Add(new BuyerInfo
                    {
                        BuyerId = reader.GetInt32(reader.GetOrdinal("PartyId")).ToString(),
                        Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? "" : reader.GetString(reader.GetOrdinal("Name"))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading buyers: {ex.Message}", ex);
            }

            return buyers;
        }

        // Get all parties (all types: Buyer, Seller, Agent)
        public static List<PartyInfo> GetAllParties()
        {
            var parties = new List<PartyInfo>();

            // SQL query explicitly includes CNIC, ContactPhone, and Address columns
            string query = @"
                SELECT 
                    [PartyId], 
                    [Type], 
                    [Name], 
                    [CNIC], 
                    [ContactPhone], 
                    [ContactEmail], 
                    [Address], 
                    [Status]
                FROM [dbo].[Parties]
                ORDER BY [Type], [Name]";

            try
            {
                using var connection = DatabaseHelper.GetConnection();
                using var command = new SqlCommand(query, connection);
                connection.Open();

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Get column ordinals for all fields including CNIC, ContactPhone, and Address
                    int partyIdOrd = reader.GetOrdinal("PartyId");
                    int typeOrd = reader.GetOrdinal("Type");
                    int nameOrd = reader.GetOrdinal("Name");
                    int cnicOrd = reader.GetOrdinal("CNIC");
                    int contactPhoneOrd = reader.GetOrdinal("ContactPhone");
                    int contactEmailOrd = reader.GetOrdinal("ContactEmail");
                    int addressOrd = reader.GetOrdinal("Address");
                    int statusOrd = reader.GetOrdinal("Status");

                    // Create PartyInfo object with all fields including CNIC, ContactPhone, and Address
                    parties.Add(new PartyInfo
                    {
                        PartyId = reader.GetInt32(partyIdOrd),
                        Type = reader.IsDBNull(typeOrd) ? "" : reader.GetString(typeOrd),
                        Name = reader.IsDBNull(nameOrd) ? "" : reader.GetString(nameOrd),
                        CNIC = reader.IsDBNull(cnicOrd) ? "" : reader.GetString(cnicOrd),
                        ContactPhone = reader.IsDBNull(contactPhoneOrd) ? "" : reader.GetString(contactPhoneOrd),
                        ContactEmail = reader.IsDBNull(contactEmailOrd) ? "" : reader.GetString(contactEmailOrd),
                        Address = reader.IsDBNull(addressOrd) ? "" : reader.GetString(addressOrd),
                        Status = reader.IsDBNull(statusOrd) ? "Active" : reader.GetString(statusOrd)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading parties: {ex.Message}", ex);
            }

            return parties;
        }

        public class BuyerInfo
        {
            public string BuyerId { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public class PartyInfo
        {
            public int PartyId { get; set; }
            public string Type { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string CNIC { get; set; } = string.Empty;
            public string ContactPhone { get; set; } = string.Empty;
            public string ContactEmail { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }
    }
}

