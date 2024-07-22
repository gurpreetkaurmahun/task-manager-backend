using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using NuGet.Common;



namespace TaskManager.Models{
    public class TaskItem{
        [Key]
        public int TaskItemId { get; set; } 
        private string taskItemName;
        private string taskItemDescription;
        private DateOnly dateCreated;
        private DateOnly dueDate;
        private bool  isCompleted ;
        private ICollection <SubTask>? subTasks;

        public string TaskItemName{
            get=>taskItemName;
            set=>taskItemName=value;
        }

        public string TaskItemDescription{
            get=>taskItemDescription;
            set=>taskItemDescription=value;
        }

        public DateOnly DateCreated{
            get=>dateCreated;
            set=>dateCreated=value;
        }

        public DateOnly DueDate{
            get=>dueDate;
            set=>dueDate=value;
        }
        public bool IsCompleted{
            get=>isCompleted;
            set=>isCompleted=value;
        }
       
        public ICollection<SubTask>? SubTasks{
            get=>subTasks;
            set=>subTasks=value;
        }


  

    }
}