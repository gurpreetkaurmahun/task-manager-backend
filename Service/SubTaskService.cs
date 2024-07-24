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
        public (SubTask subtask,string message) GetSubTaskWithId(int taskId,int id){
            try
            {
                 var subTask=_context.SubTasks.FirstOrDefault(sb=>sb.SubTaskId==id);
                if(subTask==null)
                {
                    return (null,$"SubTask with id{id} doesnot exists");
                }
                _logger.LogInformationWithMethod($"Successfully retrieved SubTask with  ID {id}");

                return (subTask, $"Successfully retrieved SubTask for Task with id: {id}");

            }
            catch(Exception ex)
            {
                string message = $"Failed to retrieve SubTask with id{id}:{ex.Message}";
                _logger.LogErrorWithMethod(message);

                return (null,$"Failed with error:{message}");

            }
           


        }

        //Method to retreive all subtasks associated with a particular task
        public (List<SubTask> subTask,string message )GetSubTasks(int id)
        {
            var subTask=new List<SubTask>();

            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");

            try
            {
                // Checking if the TaskItemId (given in route parameter) exists, if it does not, an error message is returned
                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==id);

                if(!taskItem)
                {
                    _logger.LogErrorWithMethod($"Task with id:{id} doesnot exists, Please recheck the task Id");
                    return (subTask,$"Task with id:{id} doesnot exists, Please recheck the task Id");
                }
                        
                 //Retreiving all subtasks that belong to TaskItem with id   
                subTask =  _context.SubTasks.Where(st=>st.TaskItemId==id).ToList();
                
                if (subTask == null)
                {
                    _logger.LogErrorWithMethod($"SubTask with id:{id} doesnot exists");
                    return (subTask,$"SubTask for Task with id:{id} doesnot exists" );
                }

                _logger.LogInformationWithMethod($"Successfully retrieved SubTasks for Task with TaskId {id}");

                return (subTask, $"Successfully retrieved SubTask for Task with id: {id}");

            }

            catch(Exception ex)
            {
                string message = $"Failed to retrieve TaskItems:{ex.Message}";
                _logger.LogErrorWithMethod(message);

                    
                return (null,$"Failed with error:{message}");

            }
            
            }

        public (SubTask subTask ,string Message) AddSubTask(SubTask subTask)
        {
            try
            {

                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==subTask.TaskItemId);

                if(!taskItem)
                {
                    _logger.LogErrorWithMethod($" Cannot add SubTask, Task with id:{subTask.TaskItemId} doesnot exists, Please recheck the task Id");
                    return (null,$"  Cannot add SubTask,Task with id:{subTask.TaskItemId} doesnot exists, Please recheck the task Id");
                }
                    
                if(!ValidationHelper.ValidInput(subTask.SubTaskName) )
                {
                    _logger.LogErrorWithMethod("The SubTask name cannot be left empty");
                    return (null,"The SubTask name cannot be left empty");
                }

                if(!ValidationHelper.ValidInput(subTask.SubTaskDescription) )
                {
                    _logger.LogErrorWithMethod("The SubTask description cannot be left empty");
                    return (null,"The SubTask description cannot be left empty");
                    
                }

                var (result,message)=ValidationHelper.ValidDates(subTask.DateCreated,subTask.DueDate);
                    
                if(!result)
                {
                    _logger.LogErrorWithMethod(message);
                    return (null,$"Failed with error:{message}") ;
                }

                _context.SubTasks.Add(subTask);
                _context.SaveChanges();

                _logger.LogInformationWithMethod($"TaskItem with name:{subTask.SubTaskName} added sucsessfully");
                return ( subTask,$"TaskItem with name {subTask.SubTaskName} added successfully");

            }
            catch(Exception ex)
            {
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                return (null,$"Failed with error:{ex.Message}");

             }
        }

        public (bool Result ,string Message) UpdateSubTask(int taskId,int id,SubTask subTask)
        {
            try
            {

                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==taskId);
                if(!taskItem){

                    _logger.LogErrorWithMethod($" Cannot update SubTask, Task with id:{taskId} doesnot exists, Please recheck the task Id");
                    return (false,$"  Cannot add SubTask,Task with id:{taskId} doesnot exists, Please recheck the task Id");

                }

                if(!ValidationHelper.ValidInput(subTask.SubTaskName))
                {
                    _logger.LogErrorWithMethod("The SubTask name cannot be left empty");
                    return (false,$"The SubTask name cannot be left empty");
                }

                if(!ValidationHelper.ValidInput(subTask.SubTaskDescription))
                {
                    return (false,$"The SubTask description cannot be left empty");
                }

                var(result,message)=ValidationHelper.ValidDates(subTask.DateCreated,subTask.DueDate);
                if(!result){

                    _logger.LogErrorWithMethod(message);
                    return (false,$"Failed with error:{message}") ;
                }

                _context.Entry(subTask).State = EntityState.Modified;
                _context.SaveChanges();
                _logger.LogInformationWithMethod($"SubTask with id:{subTask.SubTaskId} successfully updated");

                var  subTaskMessage = $"Changes made to  SubTask with Id {subTask.SubTaskId}" ;
                return (true, subTaskMessage);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (! SubTaskExists(id))
                {
                    _logger.LogErrorWithMethod($"Concurrency error: {ex.Message}");
                    return (false,$"Concurrency error: {ex.Message}");
                }

                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                return (false,$"Failed with error:{ex.Message}");
            }
           
            catch(Exception ex)
            {
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return (false,$"Failed with error:{ex.Message}");
            }
        }

        public (bool result,string message) DeleteTask(int taskId,int id)
        {
            try
            {

                var taskItem=_context.TaskItems.Any(task=>task.TaskItemId==taskId);
                if(!taskItem)
                {
                    _logger.LogErrorWithMethod($" Cannot delete SubTask, Task with id:{taskId} doesnot exists, Please recheck the task Id");
                    return (false,$"  Cannot add SubTask,Task with id:{taskId} doesnot exists, Please recheck the task Id");

                }

                var subTask=_context.SubTasks.Find(id);
                if(subTask==null)
                {
                    _logger.LogErrorWithMethod($"SubTask with id:{id} not found");
                    return (false,$"SubTask with id:{id} not found");
                }

                _context.SubTasks.Remove(subTask);
                _context.SaveChangesAsync();
                _logger.LogInformationWithMethod($"SubTask with id:{id} deleted sucessfully");

                return (true,$"SubTask with id:{id} deleted sucessfully");


                }catch(Exception ex)
                {
                    _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                    return (false,$"Failed with error:{ex.Message}");
                }

        }

        private bool SubTaskExists(int id)
        {
            return _context.SubTasks.Any(e => e.SubTaskId == id);
        }







    }}