using Microsoft.AspNetCore.Mvc;
using BankApplication.Interfaces;
using BankApplication.Models;
using BankApplication.Models.DTOs;

namespace BankApplication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountservice;
        public AccountController(IAccountService accountService)
        {
            _accountservice = accountService;
        }

        [HttpPost]
        public async Task<ActionResult<Account>> PostAccount([FromBody] AccountAddRequestDto dto)
        {
            try
            {
                var result = await _accountservice.CreateAccount(dto);
                if (result != null)
                {
                    return Created("", result);
                }
                return BadRequest("Unable to process request at the moment");

            } catch (Exception e) {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("user-from-account/{account_id}")]
        public async Task<ActionResult<User>> GetUser(int account_id)
        {
            try
            {
                var result = await _accountservice.GetUserFromAccount(account_id);
                if (result == null)
                {
                    return NotFound("No user found.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("balance/{account_id}")]
        public async Task<ActionResult<decimal>> GetBalance(int account_id)
        {
            try
            {
                var result = await _accountservice.GetAccountBalance(account_id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}