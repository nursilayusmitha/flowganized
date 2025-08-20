using Microsoft.EntityFrameworkCore;
using Flowganized.Data; 
using System.Linq.Expressions;

namespace Flowganized.Utils;

public static class IdGenerator
{
    public static async Task<int> GenerateUniqueIdAsync<T>(AppDbContext context, Expression<Func<T, int>> propertySelector) where T : class
    {
        var rand = new Random();
        int newId;
        var propName = ((MemberExpression)propertySelector.Body).Member.Name;
        var exists = true;

        do
        {
            newId = rand.Next(100_000_000, 999_999_999);
            exists = await context.Set<T>().AnyAsync(e => EF.Property<int>(e, propName) == newId);
        } while (exists);

        return newId;
    }
}

