using TaskManager.Models;
using TaskManager.Helpers;
using System.Threading.Tasks;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Service{

    public class SubTaskService{

        private readonly TaskContext _context;
        private readonly ILogger<SubTaskService> _logger;

        public SubTaskService(TaskContext context,ILogger<SubTaskService> logger)
        {
            _context=context;
            _logger=logger;
        }

        //Method to retreive Subtask by Id
        public (SubTask subtask,string message) GetSubTaskWithId(int id){
            try{

                //Retrieve the SubTask using the provided id
                 var subTask=_context.SubTasks.FirstOrDefault(sb=>sb.SubTaskId==id);

                if(subTask==null)
                {
                    return (null,$"SubTask with id: {id} doesnot exists");
                }
                _logger.LogInformationWithMethod($"Successfully retrieved SubTask with id: {id}");

                return (subTask, $"Successfully retrieved SubTask for Task with id: {id}");

            }
            catch(Exception ex){
                // Log the exception if an error occurs during retrieval
                _logger.LogErrorWithMethod($"Failed to retrieve SubTask with id: {id}");
                return (null,$"Failed to retrieve SubTask with id: {id}");
            }
           
        }

        //Method to retreive all subtasks associated with a particular task
        public (List<SubTask> subTask,string message )GetSubTasks(int id)
        {
            var subTask=new List<SubTask>();

            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");

            try{
                // Checking if the TaskItemId (given in route parameter) exists, if it does not, an error message is returned
                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==id);

                if(!taskItem)
                {
                    _logger.LogErrorWithMethod($"TaskItem with id: {id} doesnot exists, Please recheck the task id");
                    return (subTask,$"TaskItem with id: {id} doesnot exists, Please recheck the task id");
                }
                        
                //Retreiving all subtasks that belong to TaskItem with id   
                subTask =  _context.SubTasks.Where(st=>st.TaskItemId==id).ToList();
                
                if (subTask == null)
                {
                    _logger.LogErrorWithMethod($"SubTask with id: {id} doesnot exists");
                    return (subTask,$"SubTask for Task with id: {id} doesnot exists" );
                }

                _logger.LogInformationWithMethod($"Successfully retrieved SubTasks for Task with id: {id}");
                return (subTask, $"Successfully retrieved SubTask for Task with id: {id}");

            }

            catch(Exception ex){
                // Log the exception if an error occurs during retrieval
                _logger.LogErrorWithMethod($"Failed to retreive SubTasks for Task with id: {id}");
                return (null,$"Failed to retreive SubTasks for Task with TaskId: {id}");

            }
            
        }

        public (SubTask subTask ,string Message) AddSubTask(SubTask subTask)
        {
            try{
                // Retrieving existing SubTask
                var subTaskItem=_context.TaskItems.Any(task=>task.TaskItemId==subTask.TaskItemId);

                if(!subTaskItem)
                {
                    _logger.LogErrorWithMethod($" Cannot add SubTask, Task with id:{subTask.TaskItemId} doesnot exists, Please recheck the task Id");
                    return (null,$"  Cannot add SubTask,Task with id:{subTask.TaskItemId} doesnot exists, Please recheck the task Id");
                }

                // Check if the SubTask name is valid (not empty)  
                if(!ValidationHelper.ValidInput(subTask.SubTaskName) )
                {
                    _logger.LogErrorWithMethod("The SubTask name cannot be left empty");
                    return (null,"The SubTask name cannot be left empty");
                }

                // Check if the SubTask Description is valid (not empty)
                if(!ValidationHelper.ValidInput(subTask.SubTaskDescription) )
                {
                    _logger.LogErrorWithMethod("The SubTask description cannot be left empty");
                    return (null,"The SubTask description cannot be left empty");
                    
                }

                // Validate the dates in the SubTask
                var (result,message)=ValidationHelper.ValidDates(subTask.DateCreated,subTask.DueDate);
                    
                if(!result)
                {
                    _logger.LogErrorWithMethod(message);
                    return (null,$"Failed with error:{message}") ;
                }

                // Add the SubTask to the database context
                _context.SubTasks.Add(subTask);
                _context.SaveChanges();

                // Log a success message indicating the SubTask was added successfully
                _logger.LogInformationWithMethod($"TaskItem with name:{subTask.SubTaskName} added sucsessfully");

                // Return the added SubTask and a success message
                return ( subTask,$"TaskItem with name {subTask.SubTaskName} added successfully");

            }
            catch(Exception ex){
                // Log an error if an exception occurs during the process
                _logger.LogErrorWithMethod($"Add request failed due to some internal error: {ex.Message}");
                return (null,"Add request failed due to some internal error");

            }
        }

        public (bool Result ,string Message) UpdateSubTask(int taskId,int id,SubTask subTask)
        {
            try
            {
                // Retrieving existing TaskItem with id
                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==taskId);
                if(!taskItem){

                    _logger.LogErrorWithMethod($" Cannot update SubTask, Task with id:{taskId} doesnot exists, Please recheck the task Id");
                    return (false,$"  Cannot add SubTask,Task with id:{taskId} doesnot exists, Please recheck the task Id");

                }

                // Check if the SubTask name is valid (not empty)  
                if(!ValidationHelper.ValidInput(subTask.SubTaskName)){

                    _logger.LogErrorWithMethod("The SubTask name cannot be left empty");
                    return (false,$"The SubTask name cannot be left empty");
                }

                // Check if the SubTask Description is valid (not empty)
                if(!ValidationHelper.ValidInput(subTask.SubTaskDescription)){

                    return (false,$"The SubTask description cannot be left empty");
                }

                 // Validate the dates in the TaskItem
                var(result,message)=ValidationHelper.ValidDates(subTask.DateCreated,subTask.DueDate);
                if(!result){

                    _logger.LogErrorWithMethod(message);
                    return (false,$"Failed with error:{message}") ;
                }

                // Mark the SubTask entity as modified
                _context.Entry(subTask).State = EntityState.Modified;
                _context.SaveChanges();
                _logger.LogInformationWithMethod($"SubTask with id: {subTask.SubTaskId} successfully updated");

                var  subTaskMessage = $"Changes made to  SubTask with id: {subTask.SubTaskId}" ;
                return (true, subTaskMessage);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (! SubTaskExists(id))
                {
                    _logger.LogErrorWithMethod($"SubTask does not exist in the database. Error message: {ex.Message}");
                    return (false,$"SubTask does not exist in the database.");
                }

                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                return (false,$"Update request failed due to some internal error. Try again.");
            }
           
            catch(Exception ex)
            {
                // Log an error if an exception occurs during the process
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                return (false,"Update request failed due to some internal error. Try again.");
            }
        }

        public (bool result,string message) DeleteSubTask(int taskId,int id)
        {
            try{

                // Retrieve the SubTask from the database thah belongs to the Task with id
                var subTaskItem = _context.TaskItems.Any(task => task.TaskItemId == taskId);

                // Check if the SubTask exists
                if(!subTaskItem){

                    // Log an error if the TaskItem with the specified id was not found
                    _logger.LogErrorWithMethod($" Cannot delete SubTask, Task with id: {taskId} doesnot exists, Please recheck the task Id");
                    return (false,$"  Cannot add SubTask,Task with id: {taskId} doesnot exists, Please recheck the task Id");

                }

                // Check if the SubTask exists
                var subTask = _context.SubTasks.Find(id);
                if(subTask == null)
                {
                    // Log an error if the SubTask with the specified id was not found
                    _logger.LogErrorWithMethod($"SubTask with id: {id} not found");
                    return (false,$"SubTask with id: {id} not found");
                }

                // Remove the SubTask from the database context
                _context.SubTasks.Remove(subTask);
                _context.SaveChangesAsync();

                _logger.LogInformationWithMethod($"SubTask with id: {id} deleted sucessfully");
                return (true,$"SubTask with id: {id} deleted sucessfully");

                }
                
                catch(Exception ex){

                    // Log an error if an exception occurs during the deletion process
                    _logger.LogErrorWithMethod($"Delete request failed due to some internal error: {ex.Message}");
                    return (false,$"Delete request failed due to some internal error");
                }

        }

        private bool SubTaskExists(int id)
        {
            return _context.SubTasks.Any(e => e.SubTaskId == id);
        }







    }}