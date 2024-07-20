using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManager.Models{
 
    public class SubTask{

        [Key]
        public int SubTaskId { get; set; } 
        private string subTaskName;
        private string subTaskDescription;
        private DateTime dateCreated ;
        private DateTime dueDate ;
        private bool  isCompleted ;
        private int?  taskItemId ;
        private TaskItem? taskItem;

       public string SubTaskName{
        get=>subTaskName;
        set=>subTaskName=value;
       }

       public string SubTaskDescription{
        get=>subTaskDescription;
        set=>subTaskDescription=value;
       }
       public DateTime DateCreated{
        get=>dateCreated;
        set=>dateCreated=value;
       }
        public DateTime DueDate{
            get=>dueDate;
            set=>dueDate=value;
        }
        public bool IsCompleted{
            get=>isCompleted;
            set=>isCompleted=value;
        }
         public int? TaskItemId
        {
            get => taskItemId;
            set => taskItemId = value;
        }
        public TaskItem? TaskItem
        {
            get => taskItem;
            set => taskItem = value;
        }


    }
}