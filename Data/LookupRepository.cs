// Data/LookupRepository.cs
using Electronics_Shop2.Models;
using Npgsql;
using System.Data;

namespace Electronics_Shop2.Data
{
    public class LookupRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public LookupRepository()
        {
            _dbConnection = new DatabaseConnection();
        }

        public List<Client> GetClients()
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

        public List<Staff> GetStaff()
        {
            var staff = new List<Staff>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = @"SELECT s.id_Staff, s.Staff_Name, s.id_Position, s.Phone_Number, s.Salary, s.Hire_Date, p.Position
                         FROM Staff s 
                         JOIN Positions p ON s.id_Position = p.id_Position 
                         ORDER BY s.Staff_Name";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        staff.Add(new Staff
                        {
                            IdStaff = reader.GetInt32("id_Staff"),
                            StaffName = reader.GetString("Staff_Name"),
                            IdPosition = reader.GetInt32("id_Position"),
                            PhoneNumber = reader.IsDBNull("Phone_Number") ? null : reader.GetString("Phone_Number"),
                            Salary = reader.IsDBNull("Salary") ? null : reader.GetDecimal("Salary"),
                            HireDate = reader.GetDateTime("Hire_Date"),
                            Position = reader.GetString("Position")
                        });
                    }
                }
            }

            return staff;
        }

        public List<PaymentType> GetPaymentTypes()
        {
            var paymentTypes = new List<PaymentType>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "SELECT id_Payment_Type, Payment_Type FROM Payment_Types ORDER BY Payment_Type";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        paymentTypes.Add(new PaymentType
                        {
                            IdPaymentType = reader.GetInt32("id_Payment_Type"),
                            PaymentTypeName = reader.GetString("Payment_Type")
                        });
                    }
                }
            }

            return paymentTypes;
        }


        public List<Category> GetCategories()
        {
            var categories = new List<Category>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "SELECT id_Category, Category_Name FROM Categories ORDER BY Category_Name";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            IdCategory = reader.GetInt32("id_Category"),
                            CategoryName = reader.GetString("Category_Name")
                        });
                    }
                }
            }

            return categories;
        }

        public List<Brand> GetBrands()
        {
            var brands = new List<Brand>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "SELECT id_Brand, Brand_Name FROM Brands ORDER BY Brand_Name";

                using (var command = new NpgsqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        brands.Add(new Brand
                        {
                            IdBrand = reader.GetInt32("id_Brand"),
                            BrandName = reader.GetString("Brand_Name")
                        });
                    }
                }
            }

            return brands;
        }

        public List<PhoneModel> GetModelsByBrand(int brandId)
        {
            var models = new List<PhoneModel>();

            using (var connection = _dbConnection.GetConnection())
            {
                connection.Open();
                var query = "SELECT id_Model, Phone_Model FROM Phone_Models WHERE id_Brand = @BrandId ORDER BY Phone_Model";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BrandId", brandId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            models.Add(new PhoneModel
                            {
                                IdModel = reader.GetInt32("id_Model"),
                                ModelName = reader.GetString("Phone_Model")
                            });
                        }
                    }
                }
            }

            return models;
        }
    }
}