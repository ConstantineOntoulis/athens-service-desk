using AthensServiceDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AthensServiceDesk.Infrastructure.Persistence.Configurations;

public sealed class ServiceRequestConfiguration : IEntityTypeConfiguration<ServiceRequest>
{
    public void Configure(
        EntityTypeBuilder<ServiceRequest> builder)
    {
        builder.ToTable("ServiceRequests");

        builder.HasKey(request => request.Id);

        builder.Property(request => request.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(request => request.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(request => request.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(request => request.Status)
            .IsRequired();

        builder.Property(request => request.Priority)
            .IsRequired();

        builder.Property(request => request.CreatedAt)
            .IsRequired();

        builder.HasOne(request => request.Department)
            .WithMany(department => department.ServiceRequests)
            .HasForeignKey(request => request.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.ServiceCategory)
            .WithMany(category => category.ServiceRequests)
            .HasForeignKey(request => request.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.CreatedByUser)
            .WithMany(user => user.CreatedRequests)
            .HasForeignKey(request => request.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(request => request.AssignedToUser)
            .WithMany(user => user.AssignedRequests)
            .HasForeignKey(request => request.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(request => request.Status);

        builder.HasIndex(request => request.Priority);

        builder.HasIndex(request => request.CreatedAt);

        builder.HasIndex(request => request.DepartmentId);

        builder.HasIndex(request => request.ServiceCategoryId);

        builder.HasIndex(request => request.CreatedByUserId);

        builder.HasIndex(request => request.AssignedToUserId);
    }
}