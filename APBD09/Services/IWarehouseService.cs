using APBD09.Models;

namespace APBD09.Services;

public interface IWarehouseService
{
    int AddProductToWarehouse(ProductWarehouseRequest request);
}