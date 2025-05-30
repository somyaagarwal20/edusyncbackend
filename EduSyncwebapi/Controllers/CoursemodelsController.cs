using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduSyncwebapi.Data;
using EduSyncwebapi.Models;
using EduSyncwebapi.Dtos;
using Microsoft.AspNetCore.Http;
using EduSyncwebapi.Services;

namespace EduSyncwebapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursemodelsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IBlobStorageService _blobStorageService;
        private const string ContainerName = "courses";

        public CoursemodelsController(AppDbContext context, IBlobStorageService blobStorageService)
        {
            _context = context;
            _blobStorageService = blobStorageService;
        }

        // GET: api/Coursemodels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CourseDto>>> GetCoursemodels()
        {
            var courses = await _context.Coursemodels.ToListAsync();

            var courseDtos = courses.Select(c => new CourseDto
            {
                Cousreld = c.Cousreld,
                Title = c.Title,
                Description = c.Description,
                InstructorId = c.InstructorId,
                MediaUrl = c.MediaUrl
            }).ToList();

            return Ok(courseDtos);
        }

        // GET: api/Coursemodels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CourseDto>> GetCoursemodel(Guid id)
        {
            var course = await _context.Coursemodels.FindAsync(id);

            if (course == null)
                return NotFound();

            var courseDto = new CourseDto
            {
                Cousreld = course.Cousreld,
                Title = course.Title,
                Description = course.Description,
                InstructorId = course.InstructorId,
                MediaUrl = course.MediaUrl
            };

            return Ok(courseDto);
        }

        // PUT: api/Coursemodels/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCoursemodel(Guid id, CourseDto courseDto)
        {
            if (id != courseDto.Cousreld)
                return BadRequest();

            var course = await _context.Coursemodels.FindAsync(id);
            if (course == null)
                return NotFound();

            // Update fields
            course.Title = courseDto.Title;
            course.Description = courseDto.Description;
            course.InstructorId = courseDto.InstructorId;
            course.MediaUrl = courseDto.MediaUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CoursemodelExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // POST: api/Coursemodels
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<CourseDto>> PostCoursemodel([FromForm] CourseDto courseDto, IFormFile file)
        {
            try
            {
                string mediaUrl = null;
                if (file != null)
                {
                    // Upload file to Azure Blob Storage
                    mediaUrl = await _blobStorageService.UploadFileAsync(file, ContainerName);
                }

                var course = new Coursemodel
                {
                    Cousreld = Guid.NewGuid(),
                    Title = courseDto.Title,
                    Description = courseDto.Description,
                    InstructorId = courseDto.InstructorId,
                    MediaUrl = mediaUrl
                };

                _context.Coursemodels.Add(course);
                await _context.SaveChangesAsync();

                courseDto.Cousreld = course.Cousreld;
                courseDto.MediaUrl = course.MediaUrl;

                return CreatedAtAction(nameof(GetCoursemodel), new { id = course.Cousreld }, courseDto);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Coursemodels/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoursemodel(Guid id)
        {
            var course = await _context.Coursemodels.FindAsync(id);
            if (course == null)
                return NotFound();

            // Delete associated file from blob storage if it exists
            if (!string.IsNullOrEmpty(course.MediaUrl))
            {
                try
                {
                    var fileName = course.MediaUrl.Split('/').Last();
                    await _blobStorageService.DeleteFileAsync(fileName, ContainerName);
                }
                catch (Exception ex)
                {
                    // Log the error but continue with course deletion
                    // You might want to add proper logging here
                }
            }

            _context.Coursemodels.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CoursemodelExists(Guid id)
        {
            return _context.Coursemodels.Any(e => e.Cousreld == id);
        }
    }
}
