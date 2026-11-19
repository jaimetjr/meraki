using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace API.Common;

public static class QueryableExtensions
{
    /// <summary>
    /// Loads every entity with the given ids. Any id that does not exist (or is soft-deleted,
    /// thanks to the global query filters) is a client error, reported with the missing ids.
    /// </summary>
    public static async Task<List<T>> LoadAllByIdAsync<T>(this IQueryable<T> source, IEnumerable<Guid>? ids, string label)
        where T : Entity
    {
        var wanted = (ids ?? []).Distinct().ToList();
        if (wanted.Count == 0) return [];

        var found = await source.Where(e => wanted.Contains(e.Id)).ToListAsync();
        var missing = wanted.Except(found.Select(e => e.Id)).ToList();
        if (missing.Count > 0)
            throw new DomainException($"Unknown or deleted {label}: {string.Join(", ", missing)}");

        return found;
    }
}
