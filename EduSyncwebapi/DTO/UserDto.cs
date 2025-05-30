using System;

namespace EduSyncwebapi.Dtos
{
    public class UserDto
    {
        public Guid? UserId { get; set; }  // Optional for create, filled for read

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Role { get; set; }  // "Student" or "Instructor"

        public string? Password { get; set; }  // Plain password for registration
    }

    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public Guid UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}
