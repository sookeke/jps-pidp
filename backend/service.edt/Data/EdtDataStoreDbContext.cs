namespace edt.service.Data;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using edt.service.ServiceEvents.UserAccountCreation.Models;

public class EdtDataStoreDbContext : DbContext
{
    private readonly IClock clock;

    public EdtDataStoreDbContext(DbContextOptions<EdtDataStoreDbContext> options, IClock clock) : base(options) => this.clock = clock;

    public DbSet<EmailLog> EmailLogs { get; set; } = default!;
    public DbSet<IdempotentConsumer> IdempotentConsumers { get; set; } = default!;
    public DbSet<NotificationAckModel> Notifications { get; set; } = default!;
    public DbSet<FailedEventLog> FailedEventLogs { get; set; } = default!;

    public override int SaveChanges()
    {
        this.ApplyAudits();

        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        this.ApplyAudits();

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("edt");
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdempotentConsumer>()
                    .ToTable("IdempotentConsumers")
                    .HasKey(x => new { x.MessageId, x.Consumer });

        modelBuilder.Entity<NotificationAckModel>()
            .ToTable("Notifications")
            .HasKey(x => new { x.NotificationId, x.EmailAddress });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EdtDataStoreDbContext).Assembly);
    }

    private void ApplyAudits()
    {
        this.ChangeTracker.DetectChanges();
        var updated = this.ChangeTracker.Entries()
            .Where(x => x.Entity is BaseAuditable
                && (x.State == EntityState.Added || x.State == EntityState.Modified));

        var currentInstant = this.clock.GetCurrentInstant();

        foreach (var entry in updated)
        {
            entry.CurrentValues[nameof(BaseAuditable.Modified)] = currentInstant;

            if (entry.State == EntityState.Added)
            {
                entry.CurrentValues[nameof(BaseAuditable.Created)] = currentInstant;
            }
            else
            {
                entry.Property(nameof(BaseAuditable.Created)).IsModified = false;
            }
        }
    }

    public async Task IdempotentConsumer(string messageId, string consumer)
    {
        await this.IdempotentConsumers.AddAsync(new IdempotentConsumer
        {
            MessageId = messageId,
            Consumer = consumer
        });
        await this.SaveChangesAsync();
    }
    public async Task<bool> HasBeenProcessed(string messageId, string consumer) => await this.IdempotentConsumers.AnyAsync(x => x.MessageId == messageId && x.Consumer == consumer);
}
