using TaskManager.Models;
using TaskManager.Helpers;
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
                _logger.LogInformationWithMethod($"Retreiving all TaskItems from the DB");

                // Retrieve all TaskItems including their related SubTasks.
                taskItems = _context.TaskItems.Include(task=>task.SubTasks).ToList();

                _logger.LogInformationWithMethod("Sucessfully retreived all TaskItems");
                return taskItems;
            }
             catch(Exception ex){
                // Log the exception if an error occurs during retrieval
                _logger.LogErrorWithMethod($"Failed to retrieve TaskItems from the DB with exception message: {ex.Message}");
                return taskItems;
            }

        }

        public (TaskItem taskItem,string message) GetTaskItem(int id){

            TaskItem taskItem = null;
            try{

            //Retrieve the TaskItem including its related SubTasks using the provided ID
            taskItem = _context.TaskItems.Include(t => t.SubTasks).FirstOrDefault(t => t.TaskItemId == id);

            if (taskItem == null)
            {
                _logger.LogErrorWithMethod($"Failed to retrieve TaskItem with id: {id}");
                return (null, $"Failed to retrieve TaskItem with id: {id}");
            }

            _logger.LogInformationWithMethod($"Successfully retrieved TaskItem with id: {id}");

             // Return the TaskItem and a success message
            return (taskItem, $"Successfully retrieved TaskItem with id: {id}");
            }
            
            catch(Exception ex){
                // Log any exceptions that occur during the retrieval process
                _logger.LogErrorWithMethod($"Failed to retrieve task item with id: {id}. Got exception message: {ex.Message}");
                return (taskItem, $"Failed to retrieve task item with id:{id}");
            }
        
        }

        public (bool Result, string Message) UpdateTask(int id, TaskItem taskItem){
             try{
                // Retrieving existing TaskItem
                var existingTaskItem = _context.TaskItems.Find(id);
                if (existingTaskItem == null){
                    _logger.LogErrorWithMethod($"TaskItem with id: {id} does not exist");
                    return (false, $"TaskItem with id: {id} does not exist");
                }

                // Handle "mark as complete" request
                if (taskItem.IsCompleted){
                    if (!CheckIfSubTaskAreComplete(id)){
                        return (false,"Cannot mark the task as complete as one or more subtasks are incomplete");
                    } else {
                         existingTaskItem.IsCompleted = taskItem.IsCompleted;
                        _context.SaveChanges();
                        _logger.LogInformationWithMethod($"TaskItem with id: {id} successfully marked as complete");
                        return (true, $"TaskItem with id: {id} successfully marked as complete");
                    }
                } 
        
                // Handle other update requests with validation checks
                // Check if the provided ID matches the TaskItem's ID
                if (id != taskItem.TaskItemId){
                     _logger.LogErrorWithMethod("Invalid request as provided id does not match with TaskItemId");
                     return (false,"Invalid request as provided id does not match with TaskItemId");
                 }

                // Validate the dates in the TaskItem
                var (result,message)=ValidationHelper.ValidDates(taskItem.DateCreated,taskItem.DueDate);
                 if(!result){
                     _logger.LogErrorWithMethod(message);
                     return (false, message);
                }
                
                // Mark the TaskItem entity as modified
                existingTaskItem.DateCreated = taskItem.DateCreated;
                existingTaskItem.DueDate = taskItem.DueDate;
                existingTaskItem.TaskItemName = taskItem.TaskItemName;
                existingTaskItem.TaskItemDescription = taskItem.TaskItemDescription;
                 _context.SaveChanges();
                 _logger.LogInformationWithMethod($"TaskItem with id:{id} successfully updated");


                string taskMessage = $"Changes made to task item with id: {taskItem.TaskItemId}" ;
                return (true, taskMessage);
            }
            catch (DbUpdateConcurrencyException ex){
                 if (!TaskItemExists(id)){
                    _logger.LogErrorWithMethod($"Task item does not exist in the database. Error message: {ex.Message}");
                    return (false, $"Task item does not exist in the database");
                 }

                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                return (false, $"Update request failed due to some internal error. Try again.");
             }
              catch(Exception ex){
                  _logger.LogErrorWithMethod($"Failed with error: {ex.Message}");

                 return (false, "Update request failed due to some internal error. Try again.");
             }
        }


        public (TaskItem task ,string Message) AddTask(TaskItem taskItem){

            try{
                // Validate the dates in the TaskItem
                var (result, message)=ValidationHelper.ValidDates(taskItem.DateCreated,taskItem.DueDate);
                if(!result){
                    _logger.LogErrorWithMethod(message);
                    return (null, $"Date validation failed. Error message: {message}") ;
                }

                 // Check if the TaskItem name is valid (not empty)
                if(!ValidationHelper.ValidInput(taskItem.TaskItemName) ){
                    return (null, "Validation failed.The TaskItem name cannot be empty.");
                }

                // Check if the TaskItem description is valid (not empty)
                 if(!ValidationHelper.ValidInput(taskItem.TaskItemDescription) ){
                    return (null, "Validation failed. TaskItem description cannot be left empty.");
                }

                // Add the TaskItem to the database context
                _context.TaskItems.Add(taskItem);
                _context.SaveChanges();

                // Log a success message indicating the TaskItem was added successfully
                _logger.LogInformationWithMethod($"TaskItem with name: {taskItem.TaskItemName} added sucsessfully");

                 // Return the added TaskItem and a success message
                return ( taskItem,$"TaskItem with name {taskItem.TaskItemName} added successfully");

            }
              catch(Exception ex){
                // Log an error if an exception occurs during the process
                _logger.LogErrorWithMethod($"Add request failed due to some internal error: {ex.Message}");
                return (null, "Add request failed due to some internal error");
             }
        }

        public (bool result,string message) DeleteTask(int id){

            try{

                 // Retrieve the TaskItem from the database, including its associated SubTasks
                var taskItem = _context.TaskItems.Include(t => t.SubTasks)
                .FirstOrDefault(t => t.TaskItemId == id);

                // Check if the TaskItem exists
                if (taskItem == null){
                    // Log an error if the TaskItem with the specified id was not found
                    _logger.LogErrorWithMethod($"TaskItem with id: {id} not found");
                    return (false, $"TaskItem with id: {id} not found");
                }

                if(!taskItem.IsCompleted){
                    _logger.LogErrorWithMethod($"TaskItem with id: {id} cannot be deleted as it has one or more pending subtasks.");
                    return (false, $"TaskItem with id: {id} cannot be deleted as it has one or more pending subtasks.");
                }

                // Remove the TaskItem from the database context as TaskItem is completed
                _context.TaskItems.Remove(taskItem);
                _context.SaveChanges();

                _logger.LogInformationWithMethod($"TaskItem with id: {id} deleted sucessfully");

                return (true,$"TaskItem with id: {id} deleted sucessfully");
            }

            catch(Exception ex){
                // Log an error if an exception occurs during the deletion process
                _logger.LogErrorWithMethod($"Delete request failed due to some internal error: {ex.Message}");
                 return (false, $"Delete request failed due to some internal error");
             }
        }

    
        public bool CheckIfSubTaskAreComplete(int id){

            TaskItem taskItem = _context.TaskItems.Include(t => t.SubTasks).FirstOrDefault(t => t.TaskItemId == id);
            var subTasks = taskItem.SubTasks;

            if (subTasks.Count == 0){
                return true;
            } 
            
            foreach(SubTask subTask in subTasks){
                if (subTask.IsCompleted == false){
                        return false;
                }
            }
             
            // All subtasks are complete
            return true;
        }


        private bool TaskItemExists(int id){
            return _context.TaskItems.Any(e => e.TaskItemId == id);
        }

    }

}