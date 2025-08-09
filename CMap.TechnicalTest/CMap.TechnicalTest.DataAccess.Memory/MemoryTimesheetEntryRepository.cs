using System.Collections.Concurrent;
using CMap.TechnicalTest.DataAccess.Interfaces;
using CMap.TechnicalTest.Models;

namespace CMap.TechnicalTest.DataAccess.Memory;

public class MemoryTimesheetEntryRepository : ITimesheetEntryRepository
{
    private readonly ConcurrentDictionary<Guid, TimesheetEntry> _entries = new();
    
    public TimesheetEntry? GetTimesheetEntryById(Guid id)
    {
        if(id == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(id), id, $"{nameof(id)} cannot be empty");
        
        return _entries.GetValueOrDefault(id);
    }

    public IEnumerable<TimesheetEntry> GetTimesheetEntriesByUserId(Guid userId, DateTime? startDate, DateTime? endDate)
    {
        if(userId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(userId), userId, $"{nameof(userId)} cannot be empty");
        
        // Locking query
        return _entries.Values.Where(e =>
            e.UserId == userId
            && (!startDate.HasValue || startDate.Value <= e.Date)
            && (!endDate.HasValue || endDate.Value >= e.Date));
    }

    public IEnumerable<TimesheetEntry> GetTimesheetEntriesByUserIdAndProjectId(Guid userId, Guid projectId, DateTime? startDate, DateTime? endDate)
    {
        if(userId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(userId), userId, $"{nameof(userId)} cannot be empty");
        
        if(projectId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(projectId), projectId, $"{nameof(projectId)} cannot be empty");
        
        // Locking query
        return _entries.Values.Where(e =>
            e.UserId == userId
            && e.ProjectId == projectId
            && (!startDate.HasValue || startDate.Value <= e.Date)
            && (!endDate.HasValue || endDate.Value >= e.Date));
    }

    public TimesheetEntry CreateTimesheetEntry(TimesheetEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (entry.Id == Guid.Empty)
            entry.Id = Guid.NewGuid();
        
        if(_entries.TryAdd(entry.Id, entry))
            return entry;
        
        throw new InvalidOperationException($"An entry already exists with Id {entry.Id}");
    }

    public TimesheetEntry UpdateTimesheetEntry(TimesheetEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if(entry.Id == Guid.Empty)
            throw new ArgumentOutOfRangeException("entry.Id", entry.Id, $"{nameof(entry.Id)} cannot be empty");

        if (!_entries.ContainsKey(entry.Id))
            throw new KeyNotFoundException();
            
        // Entry may have been removed since the check above - presume it is not an issue to add back, small edge case
        // Possible to use TryUpdate here if an ETag or similar was used to ensure the result from GetTimesheetEntryById had the same ETag
        // This would remove the edge case and provide consistency checking, but is likely over-engineering for this example
        _entries.AddOrUpdate(entry.Id, entry, (k, v) => entry);
        
        return entry;
    }

    public void DeleteTimesheetEntry(Guid entryId)
    {
        if(entryId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(entryId), entryId, $"{nameof(entryId)} cannot be empty");
        
        if (!_entries.TryRemove(entryId, out _))
            throw new KeyNotFoundException();
    }
}