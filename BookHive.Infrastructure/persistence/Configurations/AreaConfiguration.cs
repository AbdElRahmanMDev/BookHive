using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookHive.Infrastructure.persistence.Configurations
{
    public class AreaConfiguration : IEntityTypeConfiguration<Area>
    {
        public void Configure(EntityTypeBuilder<Area> builder)
        {
            builder.HasIndex(x => new {x.GovernorateId,x.Name}).IsUnique();

            builder.Property(x => x.Name).HasMaxLength(100);
            builder.Property(x => x.CreatedOn).HasDefaultValueSql("GETDATE()");
        }
    }
}
