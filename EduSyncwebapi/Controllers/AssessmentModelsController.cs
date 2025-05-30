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
    public class AssessmentModelsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AssessmentModelsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/AssessmentModels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AssessmentDto>>> GetAssessmentModels()
        {
            var assessments = await _context.AssessmentModels.ToListAsync();

            var assessmentDtos = assessments.Select(a => new AssessmentDto
            {
                AssessmentId = a.AssessmentId,
                CourseId = a.CourseId,
                Title = a.Title,
                Questions = a.Questions,
                MaxScore = a.MaxScore
            }).ToList();

            return Ok(assessmentDtos);
        }

        // GET: api/AssessmentModels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AssessmentDto>> GetAssessmentModel(Guid id)
        {
            var assessment = await _context.AssessmentModels.FindAsync(id);

            if (assessment == null)
                return NotFound();

            var assessmentDto = new AssessmentDto
            {
                AssessmentId = assessment.AssessmentId,
                CourseId = assessment.CourseId,
                Title = assessment.Title,
                Questions = assessment.Questions,
                MaxScore = assessment.MaxScore
            };

            return Ok(assessmentDto);
        }

        // PUT: api/AssessmentModels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAssessmentModel(Guid id, AssessmentDto assessmentDto)
        {
            if (id != assessmentDto.AssessmentId)
                return BadRequest();

            var assessment = await _context.AssessmentModels.FindAsync(id);
            if (assessment == null)
                return NotFound();

            assessment.CourseId = assessmentDto.CourseId;
            assessment.Title = assessmentDto.Title;
            assessment.Questions = assessmentDto.Questions;
            assessment.MaxScore = assessmentDto.MaxScore;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AssessmentModelExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/AssessmentModels
        [HttpPost]
        public async Task<ActionResult<AssessmentDto>> PostAssessmentModel(AssessmentDto assessmentDto)
        {
            var assessment = new AssessmentModel
            {
                AssessmentId = Guid.NewGuid(),
                CourseId = assessmentDto.CourseId,
                Title = assessmentDto.Title,
                Questions = assessmentDto.Questions,
                MaxScore = assessmentDto.MaxScore
            };

            _context.AssessmentModels.Add(assessment);
            await _context.SaveChangesAsync();

            assessmentDto.AssessmentId = assessment.AssessmentId;

            return CreatedAtAction(nameof(GetAssessmentModel), new { id = assessment.AssessmentId }, assessmentDto);
        }

        // DELETE: api/AssessmentModels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAssessmentModel(Guid id)
        {
            var assessment = await _context.AssessmentModels.FindAsync(id);
            if (assessment == null)
                return NotFound();

            _context.AssessmentModels.Remove(assessment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AssessmentModelExists(Guid id)
        {
            return _context.AssessmentModels.Any(e => e.AssessmentId == id);
        }
    }
}
