/*using ExpenseTrackingSystem.Interfaces;
using ExpenseTrackingSystem.Misc;
using ExpenseTrackingSystem.Models;
using ExpenseTrackingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ExpenseTrackingSystem.Controllers
{
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/receipts")]
    public class ReceiptController : ControllerBase
    {
        private readonly IReceiptService _receiptService;

        public ReceiptController(IReceiptService receiptService)
        {
            _receiptService = receiptService;
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<Receipt>> Create([FromForm] ReceiptAddRequestDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                if (!User.IsInRole("Admin") && dto.Username != username)
                    return Unauthorized("You are not authorized to create receipts for other users.");

                var result = await _receiptService.CreateReceipt(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<ActionResult<Receipt>> Update(Guid id, [FromForm] ReceiptUpdateRequestDto dto)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to update this receipt.");

                var result = await _receiptService.UpdateReceipt(dto, id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<ActionResult<Receipt>> Delete(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to delete this receipt.");

                var result = await _receiptService.DeleteReceipt(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get/{id}")]
        public async Task<ActionResult<Receipt>> Get(Guid id)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var receipt = await _receiptService.GetReceipt(id);
                if (receipt == null)
                    return NotFound("Receipt not found.");

                if (!User.IsInRole("Admin") && receipt.Username != username)
                    return Unauthorized("You are not authorized to view this receipt.");

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("search")]
        public async Task<ActionResult<ICollection<Receipt>>?> Search([FromBody] ReceiptSearchModel model)
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                    return Unauthorized("Username not found in token.");

                var result = await _receiptService.SearchReceipts(model);
                if (!User.IsInRole("Admin"))
                    result = result?.Where(r => r.Username == username).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("all")]
        public async Task<ActionResult<ICollection<Receipt>>> GetAllReceipts(
            [FromQuery] string? username,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var requester = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(requester))
                    return Unauthorized("Username not found in token.");

                if (!string.IsNullOrWhiteSpace(username) && username != requester && !User.IsInRole("Admin"))
                    return Unauthorized("You are not authorized to access other users' receipts.");

                var userToQuery = string.IsNullOrWhiteSpace(username) ? requester : username;

                var receipts = await _receiptService.SearchReceipts(new ReceiptSearchModel());
                var filtered = receipts?
                                .Where(r => r.Username == userToQuery)
                                .OrderByDescending(r => r.CreatedAt)
                                .Paginate(pageNumber, pageSize)
                                .ToList();

                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



    }
}
*/