using BankApplication.Interfaces;
using BankApplication.Models.DTOs;
using BankApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace BankApplication.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class BankTransactionController : ControllerBase
    {
        private readonly IBankTransactionService _BankTransactionservice;
        public BankTransactionController(IBankTransactionService BankTransactionService)
        {
            _BankTransactionservice = BankTransactionService;
        }

        [HttpPost]
        public async Task<ActionResult<BankTransaction>> PostBankTransaction([FromBody] BankTransactionAddRequestDto dto)
        {
            try
            {
                var result = await _BankTransactionservice.CreateBankTransaction(dto);
                if (result == null)
                {
                    BadRequest("Unable to create BankTransaction");
                }
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("sent-BankTransactions/{sender_id}")]
        public async Task<ActionResult<ICollection<DepositResponseDto>>> GetSentBankTransactions(int sender_id)
        {
            try
            {
                var result = await _BankTransactionservice.GetSentBankTransactions(sender_id);
                if (result == null)
                {
                    return BadRequest("Unable to get sent BankTransactions");
                }
                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("received-BankTransactions/{receiver_id}")]
        public async Task<ActionResult<ICollection<ReceivedBankTransactionResponseDto>>> GetReceivedBankTransactions(int receiver_id)
        {
            try
            {
                var result = await _BankTransactionservice.GetReceivedBankTransactions(receiver_id);
                if (result == null)
                {
                    return BadRequest("Unable to get sent BankTransactions");
                }
                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("deposit-BankTransactions/{receiver_id}")]
        public async Task<ActionResult<ICollection<DepositResponseDto>>> GetDepositBankTransactions(int receiver_id)
        {
            try
            {
                var result = await _BankTransactionservice.GetDeposits(receiver_id);
                if (result == null)
                {
                    return BadRequest("Unable to get sent BankTransactions");
                }
                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("withdrawal-BankTransactions/{sender_id}")]
        public async Task<ActionResult<ICollection<WithdrawalResponseDto>>> GetWithdrawalBankTransactions(int sender_id)
        {
            try
            {
                var result = await _BankTransactionservice.GetReceivedBankTransactions(sender_id);
                if (result == null)
                {
                    return BadRequest("Unable to get sent BankTransactions");
                }
                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("get_details/{BankTransaction_id}")]
        public async Task<ActionResult<ICollection<ReceivedBankTransactionResponseDto>>> GetBankTransaction(int BankTransaction_id)
        {
            try
            {
                var result = await _BankTransactionservice.GetReceivedBankTransactions(BankTransaction_id);
                if (result == null)
                {
                    return BadRequest("Unable to get sent BankTransactions");
                }
                return Ok(result);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}