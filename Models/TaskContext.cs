using Microsoft.EntityFrameworkCore;

namespace TaskManager.Models{

    public class TaskContext:DbContext
    {


        public TaskContext(){}
        public TaskContext(DbContextOptions<TaskContext> options)
        :base(options){}

        public virtual DbSet<TaskItem> TaskItems{get;set;}

        public virtual  DbSet<SubTask> SubTasks{get;set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>()
                .HasMany(t => t.SubTasks)
                .WithOne(s => s.TaskItem)
                .HasForeignKey(s => s.TaskItemId);

            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.TaskItem)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.TaskItemId);
        }

       
        
    }
}