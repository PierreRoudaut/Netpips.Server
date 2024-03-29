﻿// <auto-generated />

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Netpips.Core.Model;

namespace Netpips.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20181001095258_added_downloadItemId_to_ShowRssItem")]
    partial class added_downloadItemId_to_ShowRssItem
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.0-rtm-30799")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Netpips.Download.Model.DownloadItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Archived");

                    b.Property<DateTime>("CanceledAt");

                    b.Property<DateTime>("CompletedAt");

                    b.Property<DateTime>("DownloadedAt");

                    b.Property<string>("FileUrl");

                    b.Property<string>("Hash");

                    b.Property<string>("Name");

                    b.Property<Guid>("OwnerId");

                    b.Property<DateTime>("StartedAt");

                    b.Property<string>("State")
                        .IsRequired();

                    b.Property<string>("Token");

                    b.Property<long>("TotalSize");

                    b.Property<string>("Type")
                        .IsRequired();

                    b.Property<string>("_movedFiles")
                        .HasColumnName("MovedFiles");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("DownloadItems");
                });

            modelBuilder.Entity("Netpips.Identity.Model.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("FamilyName");

                    b.Property<string>("GivenName");

                    b.Property<bool>("ManualDownloadEmailNotificationEnabled");

                    b.Property<string>("Picture");

                    b.Property<string>("Role")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasDefaultValue("User");

                    b.Property<bool>("TvShowSubscriptionEmailNotificationEnabled");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasFilter("[Email] IS NOT NULL");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Netpips.Subscriptions.Model.ShowRssItem", b =>
                {
                    b.Property<string>("Guid")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid?>("DownloadItemId");

                    b.Property<string>("Hash");

                    b.Property<string>("Link");

                    b.Property<DateTime>("PubDate");

                    b.Property<int>("ShowRssId");

                    b.Property<string>("Title");

                    b.Property<int>("TvMazeShowId");

                    b.Property<string>("TvShowName");

                    b.HasKey("Guid");

                    b.HasIndex("DownloadItemId");

                    b.ToTable("ShowRssItems");
                });

            modelBuilder.Entity("Netpips.Subscriptions.Model.TvShowSubscription", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ShowRssId");

                    b.Property<string>("ShowTitle");

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId", "ShowRssId")
                        .IsUnique();

                    b.ToTable("TvShowSubscriptions");
                });

            modelBuilder.Entity("Netpips.Download.Model.DownloadItem", b =>
                {
                    b.HasOne("Netpips.Identity.Model.User", "Owner")
                        .WithMany("DownloadItems")
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Netpips.Subscriptions.Model.ShowRssItem", b =>
                {
                    b.HasOne("Netpips.Download.Model.DownloadItem", "DownloadItem")
                        .WithMany()
                        .HasForeignKey("DownloadItemId");
                });

            modelBuilder.Entity("Netpips.Subscriptions.Model.TvShowSubscription", b =>
                {
                    b.HasOne("Netpips.Identity.Model.User", "User")
                        .WithMany("TvShowSubscriptions")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
