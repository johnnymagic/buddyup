using Microsoft.EntityFrameworkCore;
using BuddyUp.API.Models.Domain;

namespace BuddyUp.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Sport> Sports { get; set; }
        public DbSet<UserSport> UserSports { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Verification> Verifications { get; set; }
        public DbSet<UserReport> UserReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and constraints
            
            // User - UserProfile (1:1)
            modelBuilder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.Profile)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // User - UserSport (1:N)
            modelBuilder.Entity<UserSport>()
                .HasOne(us => us.User)
                .WithMany(u => u.Sports)
                .HasForeignKey(us => us.UserId);

            // Sport - UserSport (1:N)
            modelBuilder.Entity<UserSport>()
                .HasOne(us => us.Sport)
                .WithMany(s => s.UserSports)
                .HasForeignKey(us => us.SportId);

            // Unique constraint for UserSport (user can't have the same sport twice)
            modelBuilder.Entity<UserSport>()
                .HasIndex(us => new { us.UserId, us.SportId })
                .IsUnique();

            // Match relationships
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Requester)
                .WithMany(u => u.SentMatchRequests)
                .HasForeignKey(m => m.RequesterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Match>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.ReceivedMatchRequests)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Match - Sport relationship
            modelBuilder.Entity<Match>()
                .HasOne(m => m.Sport)
                .WithMany()
                .HasForeignKey(m => m.SportId);

            // Unique constraint for Match (can't request the same person for the same sport twice)
            modelBuilder.Entity<Match>()
                .HasIndex(m => new { m.RequesterId, m.RecipientId, m.SportId })
                .IsUnique();

            // Conversation - Messages relationship
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Conversation)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ConversationId);

            // Message - User (sender) relationship
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany(u => u.SentMessages)
                .HasForeignKey(m => m.SenderId);

            // User Report relationships
            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.ReportingUser)
                .WithMany(u => u.ReportsSubmitted)
                .HasForeignKey(r => r.ReportingUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserReport>()
                .HasOne(r => r.ReportedUser)
                .WithMany(u => u.ReportsReceived)
                .HasForeignKey(r => r.ReportedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed sports
            modelBuilder.Entity<Sport>().HasData(
                new Sport { SportId = Guid.Parse("8056afb6-6f5a-4799-a69c-f8e5c2de53ad"), Name = "Tennis", Description = "A racket sport played on a rectangular court", IconUrl = "/images/sports/tennis.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("3a98a529-8ea9-46e1-8acf-1c4558fd8bf4"), Name = "Basketball", Description = "A team sport played with a ball and hoop", IconUrl = "/images/sports/basketball.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("cb5b097c-ab7b-4b28-8c7f-4fef91d8a9e7"), Name = "Running", Description = "An individual sport involving running distances", IconUrl = "/images/sports/running.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("9544f52b-8a96-457a-a481-c7b4f0dd78fd"), Name = "Cycling", Description = "A sport involving riding bicycles", IconUrl = "/images/sports/cycling.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("a0e58388-ccba-4cdf-9af2-25ee470adb68"), Name = "Swimming", Description = "A water-based sport", IconUrl = "/images/sports/swimming.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("a20ed55e-13d3-4b92-8abc-c3e7c8ae9550"), Name = "Yoga", Description = "A physical, mental and spiritual practice", IconUrl = "/images/sports/yoga.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("caeb79ef-c17e-443e-914b-b27fb9a8c1cc"), Name = "Golf", Description = "A club-and-ball sport", IconUrl = "/images/sports/golf.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("c9ea073c-f6fb-4a29-ad69-b5c2060ead7a"), Name = "Hiking", Description = "Walking in natural environments", IconUrl = "/images/sports/hiking.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("b03df04f-405a-46e4-a424-a0351f67ff98"), Name = "Soccer", Description = "A team sport played with a ball", IconUrl = "/images/sports/soccer.svg", IsActive = true },
                new Sport { SportId = Guid.Parse("8731bdcf-cda2-48d5-b92f-2cf9b4797c61"), Name = "Volleyball", Description = "A team sport played with a ball over a net", IconUrl = "/images/sports/volleyball.svg", IsActive = true }
            );
        }
    }
}