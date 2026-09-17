using CustomerManagement.Api.DTOs;
using CustomerManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll(
        [FromQuery] string? search)
    {
        var customers = await _customerService.GetCustomersAsync(search);

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(
        CustomerRequestDto request)
    {
        try
        {
            var customer = await _customerService.CreateCustomerAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                customer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(
        int id,
        CustomerRequestDto request)
    {
        try
        {
            var updated = await _customerService.UpdateCustomerAsync(id, request);

            if (updated == null)
                return NotFound();

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteCustomerAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}