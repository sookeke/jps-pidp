namespace edt.service.Data.Configuration;

using edt.service.ServiceEvents.UserAccountCreation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FailedLogEventConfiguration : IEntityTypeConfiguration<FailedEventLog>
{
    public void Configure(EntityTypeBuilder<FailedEventLog> builder)
    {
        //builder.Property(e=>e.EventPayload).HasJsonConversion<>
    }
}
