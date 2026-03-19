using System.Text.Json;
using DataLabelProject.Application.DTOs.Annotations;
using DataLabelProject.Application.DTOs.Common;
using DataLabelProject.Application.DTOs.Consensus;
using DataLabelProject.Business.Models;
using DataLabelProject.Business.Services.Shared;
using DataLabelProject.Data;
using DataLabelProject.Data.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace DataLabelProject.Business.Services.Consensus;

public class ConsensusService : IConsensusService
{
	private const double DefaultIouThreshold = 0.7;

	private readonly IConsensusRepository _consensusRepository;
	private readonly IAnnotationRepository _annotationRepository;
	private readonly ILabelingTaskItemRepository _taskRepository;
	private readonly IClusteringService _clusteringService;
	private readonly IAgreementService _agreementService;
	private readonly AppDbContext _context;

	public ConsensusService(
		IConsensusRepository consensusRepository,
		IAnnotationRepository annotationRepository,
		ILabelingTaskItemRepository taskRepository,
		IClusteringService clusteringService,
		IAgreementService agreementService,
		AppDbContext context)
	{
		_consensusRepository = consensusRepository;
		_annotationRepository = annotationRepository;
		_taskRepository = taskRepository;
		_clusteringService = clusteringService;
		_agreementService = agreementService;
		_context = context;
	}

	public async Task<ConsensusResponse> CreateConsensusAsync(Guid taskId, ConsensusCreateRequest request)
	{
		var task = await _taskRepository.GetByIdAsync(taskId)
			?? throw new KeyNotFoundException("Task not found");

		var annotations = (await _annotationRepository.GetByTaskItemIdAsync(taskId)).ToList();
		
		// Filter out skipped annotations (payload = null) - only count annotations with actual data
		var annotationsWithPayload = annotations
			.Where(a => !string.IsNullOrWhiteSpace(a.Payload))
			.ToList();
		
		var distinctAnnotatorCount = annotationsWithPayload.Select(a => a.AnnotatorId).Distinct().Count();

		var projectConfig = await _context.ProjectConfigs
			.AsNoTracking()
			.Where(pc => pc.ProjectId == task.ProjectId)
			.OrderByDescending(pc => pc.ProjectConfigId)
			.FirstOrDefaultAsync();

		var threshold = projectConfig?.AgreementThreshold ?? 0.8;
		var minimumAnnotations = projectConfig?.AnnotationsPerSample ?? 3;

		if (distinctAnnotatorCount < minimumAnnotations)
			throw new InvalidOperationException(
				$"At least {minimumAnnotations} approved annotations are required to evaluate consensus");

		var allBoxes = BoxConversionHelper.FlattenBoxes(annotationsWithPayload);
		if (allBoxes.Count == 0)
			throw new InvalidOperationException("No bounding boxes found in approved annotations");

		var clusters = _clusteringService.ClusterByIoU(allBoxes, DefaultIouThreshold);
		var calculatedScore = _agreementService.CalculateOverallScore(clusters, distinctAnnotatorCount);

		var finalScore = request.AgreementScore ?? calculatedScore;
		if (request.AgreementScore.HasValue && Math.Abs(request.AgreementScore.Value - calculatedScore) > 0.0001)
			throw new ArgumentException("Provided agreementScore does not match calculated agreement score");

		if (finalScore < threshold)
			throw new InvalidOperationException(
				$"Agreement score {finalScore:F4} is below threshold {threshold:F4}; escalate to reviewer");

		var payloadJson = request.Payload.HasValue
			? request.Payload.Value.GetRawText()
			: JsonSerializer.Serialize(new
			{
				bboxes = _agreementService.BuildConsensusBboxes(clusters),
				agreementScore = finalScore
			});

		var consensus = new Business.Models.Consensus
		{
			ConsensusId = Guid.NewGuid(),
			DatasetItemId = taskId,
			Payload = payloadJson,
			CreatedAt = DateTime.UtcNow
		};

		var created = await _consensusRepository.CreateAsync(consensus);
		return MapToDto(created);
	}

	public async Task<ConsensusResponse?> GetConsensusByIdAsync(Guid consensusId)
	{
		var consensus = await _consensusRepository.GetByIdAsync(consensusId);
		return consensus == null ? null : MapToDto(consensus);
	}

	public async Task<ConsensusResponse?> GetConsensusByTaskItemIdAsync(Guid taskItemId)
	{
		var taskItem = await _taskRepository.GetByIdAsync(taskItemId);
		if (taskItem == null)
			return null;

		var consensuses = await _consensusRepository.GetByDatasetItemIdAsync(taskItem.DatasetItemId);
		var consensus = consensuses.FirstOrDefault();

		return consensus == null ? null : MapToDto(consensus);
	}

	public async Task<PagedResponse<ConsensusResponse>> GetConsensusesAsync(ConsensusQueryParameters @params)
	{
		var paged = await _consensusRepository.GetConsensusesAsync(@params);

		return new PagedResponse<ConsensusResponse>
		{
			Items = paged.Items.Select(MapToDto).ToList(),
			TotalItems = paged.TotalItems,
			Page = @params.Page,
			PageSize = @params.PageSize,
		};
	}

	private static ConsensusResponse MapToDto(Business.Models.Consensus consensus)
	{
		object parsedPayload;
		double agreementScore = 0;
		try
		{
			var jsonDoc = JsonDocument.Parse(consensus.Payload);
			var root = jsonDoc.RootElement;
			if (root.TryGetProperty("originalPayload", out var originalElement))
			{
				parsedPayload = JsonSerializer.Deserialize<object>(originalElement.GetString() ?? "{}") ?? "{}";
			}
			else
			{
				parsedPayload = JsonSerializer.Deserialize<object>(consensus.Payload) ?? consensus.Payload;
			}
			if (root.TryGetProperty("agreementScore", out var scoreElement) && scoreElement.TryGetDouble(out var score))
			{
				agreementScore = score;
			}
		}
		catch
		{
			parsedPayload = consensus.Payload;
		}

		return new ConsensusResponse
		{
			ConsensusId = consensus.ConsensusId,
			DatasetItemId = consensus.DatasetItemId,
			Payload = parsedPayload,
			CreatedAt = consensus.CreatedAt
		};
	}
}
