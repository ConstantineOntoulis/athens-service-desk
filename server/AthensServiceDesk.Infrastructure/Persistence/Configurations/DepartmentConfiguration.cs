using System;
using System.Collections.Generic;
using System.Text;
using AthensServiceDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AthensServiceDesk.Infrastructure.Persistence.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments");
            builder.HasKey(department => department.Id);
            builder.Property(department => department.Name)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(department => department.Description)
                .HasMaxLength(500);
            builder.Property(department => department.IsActive)
                .HasDefaultValue(true);
            builder.Property(department => department.CreatedAt)
                .IsRequired();
            builder.HasData(
                new Department
                {
                    Id = 1,
                    Name = "Citizen Services",
                    Description = "Handles citizen-facing service requests and general support.",
                    IsActive = true
                },
                new Department
                {
                    Id = 2,
                    Name = "Infrastructure",
                    Description = "Handles public infrastructure issues such as roads and lighting.",
                    IsActive = true
                },
                new Department
                {
                    Id = 3,
                    Name = "Maintenance",
                    Description = "Handles maintenance-related service requests.",
                    IsActive = true
                },
                new Department
                {
                    Id = 4,
                    Name = "Digital Services",
                    Description = "Handles digital platform and online service requests.",
                    IsActive = true
                });
        }
    }
}
