using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DataLabelProject.Business.Services.Projects;
using DataLabelProject.Business.Services.Tasks;
using DataLabelProject.Business.Services.Guidelines;
using DataLabelProject.Application.DTOs.Projects;
using DataLabelProject.Application.DTOs.Users;
using DataLabelProject.Business.Services.Labels;
using DataLabelProject.Application.DTOs.Labels;
using DataLabelProject.Business.Services.Datasets;
using DataLabelProject.Application.DTOs.Datasets;

namespace DataLabelProject.Application.Controllers;

[ApiController]
[Route("api/projects")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IProjectMemberService _projectMemberService;
    private readonly IDatasetService _datasetService;
    private readonly ILabelService _labelService;
    private readonly IGuidelineService _guidelineService;
    private readonly ILabelingTaskService _taskService;

    public ProjectsController(
        IProjectService projectService,
        IProjectMemberService projectMemberService,
        IDatasetService datasetService,
        ILabelService labelService,
        IGuidelineService guidelineService,
        ILabelingTaskService taskService)
    {
        _projectService = projectService;
        _projectMemberService = projectMemberService;
        _datasetService = datasetService;
        _labelService = labelService;
        _guidelineService = guidelineService;
        _taskService = taskService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProjects(
        [FromQuery] ProjectQueryParameters @params)
    {
        var projects = await _projectService.GetProjects(@params);
        return Ok(projects);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetProject(
        [FromRoute] Guid id)
    {
        var project = await _projectService.GetProjectById(id);
        if (project == null) return NotFound();
        return Ok(project);
    }

    /// <summary>
    /// Get all active members of a project
    /// </summary>
    [HttpGet("{id}/members")]
    [Authorize]
    public async Task<IActionResult> GetMembers(
        [FromRoute] Guid id, 
        [FromQuery] UserQueryParameters @params)
    {
        var result = _projectMemberService.GetUserFromProject(id, @params);
        if (result == null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// Add member to project
    /// </summary>
    [HttpPost("{id}/members/{memberId}")]
    [Authorize]
    public async Task<IActionResult> AddMember(
        [FromRoute] Guid id, 
        [FromRoute] Guid memberId)
    {
        await _projectMemberService.AddUserToProject(memberId, id);
        return NoContent();
    }

    /// <summary>
    /// Remove member from project
    /// </summary>
    [HttpDelete("{id}/members/{memberId}")]
    [Authorize(Roles = "manager")]
    public async Task<IActionResult> RemoveMember(
        [FromRoute] Guid id, 
        [FromRoute] Guid memberId)
    {
        await _projectMemberService.RemoveUserFromProject(memberId, id);
        return NoContent();
    }

    [HttpPost]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest request)
    {
        var created = await _projectService.CreateProject(request);
        return CreatedAtAction(nameof(GetProject), new { id = created.ProjectId }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> UpdateProject(
        [FromRoute] Guid id, 
        [FromBody] UpdateProjectRequest request)
    {
        var updated = await _projectService.UpdateProject(id, request);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProject(
        [FromRoute] Guid id)
    {
        await _projectService.DeleteProject(id);
        return NoContent();
    }
    
    /// <summary>
    /// Get all labels for a project
    /// </summary>
    [HttpGet("{id}/labels")]
    [Authorize]
    public async Task<IActionResult> GetProjectLabels(
        [FromRoute] Guid id, 
        [FromQuery] LabelQueryParameters @params)
    {
        var result = _labelService.GetProjectLabels(id, @params);
        return Ok(result);
    }

    /// <summary>
    /// Get all datasets for a project
    /// </summary>
    [HttpGet("{id}/datasets")]
    [Authorize]
    public async Task<IActionResult> GetProjectDatasets(
        [FromRoute] Guid id, 
        [FromQuery] DatasetQueryParameters @params)
    {
        var result = _datasetService.GetProjectDatasets(id, @params);
        return Ok(result);
    }

    /// <summary>
    /// Get guideline for a project
    /// </summary>
    [HttpGet("{id}/guideline")]
    [Authorize]
    public async Task<IActionResult> GetProjectGuideline(
        [FromRoute] Guid id)
    {
        var result = _guidelineService.GetProjectGuideline(id);
        return Ok(result);
    }

    /// <summary>
    /// Get all task items for a project
    /// </summary>
    [HttpGet("{id}/task-items")]
    [Authorize]
    public async Task<IActionResult> GetProjectTaskItems([FromRoute] Guid id)
    {
        var taskItems = await _taskService.GetTaskItemsByProjectIdAsync(id);
        var response = taskItems.Select(i => new DataLabelProject.Application.DTOs.Tasks.TaskItemResponse
        {
            TaskItemId = i.TaskItemId,
            DatasetItemId = i.DatasetItemId,
            TaskId = i.TaskId,
            RevisionCount = i.RevisionCount,
            Status = i.Status
        }).ToList();

        return Ok(response);
    }
}
