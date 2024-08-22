﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TvShowsCatalog.Web.UI.Data;

#nullable disable

namespace TvShowsCatalog.Web.UI.Migrations
{
    [DbContext(typeof(ReviewContext))]
    partial class ReviewContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.8");

            modelBuilder.Entity("TvShowsCatalog.Web.Models.CoreModels.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("id");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("comment");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("creationDate");

                    b.Property<Guid>("MemberUmbracoKEy")
                        .HasColumnType("TEXT")
                        .HasColumnName("memberUmbracoKey");

                    b.Property<int>("Rating")
                        .HasColumnType("INTEGER")
                        .HasColumnName("rating");

                    b.Property<Guid>("TvShowUmbracoKey")
                        .HasColumnType("TEXT")
                        .HasColumnName("tvShowUmbracoKey");

                    b.HasKey("Id");

                    b.ToTable("review", (string)null);
                });
#pragma warning restore 612, 618
        }
    }
}
