using HMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Infrastructure.Data.Configurations
{
    public class BaseConfigurations<TEntity, TKey> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseEntity<TKey>
    {
        public void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}
