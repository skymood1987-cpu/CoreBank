using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Repositories;

namespace MinCoreBank.Controllers
{
    [Authorize(Roles = "admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class GeneralLedgerController : ControllerBase
    {
        private readonly IGeneralLedgerRepository _repository;

        public GeneralLedgerController(IGeneralLedgerRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("getAllgl")]
        public async Task<ActionResult<IEnumerable<GeneralLedgerAccount>>> GetAll()
        {
            var accounts = await _repository.GetAllAsync();
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GeneralLedgerAccount>> GetById(int id)
        {
            var account = await _repository.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        [HttpPost("creategl")]
        public async Task<ActionResult<GeneralLedgerAccount>> Create(GeneralLedgerAccountCreateDto dto)
        {

            if (await _repository.AccountIdExistsAsync(dto.Id))
            {
                return BadRequest("رقم الحساب موجود مسبقاً");
            }

            var account = new GeneralLedgerAccount
            {
               // Id = accountId,
                Id= dto.Id,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Type = dto.Type,
                Subtype = dto.Subtype,
                Currency = dto.Currency,
                BranchId = dto.BranchId,
                CustomerId = dto.CustomerId,
                Balance = dto.Balance,
                AvailableBalance = dto.AvailableBalance,
                OpeningDate = dto.OpeningDate,
                Status = dto.Status
               
            };

            var userId = User.Identity.Name; // Get current user ID
            await _repository.CreateAsync(account, userId);

            return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
        }

        [HttpPut("Updategl/{id}")]
        public async Task<IActionResult> Update(int id, GeneralLedgerAccountUpdateDto dto)
        {
            var existingAccount = await _repository.GetByIdAsync(id);
            if (existingAccount == null)
            {
                return NotFound();
            }

            existingAccount.NameAr = dto.NameAr ?? existingAccount.NameAr;
            existingAccount.NameEn = dto.NameEn ?? existingAccount.NameEn;
            existingAccount.Status = dto.Status ?? existingAccount.Status;
            existingAccount.InterestRate = dto.InterestRate ?? existingAccount.InterestRate;
            existingAccount.LastActivityDate = DateTime.UtcNow;

            var userId = User.Identity.Name; // Get current user ID
            await _repository.UpdateAsync(existingAccount, userId);

            return NoContent();
        }

        [HttpPut("deletegl/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var account = await _repository.GetByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            var userId = User.Identity.Name;
            await _repository.SoftDeleteAsync(id, userId);
            return NoContent();
        }

        private string GenerateAccountId()
        {
            // Implement your account ID generation logic
           return Guid.NewGuid().ToString().Substring(0, 10).ToUpper();
        }
    }
}