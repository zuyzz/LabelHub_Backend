using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Users;

namespace DataLabelProject.Application.DTOs.Projects
{
    public class ProjectMemberQueryParameters : UserQueryParameters
    {
        public bool? IsAvailable { get; set; }
    }
}
