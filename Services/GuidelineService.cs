using Microsoft.EntityFrameworkCore;
using DataLabel_Project_BE.Data;
using DataLabel_Project_BE.DTOs.Guideline;
using DataLabel_Project_BE.Models;

namespace DataLabel_Project_BE.Services;

public class GuidelineService
{
    private readonly AppDbContext _context;

    public GuidelineService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get all guidelines
    /// </summary>
    public async Task<List<GuidelineResponse>> GetAllGuidelines()
    {
        return await _context.Guidelines
            .Select(g => new GuidelineResponse
            {
                GuidelineId = g.GuidelineId,
                Title = g.Title,
                Content = g.Content,
                Version = g.Version,
                CreatedAt = g.CreatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Get guideline by ID
    /// </summary>
    public async Task<GuidelineResponse?> GetGuidelineById(Guid id)
    {
        var guideline = await _context.Guidelines.FindAsync(id);
        if (guideline == null) return null;

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            Version = guideline.Version,
            CreatedAt = guideline.CreatedAt
        };
    }

    /// <summary>
    /// Create new guideline
    /// </summary>
    public async Task<GuidelineResponse> CreateGuideline(CreateGuidelineRequest request)
    {
        var guideline = new Guideline
        {
            GuidelineId = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Guidelines.Add(guideline);
        await _context.SaveChangesAsync();

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            Version = guideline.Version,
            CreatedAt = guideline.CreatedAt
        };
    }

    /// <summary>
    /// Update guideline
    /// </summary>
    public async Task<GuidelineResponse?> UpdateGuideline(Guid id, UpdateGuidelineRequest request)
    {
        var guideline = await _context.Guidelines.FindAsync(id);
        if (guideline == null) return null;

        guideline.Title = request.Title;
        guideline.Content = request.Content;
        guideline.Version++; // Increment version

        await _context.SaveChangesAsync();

        return new GuidelineResponse
        {
            GuidelineId = guideline.GuidelineId,
            Title = guideline.Title,
            Content = guideline.Content,
            Version = guideline.Version,
            CreatedAt = guideline.CreatedAt
        };
    }

    /// <summary>
    /// Delete guideline
    /// </summary>
    public async Task<(bool Success, string Message)> DeleteGuideline(Guid id)
    {
        var guideline = await _context.Guidelines.FindAsync(id);
        if (guideline == null)
            return (false, "Guideline not found");

        // Check if guideline is being used by any label sets
        var isInUse = await _context.LabelSets
            .AnyAsync(ls => ls.GuidelineId == id);

        if (isInUse)
            return (false, "Cannot delete guideline that is being used by label sets");

        _context.Guidelines.Remove(guideline);
        await _context.SaveChangesAsync();

        return (true, "Guideline deleted successfully");
    }
}
