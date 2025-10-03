// Data/ClientRepository.cs
using Electronics_Shop2.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace Electronics_Shop2.Data
{
    public class ClientRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public ClientRepository()
        {
            _dbConnection = new DatabaseConnection();
        }

        public void AddClient(Client client)
        {
            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"
                    INSERT INTO Clients (Name, Email, Phone_Number, Address, Registration_Date)
                    VALUES (@Name, @Email, @PhoneNumber, @Address, @RegistrationDate)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", client.Name);
                    command.Parameters.AddWithValue("@Email", client.Email ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Address", client.Address ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@RegistrationDate", client.RegistrationDate);

                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "SELECT id_Client, Name, Email, Phone_Number, Address, Registration_Date FROM Clients ORDER BY Name";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            IdClient = reader.GetInt32("id_Client"),
                            Name = reader.GetString("Name"),
                            Email = reader.IsDBNull("Email") ? null : reader.GetString("Email"),
                            PhoneNumber = reader.IsDBNull("Phone_Number") ? null : reader.GetString("Phone_Number"),
                            Address = reader.IsDBNull("Address") ? null : reader.GetString("Address"),
                            RegistrationDate = reader.GetDateTime("Registration_Date")
                        });
                    }
                }
            }

            return clients;
        }
    }
}