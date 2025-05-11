using APBD09.Models;
using APBD09.Services;
using Microsoft.AspNetCore.Mvc;

namespace APBD09.Controller;

[ApiController]
[Route("api/warehouse")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;

    public WarehouseController(IWarehouseService warehouseService)
    {
        _warehouseService = warehouseService;
    }

    [HttpPost]
    public IActionResult AddProductToWarehouse(ProductWarehouseRequest request)
    {
        try
        {
            var id = _warehouseService.AddProductToWarehouse(request);
            return Ok(id);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
