
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.Helpers;
using TaskManager.Service;

namespace TaskManager.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        // private readonly TaskContext _context;
        private readonly TaskService _service;
        private readonly ILogger<TaskItemsController> _logger; 
        public TaskItemsController(TaskService service, ILogger<TaskItemsController>logger)
        {
            _service = service;
            _logger=logger;
        }

        // Retrieves all TaskItems
        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
        {
            
            var taskItems = _service.GetAllTaskItems();

            if (taskItems == null){
                return StatusCode(500, $"Could not retrieve any task items");
            } else {
                _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
                return Ok(taskItems);
            }
            
        }

        // Retrieves a specific TaskItem by its ID
        // GET: api/tasks/5
       [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskItem(int id)
        {
            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");

            // Retrieve the task item and a message from the Taskservice
            var (taskItem,message)=_service.GetTaskItem(id);
             if (taskItem == null){
                return StatusCode(500, new{message});
            } else {
                _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
                return Ok(new{taskItem,message});
            }
            
        }

        // Updates a specific TaskItem by its ID
        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskItem(int id, TaskItem taskItem)
        {
            // Retrieve the task item and a message from the Taskservice
            var (result,message)=_service.UpdateTask(id,taskItem);
            if(result){
                return Ok(new{message});
            }
            else{
                return BadRequest(new{message});
            }
        }

        // Adds a new TaskItem
        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItem>> PostTaskItem(TaskItem taskItem)
        {

            // Retrieve the task item and a message from the Taskservice
            var (result,message)=_service.AddTask(taskItem);
            if(result!=null){
                return Ok(message);
            }
            else{
                return BadRequest(new{message});
            }

        }

        // Deletes a specific TaskItem by its ID
        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            // Retrieve the task item and a message from the Taskservice
            var (result,message)=_service.DeleteTask(id);
            if(result){
                return Ok(new{message});
            }
            else{
                return BadRequest(new{message});
            }
        }

   
    }
}
