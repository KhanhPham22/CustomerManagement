using CustomerManagement.Api.DTOs;
using CustomerManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CustomerManagement.Api.Controllers;

// Handles API requests for customer management
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    // Provides customer-related business logic for the controller
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    // Get all customers, optionally filtered by a search keyword
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll(
        [FromQuery] string? search)
    {
        var customers = await _customerService.GetCustomersAsync(search);

        return Ok(customers);
    }

    // Get a customer by ID
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);

        if (customer == null)
            return NotFound();

        return Ok(customer);
    }

    // Create a new customer
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

    // Update an existing customer by ID
    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> Update(
        int id,
        CustomerRequestDto request)
    {
        try
        {
            var updated = await _customerService.UpdateCustomerAsync(id, request);

            // Return 404 if the customer does not exist
            if (updated == null)
                return NotFound();

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            // Return 400 if the request data is invalid
            return BadRequest(new { message = ex.Message });
        }
    }

    // Delete a customer by ID
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _customerService.DeleteCustomerAsync(id);

        // Return 404 if the customer does not exist
        if (!deleted)
            return NotFound();

        // Return 204 when the customer is successfully deleted
        return NoContent();
    }
}