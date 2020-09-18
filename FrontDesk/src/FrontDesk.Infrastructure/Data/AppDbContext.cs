﻿using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.EFCore.Extensions;
using MediatR;
using FrontDesk.Core.Aggregates;
using FrontDesk.SharedKernel;
using System;
using FrontDesk.Core.ValueObjects;

namespace FrontDesk.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IMediator _mediator;

        //public AppDbContext(DbContextOptions options) : base(options)
        //{
        //}

        public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
            : base(options)
        {
            _mediator = mediator;
        }

        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<AppointmentType> AppointmentTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Client>(x =>
            {
                x.ToTable("Clients").HasKey(k => k.Id);
            });

            modelBuilder.Entity<Schedule>(x =>
            {
                x.ToTable("Schedules").HasKey(k => k.Id);
            });

            modelBuilder.Entity<Patient>(x =>
            {
                x.ToTable("Patients").HasKey(k => k.Id);
                x.OwnsOne(p => p.AnimalType, p =>
                {
                    p.Property(pp => pp.Breed).HasColumnName("AnimalType_Breed");
                    p.Property(pp => pp.Species).HasColumnName("AnimalType_Species");
                });
            });

            modelBuilder.Entity<Doctor>(x =>
            {
                x.ToTable("Doctors").HasKey(k => k.Id);
            });

            modelBuilder.Entity<Room>(x =>
            {
                x.ToTable("Rooms").HasKey(k => k.Id);
            });

            modelBuilder.Entity<Appointment>(x =>
            {
                x.ToTable("Appointments").HasKey(k => k.Id);
            });

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // ignore events if no dispatcher provided
            if (_mediator == null) return result;

            //TODO: need to fix
            // dispatch events only if save was successful
            var entitiesWithEvents = ChangeTracker
                .Entries()
                .Select(e => e.Entity as BaseEntity<Guid>)
                .Where(e => e?.Events != null && e.Events.Any())
                .ToArray();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.Events.ToArray();
                entity.Events.Clear();
                foreach (var domainEvent in events)
                {
                    await _mediator.Publish(domainEvent).ConfigureAwait(false);
                }
            }

            return result;
        }

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        private Type GetPrimaryKeyType<TTModel>() where TTModel : class
        {
            return Model.FindEntityType(typeof(TTModel)).FindPrimaryKey().Properties
                .Select(x => x.ClrType).Single();
        }
    }
}