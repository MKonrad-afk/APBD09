using APBD09.Models;
using Microsoft.Data.SqlClient;

namespace APBD09.Services;

public class WarehouseService : IWarehouseService
{
    private readonly string _connectionString;

    public WarehouseService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public int AddProductToWarehouse(ProductWarehouseRequest request)
    {
        using (var connection = new SqlConnection(_connectionString))
        using (var command = new SqlCommand())
        {
            connection.Open();
            command.Connection = connection;
            var transaction = connection.BeginTransaction();
            command.Transaction = transaction;
             try
            {
                if (request.Amount <= 0)
                    throw new ArgumentException("Amount must be greater than 0.");

                command.CommandText = "SELECT COUNT(*) FROM Product WHERE IdProduct = @IdProduct";
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                if ((int)command.ExecuteScalar() == 0)
                    throw new Exception("Product not found.");

                command.CommandText = "SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                if ((int)command.ExecuteScalar() == 0)
                    throw new Exception("Warehouse not found.");

                command.CommandText = @"SELECT TOP 1 IdOrder FROM [Order] 
                                        WHERE IdProduct = @IdProduct AND Amount = @Amount AND CreatedAt < @CreatedAt";
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@CreatedAt", request.CreatedAt);
                var orderId = command.ExecuteScalar();
                if (orderId == null)
                    throw new Exception("Order not found.");

                int idOrder = (int)orderId;

                command.CommandText = "SELECT COUNT(*) FROM Product_Warehouse WHERE IdOrder = @IdOrder";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                if ((int)command.ExecuteScalar() > 0)
                    throw new Exception("Order already fulfilled.");

                command.CommandText = "UPDATE [Order] SET FulfilledAt = GETDATE() WHERE IdOrder = @IdOrder";
                command.ExecuteNonQuery();

                command.CommandText = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                var price = (decimal)command.ExecuteScalar();
                var totalPrice = price * request.Amount;

                command.CommandText = @"INSERT INTO Product_Warehouse 
                        (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                        VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@IdWarehouse", request.IdWarehouse);
                command.Parameters.AddWithValue("@IdProduct", request.IdProduct);
                command.Parameters.AddWithValue("@IdOrder", idOrder);
                command.Parameters.AddWithValue("@Amount", request.Amount);
                command.Parameters.AddWithValue("@Price", totalPrice);

                int insertedId = Convert.ToInt32(command.ExecuteScalar());
                transaction.Commit();
                return insertedId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        
    }
}