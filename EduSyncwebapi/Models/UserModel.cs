using System;
using System.Collections.Generic;

namespace EduSyncwebapi.Models;

public partial class UserModel
{
    public Guid UserId { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Role { get; set; }

    public string? PasswordHash { get; set; }

    public virtual ICollection<Coursemodel> Coursemodels { get; set; } = new List<Coursemodel>();

    public virtual ICollection<ResultModel> ResultModels { get; set; } = new List<ResultModel>();
}
