using AllocationSystem.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocationSystem.WebApi.Data
{
    public class AllocationSystemDbContext : DbContext
    {
        public AllocationSystemDbContext(DbContextOptions<AllocationSystemDbContext> options) : base(options) { }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Preference> Preferences { get; set; }
        public virtual DbSet<Supervisor> Supervisors { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Topic> Topics { get; set; }
        public virtual DbSet<Group> Groups { get; set; }
        public virtual DbSet<AdminSetting> AdminSettings { get; set; }
        public virtual DbSet<SupervisorChoice> SupervisorChoices { get; set; }
        public virtual DbSet<AllocationHistory> AllocationHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasAnnotation("Relational:Collation", "Latin1_General_CI_AS");

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(250).IsUnicode(false).IsRequired();              
                entity.Property(e => e.PasswordHash).HasMaxLength(2000).IsRequired();
                entity.Property(e => e.UserType).HasMaxLength(100);
                entity.Property(e => e.IsAdmin).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.ID)
                .HasName("PK_User_ID");

                entity.ToTable("User");
            });

            modelBuilder.Entity<Student>(entity =>
            {
                entity.Property(e => e.UserID).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(10);
                entity.Property(e => e.FirstName).HasMaxLength(250).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(250);                
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e => e.Course).HasMaxLength(500).IsRequired();
                entity.Property(e => e.AcademicYear).HasMaxLength(100);
                entity.Property(e => e.TopicID);
                entity.Property(e => e.GroupID);
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.ID)
                 .HasName("PK_Student_ID");

                entity.ToTable("Student");
            });

            modelBuilder.Entity<Supervisor>(entity =>
            {
                entity.Property(e => e.UserID).IsRequired();
                entity.Property(e => e.Title).HasMaxLength(10);
                entity.Property(e => e.FirstName).HasMaxLength(250).IsRequired();
                entity.Property(e => e.LastName).HasMaxLength(250);
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.ID)
                 .HasName("PK_Supervisor_EmployeeID");

                entity.ToTable("Supervisor");
            });

            modelBuilder.Entity<Topic>(entity =>
            {
                entity.Property(e => e.TopicID).IsRequired();
                entity.Property(e => e.TopicName).IsRequired();                
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.TopicID)
                 .HasName("PK_Topic_TopicID");

                entity.ToTable("Topic");
            });

            modelBuilder.Entity<Group>(entity =>
            {
                entity.Property(e => e.GroupID).IsRequired();
                entity.Property(e => e.GroupName).IsRequired(); 
                entity.Property(e => e.TopicID);
                entity.Property(e => e.SupervisorID);
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.GroupID)
                 .HasName("PK_Group_GroupID");

                entity.ToTable("Group");
            });

            modelBuilder.Entity<Preference>(entity =>
            {
                entity.Property(e => e.PreferenceID).IsRequired();
                entity.Property(e => e.StudentID).IsRequired();
                entity.Property(e => e.TopicID).IsRequired();
                entity.Property(e => e.PreferenceOrder).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.PreferenceID)
                .HasName("PK_Preference_PreferenceID");

                entity.ToTable("Preference");

            });

            modelBuilder.Entity<AdminSetting>(entity =>
            {
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e=>e.NoOfPreferences).IsRequired();
                entity.Property(e=>e.TeamSize).IsRequired();
                entity.Property(e => e.LastSubmissionDate).IsRequired();
                entity.Property(e => e.IsTopicMultiple).IsRequired();
                entity.Property(e => e.NoOfGroups);
                entity.Property(e => e.IsAllocationDone);
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();
                entity.ToTable("AdminSetting");
            });

            modelBuilder.Entity<SupervisorChoice>(entity =>
            {
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e => e.SupervisorID).IsRequired();
                entity.Property(e => e.TopicID).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.ID)
                .HasName("PK_SupervisorChoice_ID");

                entity.ToTable("SupervisorChoice");

            });

            modelBuilder.Entity<AllocationHistory>(entity =>
            {
                entity.Property(e => e.ID).IsRequired();
                entity.Property(e => e.ProcessEndDateTime).IsRequired();
                entity.Property(e => e.ProcessStartDateTime).IsRequired();
                entity.Property(e => e.IsSuccess).IsRequired();
                entity.Property(e => e.Error);
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasKey(b => b.ID)
                .HasName("PK_AllocationHistory_ID");

                entity.ToTable("AllocationHistory");

            });

        }
    }
}
