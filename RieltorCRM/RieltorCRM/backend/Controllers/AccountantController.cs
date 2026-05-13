using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RieltorCRM.Data;
using RieltorCRM.DTOs;
using RieltorCRM.Models;

namespace RieltorCRM.backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Accountant,Admin")]
    public class AccountantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountantController> _logger;

        public AccountantController(ApplicationDbContext context, ILogger<AccountantController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] TransactionType? type = null,
            [FromQuery] TransactionStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var query = _context.Transactions
                    .Include(t => t.Deal)
                    .Include(t => t.CreatedBy)
                    .Include(t => t.ConfirmedBy)
                    .AsQueryable();

                if (type.HasValue)
                    query = query.Where(t => t.Type == type.Value);

                if (status.HasValue)
                    query = query.Where(t => t.Status == status.Value);

                if (startDate.HasValue)
                    query = query.Where(t => t.CreatedAt >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(t => t.CreatedAt <= endDate.Value);

                var total = await query.CountAsync();
                var transactions = await query
                    .OrderByDescending(t => t.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new
                    {
                        t.Id,
                        t.TransactionNumber,
                        t.Type,
                        t.Status,
                        t.Amount,
                        t.Description,
                        t.PaymentMethod,
                        t.PaymentDate,
                        t.CreatedAt,
                        DealNumber = t.Deal!.DealNumber,
                        CreatedByName = $"{t.CreatedBy!.FirstName} {t.CreatedBy.LastName}",
                        ConfirmedByName = t.ConfirmedBy != null ? $"{t.ConfirmedBy.FirstName} {t.ConfirmedBy.LastName}" : null
                    })
                    .ToListAsync();

                return Ok(new { transactions, total, page, pageSize });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении транзакций");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPost("transactions")]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDto model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                var transaction = new Transaction
                {
                    TransactionNumber = $"TRX-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Type = model.Type,
                    Status = TransactionStatus.Pending,
                    Amount = model.Amount,
                    Description = model.Description,
                    PaymentMethod = model.PaymentMethod,
                    DealId = model.DealId,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Transactions.Add(transaction);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Транзакция создана", transactionId = transaction.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании транзакции");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("transactions/{transactionId}/confirm")]
        public async Task<IActionResult> ConfirmTransaction(int transactionId)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(transactionId);
                if (transaction == null)
                    return NotFound(new { message = "Транзакция не найдена" });

                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

                transaction.Status = TransactionStatus.Completed;
                transaction.ConfirmedById = userId;
                transaction.PaymentDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Транзакция подтверждена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при подтверждении транзакции");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpPut("transactions/{transactionId}/cancel")]
        public async Task<IActionResult> CancelTransaction(int transactionId)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(transactionId);
                if (transaction == null)
                    return NotFound(new { message = "Транзакция не найдена" });

                transaction.Status = TransactionStatus.Cancelled;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Транзакция отменена" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при отмене транзакции");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }

        [HttpGet("report")]
        public async Task<IActionResult> GetFinancialReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var transactions = await _context.Transactions
                    .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate && t.Status == TransactionStatus.Completed)
                    .ToListAsync();

                var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var commission = transactions.Where(t => t.Type == TransactionType.Commission).Sum(t => t.Amount);
                var tax = transactions.Where(t => t.Type == TransactionType.Tax).Sum(t => t.Amount);

                var byType = transactions
                    .GroupBy(t => t.Type)
                    .Select(g => new
                    {
                        Type = g.Key.ToString(),
                        Count = g.Count(),
                        Total = g.Sum(t => t.Amount)
                    })
                    .ToList();

                var byMonth = transactions
                    .GroupBy(t => new { t.PaymentDate?.Year, t.PaymentDate?.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount),
                        Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount)
                    })
                    .OrderBy(g => g.Period)
                    .ToList();

                return Ok(new
                {
                    period = new { start = startDate, end = endDate },
                    summary = new { income, expense, commission, tax, profit = income - expense },
                    byType,
                    byMonth
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при формировании финансового отчета");
                return StatusCode(500, new { message = "Ошибка сервера" });
            }
        }
    }
}