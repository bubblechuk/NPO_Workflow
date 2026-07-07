using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NPO_Workflow.DAL.Models;
using NPO_Workflow.DAL.Services;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
namespace NPO_Workflow.DAL
{
    public class NPOContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        public DbSet<Order> Orders { get; set; }
        public DbSet<Detail> Details { get; set; }
        public DbSet<Technology> Technologies { get; set; }
        public DbSet<TechnologyOperation> TechnologyOperations { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<CalendarWeek> CalendarWeeks { get; set; }
        public DbSet<CalendarException> CalendarExceptions { get; set; }
        public NPOContext(DbContextOptions<NPOContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var utcConverter = new ValueConverter<DateTime, DateTime>(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
            );

            var nullableUtcConverter = new ValueConverter<DateTime?, DateTime?>(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
            );

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(utcConverter);
                    }
                    else if (property.ClrType == typeof(DateTime?))
                    {
                        property.SetValueConverter(nullableUtcConverter);
                    }
                }
            }
            modelBuilder.Entity<AuditLog>()
                .Property(e => e.Changes)
                .HasColumnType("jsonb");
            modelBuilder.Entity<Detail>()
                .HasOne<Order>()
                .WithMany()
                .HasForeignKey(d => d.OrderId);
            modelBuilder.Entity<Detail>()
                .HasOne<Detail>()
                .WithMany()
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Technology>()
                .HasOne<Detail>()
                .WithOne()
                .HasForeignKey<Technology>(t => t.DetailId);
            modelBuilder.Entity<TechnologyOperation>(entity =>
            {
                entity.HasOne<Technology>()
                    .WithMany()
                    .HasForeignKey(to => to.TechnologyId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<Operation>()
                    .WithMany()
                    .HasForeignKey(to => to.OperationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<CalendarWeek>().HasData(
                new CalendarWeek { Id = 1, CalendarId = 1, DayOfWeek = DayOfWeek.Monday,    WorkingHours = 8.25m },
                new CalendarWeek { Id = 2, CalendarId = 1, DayOfWeek = DayOfWeek.Tuesday,   WorkingHours = 8.25m },
                new CalendarWeek { Id = 3, CalendarId = 1, DayOfWeek = DayOfWeek.Wednesday, WorkingHours = 8.25m },
                new CalendarWeek { Id = 4, CalendarId = 1, DayOfWeek = DayOfWeek.Thursday,  WorkingHours = 8.25m },
                new CalendarWeek { Id = 5, CalendarId = 1, DayOfWeek = DayOfWeek.Friday,    WorkingHours = 7.25m },
                new CalendarWeek { Id = 6, CalendarId = 1, DayOfWeek = DayOfWeek.Saturday,  WorkingHours = 0m },
                new CalendarWeek { Id = 7, CalendarId = 1, DayOfWeek = DayOfWeek.Sunday,    WorkingHours = 0m }
            );
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var changedEntries = ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified)
        .ToList();

            var deletedOrderIds = changedEntries
                .Where(e => e.Entity is Order order && order.IsDeleted)
                .Select(e => ((Order)e.Entity).Id)
                .ToList();

            var deletedDetailIds = changedEntries
                .Where(e => e.Entity is Detail detail && detail.IsDeleted)
                .Select(e => ((Detail)e.Entity).Id)
                .ToList();

            var deletedTechIds = changedEntries
                .Where(e => e.Entity is Technology tech && tech.IsDeleted)
                .Select(e => ((Technology)e.Entity).Id)
                .ToList();

            if (deletedOrderIds.Any())
            {
                var detailIdsFromOrders = await Details
                    .Where(d => deletedOrderIds.Contains(d.OrderId) && !d.IsDeleted)
                    .Select(d => d.Id)
                    .ToListAsync(cancellationToken);

                if (detailIdsFromOrders.Any())
                {
                    await Details.Where(d => detailIdsFromOrders.Contains(d.Id)).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true), cancellationToken);
                    deletedDetailIds.AddRange(detailIdsFromOrders);
                }
            }

            if (deletedDetailIds.Any())
            {
                var techIdsFromDetails = await Technologies
                    .Where(t => deletedDetailIds.Contains(t.DetailId) && !t.IsDeleted)
                    .Select(t => t.Id)
                    .ToListAsync(cancellationToken);

                if (techIdsFromDetails.Any())
                {
                    await Technologies.Where(t => techIdsFromDetails.Contains(t.Id)).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true), cancellationToken);
                    deletedTechIds.AddRange(techIdsFromDetails); 
                }
            }
            if (deletedTechIds.Any())
            {
                await TechnologyOperations
                    .Where(to => deletedTechIds.Contains(to.TechnologyId) && !to.IsDeleted)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsDeleted, true), cancellationToken);
            }
            var auditEntries = OnBeforeSaveChanges();
            var result = await base.SaveChangesAsync(cancellationToken);
            if (auditEntries.Any())
            {
                AuditLogs.AddRange(auditEntries);
                await base.SaveChangesAsync(cancellationToken);
            }
            return result;
        }
private List<AuditLog> OnBeforeSaveChanges()
{
    ChangeTracker.DetectChanges();
    var auditEntries = new List<AuditLog>();

    foreach (var entry in ChangeTracker.Entries())
    {
        if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
            continue;

        var auditLog = new AuditLog
        {
            User = _currentUserService.UserName ?? "System",
            CorrelationId = _currentUserService.CorrelationId ?? string.Empty,
            RequestPath = _currentUserService.RequestPath ?? string.Empty,
            Tablename = entry.Entity.GetType().Name,
            Timestamp = DateTime.UtcNow,
            Action = string.Empty,
            Changes = string.Empty
        };
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            WriteIndented = false 
        };

        switch (entry.State)
        {
            case EntityState.Added:
                auditLog.Action = "Create";
                foreach (var prop in entry.CurrentValues.Properties)
                {
                    newValues[prop.Name] = entry.CurrentValues[prop.Name];
                }
                auditLog.Changes = System.Text.Json.JsonSerializer.Serialize(new { New = newValues }, options);
                break;

            case EntityState.Deleted:
                auditLog.Action = "Delete";
                foreach (var prop in entry.OriginalValues.Properties)
                {
                    oldValues[prop.Name] = entry.OriginalValues[prop.Name];
                }
                auditLog.Changes = System.Text.Json.JsonSerializer.Serialize(new { Old = oldValues }, options);
                break;

            case EntityState.Modified:
                auditLog.Action = "Update";
                foreach (var prop in entry.OriginalValues.Properties)
                {
                    var original = entry.OriginalValues[prop.Name];
                    var current = entry.CurrentValues[prop.Name];

                    if (Equals(original, current)) continue;
                    oldValues[prop.Name] = original;
                    newValues[prop.Name] = current;
                }
                auditLog.Changes = System.Text.Json.JsonSerializer.Serialize(new { Old = oldValues, New = newValues }, options);
                break;
        }

        auditEntries.Add(auditLog);
    }

    return auditEntries;
}
    }
}
