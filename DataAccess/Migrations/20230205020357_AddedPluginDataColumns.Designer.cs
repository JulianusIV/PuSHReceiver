﻿// <auto-generated />
using System;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DataAccess.Migrations
{
    [DbContext(typeof(DbContext))]
    [Migration("20230205020357_AddedPluginDataColumns")]
    partial class AddedPluginDataColumns
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("Models.Lease", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("Consumer")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("ConsumerData")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("HubUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<DateTime>("LastLease")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("LeaseTime")
                        .HasColumnType("int");

                    b.Property<int?>("OwnerId")
                        .HasColumnType("int");

                    b.Property<string>("Publisher")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PublisherData")
                        .IsRequired()
                        .HasColumnType("json");

                    b.Property<bool>("Subscribed")
                        .HasColumnType("tinyint(1)");

                    b.Property<string>("TopicUrl")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Leases");
                });

            modelBuilder.Entity("Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("Permissions")
                        .HasColumnType("int");

                    b.Property<string>("Salt")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Models.Lease", b =>
                {
                    b.HasOne("Models.User", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId");

                    b.Navigation("Owner");
                });
#pragma warning restore 612, 618
        }
    }
}
