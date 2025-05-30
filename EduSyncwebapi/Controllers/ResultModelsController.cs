using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncwebapi.Data;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;

namespace EduSyncwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ResultModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/ResultModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResultDto>>> GetResultModels()
        {
            var results = await _context.ResultModels.ToListAsync();

            var resultDtos = results.Select(r => new ResultDto
            {
                ResultId = r.ResultId,
                AssessmentId = r.AssessmentId,
                UserId = r.UserId,
                Score = r.Score,
                AttemptDate = r.AttemptDate
            }).ToList();

            return Ok(resultDtos);
        }

        // GET: api/ResultModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ResultDto>> GetResultModel(Guid id)
        {
            var result = await _context.ResultModels.FindAsync(id);

            if (result == null)
                return NotFound();

            var resultDto = new ResultDto
            {
                ResultId = result.ResultId,
                AssessmentId = result.AssessmentId,
                UserId = result.UserId,
                Score = result.Score,
                AttemptDate = result.AttemptDate
            };

            return Ok(resultDto);
        }

        // PUT: api/ResultModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutResultModel(Guid id, ResultDto resultDto)
        {
            if (id != resultDto.ResultId)
                return BadRequest();

            var result = await _context.ResultModels.FindAsync(id);
            if (result == null)
                return NotFound();

            result.AssessmentId = resultDto.AssessmentId;
            result.UserId = resultDto.UserId;
            result.Score = resultDto.Score;
            result.AttemptDate = resultDto.AttemptDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResultModelExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/ResultModels
        [HttpPost]
        public async Task<ActionResult<ResultDto>> PostResultModel(ResultDto resultDto)
        {
            var result = new ResultModel
            {
                ResultId = Guid.NewGuid(),
                AssessmentId = resultDto.AssessmentId,
                UserId = resultDto.UserId,
                Score = resultDto.Score,
                AttemptDate = resultDto.AttemptDate
            };

            _context.ResultModels.Add(result);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ResultModelExists(result.ResultId))
                    return Conflict();
                else
                    throw;
            }

            resultDto.ResultId = result.ResultId;
            return CreatedAtAction(nameof(GetResultModel), new { id = result.ResultId }, resultDto);
        }

        // DELETE: api/ResultModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResultModel(Guid id)
        {
            var result = await _context.ResultModels.FindAsync(id);
            if (result == null)
                return NotFound();

            _context.ResultModels.Remove(result);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ResultModelExists(Guid id)
        {
            return _context.ResultModels.Any(e => e.ResultId == id);
        }
    }
}
