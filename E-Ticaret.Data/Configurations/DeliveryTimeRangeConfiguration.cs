using E_Ticaret.Core.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace E_Ticaret.Data.Configurations
{
    public class DeliveryTimeRangeConfiguration : IEntityTypeConfiguration<DeliveryTimeRange>
    {
        public void Configure(EntityTypeBuilder<DeliveryTimeRange> builder)
        {
            builder.HasData(
                new DeliveryTimeRange
                {
                    Id = 1,
                    RangeText = "09:00 - 13:30",
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(13, 30, 0),
                    IsActive = true
                },
                new DeliveryTimeRange
                {
                    Id = 2,
                    RangeText = "12:30 - 17:00",
                    StartTime = new TimeSpan(12, 30, 0),
                    EndTime = new TimeSpan(17, 0, 0),
                    IsActive = true
                },
                new DeliveryTimeRange
                {
                    Id = 3,
                    RangeText = "13:00 - 18:00",
                    StartTime = new TimeSpan(13, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    IsActive = true
                },
                new DeliveryTimeRange
                {
                    Id = 4,
                    RangeText = "17:00 - 22:00",
                    StartTime = new TimeSpan(17, 0, 0),
                    EndTime = new TimeSpan(22, 0, 0),
                    IsActive = true
                }
            );
        }
    }
}
