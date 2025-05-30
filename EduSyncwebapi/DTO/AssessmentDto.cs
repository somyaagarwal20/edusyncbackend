using System;

namespace EduSyncwebapi.Dtos
{
    public class AssessmentDto
    {
        public Guid? AssessmentId { get; set; }  // Nullable for create

        public Guid? CourseId { get; set; }

        public string? Title { get; set; }

        public string? Questions { get; set; }  // JSON format string of quiz questions

        public int? MaxScore { get; set; }

   
    }
}
