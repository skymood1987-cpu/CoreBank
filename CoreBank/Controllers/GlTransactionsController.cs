// Controllers/GlTransactionsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
using MinCoreBank.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MinCoreBank.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
   
    public class GlTransactionsController : ControllerBase
    {
        private readonly IGlTransactionRepository _repository;

        public GlTransactionsController(IGlTransactionRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("GetAlltras")]
        public async Task<ActionResult<IEnumerable<GlTransaction>>> GetAll()
        {
            var transactions = await _repository.GetAllAsync();
            return Ok(transactions);
        }

        [HttpGet("Gettrasbybranch/{branch_id}")]
        public async Task<ActionResult<IEnumerable<GlTransaction>>> Gettrasbybranch(string branch_id)
        {
            var transactions = await _repository.GettrasbybranchAsync(branch_id);
            return Ok(transactions);
        }

        [HttpGet("Gettrasbybranchdate")]
        public async Task<ActionResult<IEnumerable<GlTransaction>>> Gettrasbybranchdate(
    [FromQuery] string branch_id,
    [FromQuery] DateTime? date_)
        {
            var transactions = await _repository.GettrasbybranchdateAsync(branch_id, date_);
            return Ok(transactions);
        }

        [HttpGet("Gettrans/{id}")]
        public async Task<ActionResult<GlTransaction>> GetById(long id)
        {
            var transaction = await _repository.GetByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            return Ok(transaction);
        }

        [HttpPost("createtrans")]
        public async Task<ActionResult<GlTransactionResponseDto>> Create([FromBody] GlTransactionCreateDto dto)
        {
            // Enhanced model validation
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    Message = "Invalid request data",
                    Errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                });
            }

            try
            {
                var userId = User.Identity?.Name ?? "system";

                // Validate branch ID format
                if (string.IsNullOrEmpty(dto.BranchId) || dto.BranchId.Length != 3)
                {
                    return BadRequest("Branch ID must be exactly 3 characters");
                }

                // Validate amounts
                if (dto.DebitAccount == null || dto.DebitAccount < 0)
                    return BadRequest("DebitAccount must be a positive number");

                if (dto.CreditAccount == null || dto.CreditAccount < 0)
                    return BadRequest("CreditAccount must be a positive number");

                if (dto.Amount == null || dto.Amount < 0)
                    return BadRequest("Amount must be a positive number");

                // Auto-calculate IQD amount
                decimal? amountIqd = dto.Currency?.Equals("IQD", StringComparison.OrdinalIgnoreCase) == true
                    ? dto.Amount
                    : dto.Amount * dto.FxRate;

                // Generate temporary reference
                dto.GenerateTempReference();

                // Create transaction entity
                var transaction = new GlTransaction
                {
                    GlName = dto.GlName ?? string.Empty,
                    GlId = dto.GlId ?? string.Empty,
                    TransactionRef = dto.TransactionRef, // Temporary reference
                    Date = dto.Date ?? DateTime.UtcNow,
                    ValueDate = dto.ValueDate ?? DateTime.UtcNow,
                    DebitAccount = dto.DebitAccount,
                    CreditAccount = dto.CreditAccount,
                    Amount = dto.Amount,
                    AmountIqd = amountIqd,
                    Currency = dto.Currency?.ToUpper() ?? "IQD",
                    FxRate = dto.Currency?.Equals("IQD", StringComparison.OrdinalIgnoreCase) == true ? 1.0m : dto.FxRate,
                    CbiCode = dto.CbiCode ?? string.Empty,
                    DescriptionAr = dto.DescriptionAr ?? string.Empty,
                    DescriptionEn = dto.DescriptionEn ?? string.Empty,
                    BranchId = dto.BranchId,
                    Status = "completed",
                    CreatedBy = userId,
                    UpdatedBy = userId
                };

                // Create transaction (repository will generate final binder number)
                var created = await _repository.CreateAsync(transaction, userId);

                // Return response with final reference
                return CreatedAtAction(nameof(GetById),
                    new { id = created.Id },
                    new GlTransactionResponseDto
                    {
                        Id = created.Id,
                        TransactionRef = created.TransactionRef, // Final binder number
                        Status = created.Status,
                        Amount = created.Amount,
                        Currency = created.Currency,
                        Date = created.Date,
                        BranchId = created.BranchId,
                        CreatedAt = created.CreatedAt
                    });
            }
            catch (DbUpdateException dbEx)
            {
                // Log error
                Console.WriteLine($"Database error: {dbEx.InnerException?.Message}");
                return StatusCode(500, new
                {
                    Message = "Database operation failed",
                    Detail = dbEx.InnerException?.Message
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex}");
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred",
                    Detail = ex.Message
                });
            }
        }

        [HttpPut("Updatetrans/{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] GlTransactionUpdateDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(dto.Status))
            {
                existing.Status = dto.Status;
            }

            if (!string.IsNullOrEmpty(dto.DescriptionAr))
            {
                existing.DescriptionAr = dto.DescriptionAr;
            }

            // FIX: ACTUALLY UPDATE THE VALUES - Add these lines:
            if (dto.DebitAccount != null)
            {
                existing.DebitAccount = dto.DebitAccount.Value;
            }

            if (dto.CreditAccount != null)
            {
                existing.CreditAccount = dto.CreditAccount.Value;
            }

            if (dto.Amount != null && dto.Amount > 0)
            {
                existing.Amount = dto.Amount.Value;
            }

            // Keep your existing validation
            if (dto.DebitAccount == null || dto.DebitAccount <= 0)
            {
                return BadRequest("DebitAccount must be a positive number");
            }

            if (dto.CreditAccount == null || dto.CreditAccount <= 0)
            {
                return BadRequest("CreditAccount must be a positive number");
            }

            var userId = User.Identity.Name;
            await _repository.UpdateAsync(existing, userId);

            return NoContent();
        }

        [HttpPost("{id}/reverse")]
        public async Task<IActionResult> Reverse(long id)
        {
            var userId = "system";//User.Identity.Name;
            await _repository.ReverseAsync(id, userId);
            return NoContent();
        }




        [HttpGet("bybranchdaterange")]
        public async Task<ActionResult<IEnumerable<GlTransaction>>> GetByBranchAndDateRange(
       [FromQuery] string branchId,
       [FromQuery] string startDate,
       [FromQuery] string endDate)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(branchId))
                {
                    return BadRequest("Branch ID is required");
                }

                if (!DateTime.TryParse(startDate, out DateTime parsedStartDate))
                {
                    return BadRequest("Invalid start date format. Use YYYY-MM-DD");
                }

                if (!DateTime.TryParse(endDate, out DateTime parsedEndDate))
                {
                    return BadRequest("Invalid end date format. Use YYYY-MM-DD");
                }

                if (parsedStartDate > parsedEndDate)
                {
                    return BadRequest("Start date cannot be after end date");
                }

                // Get transactions
                var transactions = await _repository.GetByBranchAndDateRangeAsync(
                    branchId,
                    parsedStartDate,
                    parsedEndDate);

                if (!transactions.Any())
                {
                    return NotFound("No transactions found for the specified criteria");
                }

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Error fetching transactions by branch and date range");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }


}
