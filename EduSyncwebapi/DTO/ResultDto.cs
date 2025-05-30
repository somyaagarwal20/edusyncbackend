using System;

namespace EduSyncwebapi.Dtos
{
    public class ResultDto
    {
        public Guid? ResultId { get; set; }  // Nullable for create

        public Guid? AssessmentId { get; set; }

        public Guid? UserId { get; set; }

        public int? Score { get; set; }

        public DateTime? AttemptDate { get; set; }

    }
}
