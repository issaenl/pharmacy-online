using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using pharmacyBackend.Data;
using Microsoft.EntityFrameworkCore;
using pharmacyBackend.DTO;
using pharmacyBackend.Enums;
using pharmacyBackend.Models;

namespace pharmacyBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsController : BaseController
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> CreateReview([FromBody] CreateReviewDTO dto)
        {
            var userId = GetUserId();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound("Заказ не найден.");

            if (order.Status != OrderStatus.Completed)
                return BadRequest("Отзыв можно оставить только после того, как заказ будет выполнен.");

            var newStatus = string.IsNullOrWhiteSpace(dto.Comment) ? ReviewStatus.Approved : ReviewStatus.Pending;

            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.OrderId == dto.OrderId);

            if (existingReview != null)
            {
                if (existingReview.Status != ReviewStatus.Rejected)
                {
                    return BadRequest("Вы уже оставили отзыв к этому заказу.");
                }

                bool wasApproved = existingReview.Status == ReviewStatus.Approved;

                existingReview.Rating = dto.Rating;
                existingReview.Comment = dto.Comment;
                existingReview.Status = newStatus;
                existingReview.RejectReason = null;
                existingReview.CreatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                if (wasApproved || newStatus == ReviewStatus.Approved)
                {
                    var avgRating = await _context.Reviews
                        .Where(r => r.PharmacyId == existingReview.PharmacyId && r.Status == ReviewStatus.Approved)
                        .AverageAsync(r => (double?)r.Rating);

                    var pharmacy = await _context.Pharmacies.FindAsync(existingReview.PharmacyId);
                    if (pharmacy != null)
                    {
                        pharmacy.Rating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null;
                        await _context.SaveChangesAsync();
                    }
                }

                string msg = newStatus == ReviewStatus.Approved
                    ? "Ваш отзыв обновлен и опубликован."
                    : "Ваш исправленный отзыв отправлен на модерацию.";
                return Ok(new { message = msg });
            }

            var review = new Review
            {
                UserId = userId,
                PharmacyId = order.PharmacyId,
                OrderId = dto.OrderId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                Status = newStatus
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            if (newStatus == ReviewStatus.Approved)
            {
                var avgRating = await _context.Reviews
                    .Where(r => r.PharmacyId == order.PharmacyId && r.Status == ReviewStatus.Approved)
                    .AverageAsync(r => (double?)r.Rating);

                var pharmacy = await _context.Pharmacies.FindAsync(order.PharmacyId);
                if (pharmacy != null)
                {
                    pharmacy.Rating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null;
                    await _context.SaveChangesAsync();
                }
            }

            string responseMsg = newStatus == ReviewStatus.Approved
                ? "Спасибо! Ваш отзыв опубликован."
                : "Спасибо! Ваш отзыв отправлен на модерацию.";
            return Ok(new { message = responseMsg });
        }


        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<ReviewAdminDTO>>> GetPendingReviews()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Pharmacy)
                .Where(r => r.Status == ReviewStatus.Pending)
                .OrderBy(r => r.CreatedAt)
                .Select(r => new ReviewAdminDTO
                {
                    Id = r.Id,
                    UserName = r.User.FirstName,
                    PharmacyName = r.Pharmacy.Name,
                    OrderId = r.OrderId,
                    UserId = r.UserId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();
            return Ok(reviews);
        }

        [HttpPut("{id}/moderate")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ModerateReview(int id, [FromQuery] ReviewStatus newStatus, [FromQuery] string? rejectReason = null)
        {
            var review = await _context.Reviews.Include(r => r.Pharmacy).FirstOrDefaultAsync(r => r.Id == id);

            if (review == null)
            {
                return NotFound("Отзыв не найден");
            }
            review.Status = newStatus;
            if (newStatus == ReviewStatus.Rejected)
            {
                review.RejectReason = rejectReason;
            }
            else
            {
                review.RejectReason = null;
            }

            await _context.SaveChangesAsync();

            var avgRating = await _context.Reviews
                .Where(r => r.PharmacyId == review.PharmacyId && r.Status == ReviewStatus.Approved)
                .AverageAsync(r => (double?)r.Rating);

            review.Pharmacy.Rating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Статус отзыва обновлен, рейтинг аптеки пересчитан." });
        }

        [HttpGet("my-reviews")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ReviewAdminDTO>>> GetMyReviews()
        {
            var userId = GetUserId();
            var reviews = await _context.Reviews
                .Include(r => r.Pharmacy)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewAdminDTO
                {
                    Id = r.Id,
                    PharmacyName = r.Pharmacy.Name,
                    OrderId = r.OrderId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    Status = r.Status,
                    RejectReason = r.RejectReason,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpGet("pharmacy/{pharmacyId}")]
        public async Task<ActionResult<IEnumerable<ReviewAdminDTO>>> GetPharmacyReviews(int pharmacyId)
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.PharmacyId == pharmacyId && r.Status == ReviewStatus.Approved)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewAdminDTO
                {
                    Id = r.Id,
                    UserName = r.User.FirstName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> EditReview(int id, [FromBody] EditReviewDTO dto)
        {
            var userId = GetUserId();
            var review = await _context.Reviews
                .Include(r => r.Pharmacy)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return NotFound("Отзыв не найден.");

            bool wasApproved = review.Status == ReviewStatus.Approved;
            var newStatus = string.IsNullOrWhiteSpace(dto.Comment) ? ReviewStatus.Approved : ReviewStatus.Pending;

            review.Rating = dto.Rating;
            review.Comment = dto.Comment;
            review.Status = newStatus;
            review.RejectReason = null;
            review.CreatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            if (wasApproved || newStatus == ReviewStatus.Approved)
            {
                var avgRating = await _context.Reviews
                    .Where(r => r.PharmacyId == review.PharmacyId && r.Status == ReviewStatus.Approved)
                    .AverageAsync(r => (double?)r.Rating);

                review.Pharmacy.Rating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null;
                await _context.SaveChangesAsync();
            }

            string msg = newStatus == ReviewStatus.Approved
                ? "Отзыв успешно обновлен."
                : "Отзыв изменен и отправлен на модерацию.";
            return Ok(new { message = msg });
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteReview(int id)
        {
            var userId = GetUserId();
            var review = await _context.Reviews
                .Include(r => r.Pharmacy)
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

            if (review == null)
                return NotFound("Отзыв не найден.");

            var pharmacyId = review.PharmacyId;
            bool wasApproved = review.Status == ReviewStatus.Approved;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            if (wasApproved)
            {
                var avgRating = await _context.Reviews
                    .Where(r => r.PharmacyId == pharmacyId && r.Status == ReviewStatus.Approved)
                    .AverageAsync(r => (double?)r.Rating);

                var pharmacy = await _context.Pharmacies.FindAsync(pharmacyId);
                if (pharmacy != null)
                {
                    pharmacy.Rating = avgRating.HasValue ? Math.Round(avgRating.Value, 1) : null;
                    await _context.SaveChangesAsync();
                }
            }

            return Ok(new { message = "Отзыв успешно удален." });
        }
    }
}
