using Microsoft.EntityFrameworkCore;
using RaceDay._2API.Models;
using Route = RaceDay._2API.Models.Route;


namespace RaceDay._2API.Data;

public class RaceDayDbContext : DbContext
{
    public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)
        : base(options)
    {
    }

    
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Route> Routes => Set<Route>();
    public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();
    public DbSet<Enrolment> Enrolments => Set<Enrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

       
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_User_Role",
                "[Role] IN ('Organiser','Participant')"));
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

       
        modelBuilder.Entity<Event>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_Event_EventType",
                "[EventType] IN ('Run','Walk','Cycle')"));
            e.HasOne(x => x.Organiser)
             .WithMany(u => u.Events)
             .HasForeignKey(x => x.UserID)
             .OnDelete(DeleteBehavior.Restrict);
        });

        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Event)
            .WithMany(ev => ev.Categories)
            .HasForeignKey(c => c.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Route>()
            .HasOne(r => r.Event)
            .WithMany(ev => ev.Routes)
            .HasForeignKey(r => r.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WeatherForecast>()
            .HasOne(w => w.Event)
            .WithMany(ev => ev.WeatherForecasts)
            .HasForeignKey(w => w.EventID)
            .OnDelete(DeleteBehavior.Restrict);

        
        modelBuilder.Entity<Enrolment>(e =>
        {
            e.ToTable(t => t.HasCheckConstraint("CK_Enrolment_Status",
                "[Status] IN ('Active','Cancelled')"));
            e.Property(x => x.Status).HasDefaultValue("Active");
            e.Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");

            e.HasOne(x => x.Event)
             .WithMany(ev => ev.Enrolments)
             .HasForeignKey(x => x.EventID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Participant)
             .WithMany(u => u.Enrolments)
             .HasForeignKey(x => x.ParticipantID)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Category)
             .WithMany(c => c.Enrolments)
             .HasForeignKey(x => x.CategoryID)
             .OnDelete(DeleteBehavior.Restrict);

            
            e.HasIndex(x => new { x.EventID, x.ParticipantID, x.CategoryID })
             .IsUnique();
        });

        
        modelBuilder.Entity<Result>(e =>
        {
            e.Property(x => x.CapturedAt).HasDefaultValueSql("GETDATE()");
            e.HasOne(x => x.Enrolment)
             .WithOne(en => en.Result)
             .HasForeignKey<Result>(x => x.EnrolmentID)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}