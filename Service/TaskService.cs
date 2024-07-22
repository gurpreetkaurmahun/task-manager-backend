using TaskManager.Models;
using TaskManager.Helpers;
using System.Threading.Tasks;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Service{

    public class TaskService{

        private readonly TaskContext _context;
        private readonly ILogger<TaskService> _logger; 

        public TaskService(TaskContext context, ILogger<TaskService> logger){
            _context = context;
            _logger = logger;
        }

        public List<TaskItem> GetAllTaskItems(){

            List<TaskItem> taskItems = null;

            try{

                _logger.LogInformationWithMethod($" Retreiving All TaskItems===>");

                // Retrieve all TaskItems including their related SubTasks.
                taskItems = _context.TaskItems.Include(task=>task.SubTasks).ToList();

                _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
                return taskItems;
            }
             catch(Exception ex){
                // Log the exception if an error occurs during retrieval
                _logger.LogErrorWithMethod($"Failed to retrieve TaskItems:{ex.Message}");
                return taskItems;
            }

        }

        public (TaskItem taskItem,string message) GetTaskItem(int id){

            TaskItem taskItem=null;
            try
            {

            //Retrieve the TaskItem including its related SubTasks using the provided ID
            taskItem =  _context.TaskItems.Include(t => t.SubTasks).FirstOrDefault(t => t.TaskItemId == id);

            if (taskItem == null)
            {
                _logger.LogErrorWithMethod($"Failed to retrieve TaskItem with id:{id}");
                return (null,$"Failed to retrieve TaskItem with id:{id}");
            }

            _logger.LogInformationWithMethod($"Successfully retrieved TaskItem with ID {id}");

             // Return the TaskItem and a success message
            return (taskItem,$"Successfully retrieved TaskItem with ID {id}");
            }
            
            catch(Exception ex){
                // Log any exceptions that occur during the retrieval process
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return  (taskItem,$"Failed with error:{ex.Message}");

            }
            
        }

        public (bool Result, string Message) UpdateTask(int id,TaskItem taskItem){
             try
             {
                // Check if the provided ID matches the TaskItem's ID
                if (id != taskItem.TaskItemId)
                 {
                     _logger.LogErrorWithMethod($"Invalid request");
                     return (false,"taskItem is null)");
                 }

                // Validate the dates in the TaskItem
                var (result,message)=ValidationHelper.ValidDates(taskItem.DateCreated,taskItem.DueDate);
                 if(!result){

                     _logger.LogErrorWithMethod(message);
                     return (false,message);
                }
                
                // Mark the TaskItem entity as modified
                 _context.Entry(taskItem).State = EntityState.Modified;
                _context.SaveChangesAsync();
                 _logger.LogInformationWithMethod($"TaskItem with id:{id} successfully updated");


                var  taskMessage = $"Changes made to  TaskItem with Id {taskItem.TaskItemId}" ;
                return (true, taskMessage);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency exceptions that occur during the update
                 if (!TaskItemExists(id))
                 {
                      _logger.LogErrorWithMethod($"Concurrency error: {ex.Message}");
                     return (false,$"Concurrency error: {ex.Message}");
                 }
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                   return (false,$"Failed with error:{ex.Message}");
             }
              catch(Exception ex){
                  _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                 return (false,$"Failed with error:{ex.Message}");

             }
        }

        public (TaskItem task ,string Message) AddTask(TaskItem taskItem){

            try{
                
                // Validate the dates in the TaskItem
                var (result,message)=ValidationHelper.ValidDates(taskItem.DateCreated,taskItem.DueDate);
                if(!result){
                    
                    _logger.LogErrorWithMethod(message);
                    return (null,$"Failed with error:{message}") ;
                }

                 // Check if the TaskItem name is valid (not empty)
                if(!ValidationHelper.ValidInput(taskItem.TaskItemName) ){
                    return (null,$"The TaskItem name cannot be left empty");
                }

                // Check if the TaskItem description is valid (not empty)
                 if(!ValidationHelper.ValidInput(taskItem.TaskItemDescription) ){
                    return (null,$"The TaskItem description cannot be left empty");
                }

                // Add the TaskItem to the database context
                _context.TaskItems.Add(taskItem);
                _context.SaveChangesAsync();

                // Log a success message indicating the TaskItem was added successfully
                _logger.LogInformationWithMethod($"TaskItem with name:{taskItem.TaskItemName} added sucsessfully");

                 // Return the added TaskItem and a success message
                return ( taskItem,$"TaskItem with name {taskItem.TaskItemName} added successfully");


            }
              catch(Exception ex){
                // Log an error if an exception occurs during the process
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                 return (null,$"Failed with error:{ex.Message}");

             }
        }

        public (bool result,string message) DeleteTask(int id){

            
            try{
                 // Retrieve the TaskItem from the database, including its associated SubTasks
                 var taskItem = _context.TaskItems.Include(t => t.SubTasks)
                .FirstOrDefault(t => t.TaskItemId == id);

                // Check if the TaskItem exists
                if (taskItem == null)
                {
                    // Log an error if the TaskItem with the specified id was not found
                    _logger.LogErrorWithMethod($"TaskItem with id:{id} not found");
                    return (false,$"TaskItem with id:{id} not found");
                }

                // Check if the TaskItem has any pending subtasks
                var(result,count)=HasSubtask(taskItem);
                if(result){
                    _logger.LogErrorWithMethod($"TaskItem with id:{id} cannot be deleted as it has {count} pending subtasks.");
                    return (false,$"TaskItem with id:{id} cannot be deleted as it has {count} pending subtasks.");
                }

                taskItem.IsCompleted=true;
                 // Remove the TaskItem from the database context
                _context.TaskItems.Remove(taskItem);
                 _context.SaveChangesAsync();

                _logger.LogInformationWithMethod($"TaskItem with id:{id} deleted sucessfully");

                return (true,$"TaskItem with id:{id} deleted sucessfully");


            }
            catch(Exception ex){
                // Log an error if an exception occurs during the deletion process
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                 return (false,$"Failed with error:{ex.Message}");

             }
        }

        public (bool pendingSubtasks,int count) HasSubtask(TaskItem taskItem){

            var subtasks=taskItem.SubTasks;
            if(subtasks==null){
                return (false,0);
            }
            int pendingCount = subtasks.Count(subtask => !subtask.IsCompleted);
            foreach(var subtask in subtasks){
                if(!subtask.IsCompleted){
                    return (true,pendingCount);
                }
            }
            return (false,0);

        }


        
        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.TaskItemId == id);
        }





    }
    

}