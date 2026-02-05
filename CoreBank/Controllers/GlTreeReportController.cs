
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
using MinCoreBank.Models;
using MinCoreBank.Models.Dtos;
    using MinCoreBank.Services;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
using static MinCoreBank.Models.Dtos.GlTreeDisplayDto;

    namespace MinCoreBank.Controllers
    {
        [Authorize]
        [ApiController]
        [Route("api/[controller]")]
        public class GlTreeReportController : ControllerBase
        {
            private readonly IGlTreeReportService _treeReportService;



            public GlTreeReportController(IGlTreeReportService treeReportService)
            {
                _treeReportService = treeReportService;
            }



            [HttpPost("hierarchical")]
            public async Task<ActionResult<IEnumerable<GlTreeReportDto>>> GetHierarchicalReport([FromBody] GlTreeReportRequest request)
            {
                try
                {
                    var report = await _treeReportService.GenerateTreeReportAsync(request);
                    return Ok(report);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error generating hierarchical GL tree report: {ex.Message}");
                }
            }

            [HttpPost("flat")]
            public async Task<ActionResult<IEnumerable<GlTreeDisplayDto>>> GetFlatReport([FromBody] GlTreeReportRequest request)
            {
                try
                {
                    var report = await _treeReportService.GenerateFlatTreeReportAsync(request);
                    return Ok(report);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error generating flat GL tree report: {ex.Message}");
                }
            }

            [HttpGet("branch/{branchId}/flat")]
            public async Task<ActionResult<IEnumerable<GlTreeDisplayDto>>> GetFlatReportByBranch(string branchId)
            {
                try
                {
                    var report = await _treeReportService.GetBranchTreeReportAsync(branchId);
                    return Ok(report);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error generating flat report for branch {branchId}: {ex.Message}");
                }
            }
        [HttpPost("transactions/by-gl-branch")]
        public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetTransactionsByGlIdAndBranch([FromBody] TransactionQueryRequest request)
        {
            try
            {
                var transactions = await _treeReportService.GetTransactionsByGlIdAndBranchAsync(request);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }

        [HttpPost("transactions/by-gl-date-branch")]
        public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetTransactionsByGlIdDateRangeAndBranch([FromBody] TransactionQueryRequest request)
        {
            try
            {
                var transactions = await _treeReportService.GetTransactionsByGlIdDateRangeAndBranchAsync(request);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }
        [HttpPost("transactions/by-ref-branch")]
        public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetTransactionsByRefAndBranch([FromBody] TransactionQueryRequest request)
        {
            try
            {
                var transactions = await _treeReportService.GetTransactionsByRefAndBranchAsync(request);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }

        [HttpPost("transactions/by-date-branch")]
        public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetTransactionsByDateRangeAndBranch([FromBody] TransactionQueryRequest request)
        {
            try
            {
                var transactions = await _treeReportService.GetTransactionsByDateRangeAndBranchAsync(request);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }

        [HttpPost("transactions/by-ref-date-branch")]
        public async Task<ActionResult<IEnumerable<TransactionDetailDto>>> GetTransactionsByRefDateRangeAndBranch([FromBody] TransactionQueryRequest request)
        {
            try
            {
                var transactions = await _treeReportService.GetTransactionsByRefDateRangeAndBranchAsync(request);
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving transactions: {ex.Message}");
            }
        }
        [HttpGet("bank-wide/flat")]
        public async Task<ActionResult<IEnumerable<GlTreeDisplayDto>>> GetBankWideFlatReport()
        {
            try
            {
                var report = await _treeReportService.GetBankWideTreeReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating bank-wide flat GL tree report: {ex.Message}");
            }
        }



        [HttpGet("parent/{parentGlId}")]
            public async Task<ActionResult<IEnumerable<GlTreeDisplayDto>>> GetChildrenReport(string parentGlId, [FromQuery] string branchId = null)
            {
                try
                {
                    var request = new GlTreeReportRequest
                    {
                        StartingGlId = parentGlId,
                        BranchId = branchId
                    };
                    var report = await _treeReportService.GenerateFlatTreeReportAsync(request);
                    return Ok(report);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error generating children report for parent {parentGlId}: {ex.Message}");
                }
            }

            [HttpGet("account/{glId}/details")]
            public async Task<ActionResult<GlTreeReportDto>> GetAccountDetails(string glId, [FromQuery] string branchId = null)
            {
                try
                {
                    var details = await _treeReportService.GetAccountHierarchyAsync(glId, branchId);
                    if (details == null)
                        return NotFound($"GL account {glId} not found");

                    return Ok(details);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Error retrieving details for GL {glId}: {ex.Message}");
                }
            }


        [HttpGet("branches/available")]
        public async Task<ActionResult<IEnumerable<Branch>>> GetAvailableBranches()
        {
            try
            {
                var branches = await _treeReportService.GetAvailableBranchesAsync();
                return Ok(branches);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving available branches: {ex.Message}");
            }
        }
    }
}




        
    