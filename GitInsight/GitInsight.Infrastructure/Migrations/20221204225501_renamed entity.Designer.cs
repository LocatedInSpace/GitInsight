﻿// <auto-generated />
using GitInsight.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GitInsight.Infrastructure.Migrations
{
    [DbContext(typeof(GitInsightContext))]
    [Migration("20221204225501_renamed entity")]
    partial class renamedentity
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.0");

            modelBuilder.Entity("GitInsight.Infrastructure.RepositoryEntry", b =>
                {
                    b.Property<string>("URI")
                        .HasColumnType("TEXT");

                    b.Property<string>("Mode")
                        .HasColumnType("TEXT");

                    b.Property<string>("Commit")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Results")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("URI", "Mode")
                        .HasName("PK_URI");

                    b.ToTable("Repositories");
                });
#pragma warning restore 612, 618
        }
    }
}