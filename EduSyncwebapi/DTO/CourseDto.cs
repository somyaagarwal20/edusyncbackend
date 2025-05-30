using System;

namespace EduSyncwebapi.Dtos
{
    public class CourseDto
    {
        public Guid? Cousreld { get; set; }  // Nullable for create, filled for read

        public string? Title { get; set; }

        public string? Description { get; set; }

        public Guid? InstructorId { get; set; }

        public string? MediaUrl { get; set; }

       
    }
}
