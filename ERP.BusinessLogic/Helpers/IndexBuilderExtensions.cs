using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public static class IndexBuilderExtensions
{
    public static IndexBuilder<TEntity> HasSoftDeleteFilter<TEntity>(
        this IndexBuilder<TEntity> indexBuilder)
        where TEntity : class, ISoftDelete
    {
        return indexBuilder.HasFilter("[IsDeleted] = 0");
    }
}