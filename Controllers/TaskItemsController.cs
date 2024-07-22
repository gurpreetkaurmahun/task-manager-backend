using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // GET: api/tasks/5
       [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskItem(int id)
        {
            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");

            var (taskItem,message)=_service.GetTaskItem(id);
             if (taskItem == null){
                return StatusCode(500, new{message});
            } else {
                _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
                return Ok(new{taskItem,message});
            }
            
            
        }

        //     public async Task<ActionResult<IEnumerable<SubTask>>> GetSubTaskItems(int id)
        // {
            
        //     try{
        //         _logger.LogInformationWithMethod($" Retreiving Sub Task Items===>");
        //         var subTaskItems = await _context.SubTasks.Where(subTask => subTask.TaskItemId == id).ToListAsync();
        //         _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
        //         return Ok(subTaskItems);
        //     }
        //      catch(Exception ex){
        //         //  _logger.LogErrorWithMethod($"Failed to retrieve TaskItems:{ex.Message}");

        //         return StatusCode(500,$"Failed with error:{ex.Message}");

        //     }
            
        // }

    //     // PUT: api/tasks/5
    //     // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskItem(int id, TaskItem taskItem)
        {
            var (result,message)=_service.UpdateTask(id,taskItem);
            if(result){
                return Ok(new{message});
            }
            else{
                return BadRequest(new{message});
            }


        }



        // POST: api/tasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskItem>> PostTaskItem(TaskItem taskItem)
        {

            var (result,message)=_service.AddTask(taskItem);
            if(result!=null){
                return Ok(message);
            }
            else{
                return BadRequest(new{message});
            }

        }

   

    //     // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
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
