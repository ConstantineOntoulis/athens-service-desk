using AthensServiceDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AthensServiceDesk.Infrastructure.Persistence.Configurations;

public sealed class RequestStatusHistoryConfiguration
    : IEntityTypeConfiguration<RequestStatusHistory>
{
    public void Configure(
        EntityTypeBuilder<RequestStatusHistory> builder)
    {
        builder.ToTable("RequestStatusHistory");

        builder.HasKey(history => history.Id);

        builder.Property(history => history.NewStatus)
            .IsRequired();

        builder.Property(history => history.Note)
            .HasMaxLength(500);

        builder.Property(history => history.CreatedAt)
            .IsRequired();

        builder.HasOne(history => history.ServiceRequest)
            .WithMany(request => request.StatusHistory)
            .HasForeignKey(history => history.ServiceRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(history => history.ChangedByUser)
            .WithMany(user => user.RequestStatusChanges)
            .HasForeignKey(history => history.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(
            history => history.ChangedByUserId);

        builder.HasIndex(
            history => new
            {
                history.ServiceRequestId,
                history.CreatedAt
            });
    }
}