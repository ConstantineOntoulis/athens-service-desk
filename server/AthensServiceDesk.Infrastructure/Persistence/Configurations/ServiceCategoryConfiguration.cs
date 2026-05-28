using System;
using System.Collections.Generic;
using System.Text;
using AthensServiceDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AthensServiceDesk.Infrastructure.Persistence.Configurations
{
    public class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
    {
        public void Configure(EntityTypeBuilder<ServiceCategory> builder)
        {
            builder.ToTable("ServiceCategories");

            builder.HasKey(category => category.Id);

            builder.Property(category => category.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(category => category.Description)
                .HasMaxLength(500);

            builder.Property(category => category.IsActive)
                .HasDefaultValue(true);

            builder.Property(category => category.CreatedAt)
                .IsRequired();

            builder.HasOne(category => category.Department)
                .WithMany(department => department.Categories)
                .HasForeignKey(category => category.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(
                new ServiceCategory
                {
                    Id = 1,
                    Name = "General Complaint",
                    Description = "General service complaints or non-specialized requests.",
                    DepartmentId = 1,
                    IsActive = true
                },
                new ServiceCategory
                {
                    Id = 2,
                    Name = "Appointment Request",
                    Description = "Requests that require an appointment with a service department.",
                    DepartmentId = 1,
                    IsActive = true
                },
                new ServiceCategory
                {
                    Id = 3,
                    Name = "Streetlight Issue",
                    Description = "Reports about broken, flickering, or missing street lighting.",
                    DepartmentId = 2,
                    IsActive = true
                },
                new ServiceCategory
                {
                    Id = 4,
                    Name = "Road Damage",
                    Description = "Reports about potholes, damaged roads, or pavement issues.",
                    DepartmentId = 2,
                    IsActive = true
                },
                new ServiceCategory
                {
                    Id = 5,
                    Name = "Document Support",
                    Description = "Requests related to documents, certificates, or administrative support.",
                    DepartmentId = 4,
                    IsActive = true
                }
            );
        }
    }
}
