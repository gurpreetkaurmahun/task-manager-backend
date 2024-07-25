using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Helpers;
using TaskManager.Service;

namespace TaskManager.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    public class SubTasksController : ControllerBase
    {
       private readonly SubTaskService _service;
        private readonly ILogger<SubTasksController> _logger; 
        public SubTasksController(SubTaskService service,ILogger<SubTasksController> logger)
        {
            _service=service;
            _logger=logger;
        }

        //// Retrieves a specific SubTask for a given TaskId and SubTaskId
        // GET: api/Tasks/{id}/SubTasks/{id}
         [HttpGet("{taskId}/subtasks/{id}")]
        public async Task<ActionResult<SubTask>> GetSubTask(int taskId,int id)
        {

            // Retrieve the specific sub-task and message  from the SubTask service
            var(subTask,message)=_service.GetSubTaskWithId(id);
            if(subTask!=null){
                return Ok(new{subTask,message});
            }
            else{
                return BadRequest(new{message});
            }
        }

        // Retrieves all SubTasks for a given TaskId
        // GET: api/Tasks/{id}/SubTasks
        [HttpGet("{id}/subtasks")]
        public async Task<ActionResult<SubTask>> GetSubTasks(int id)
        {
           
            // Retrieve the specific sub-task and message  from the SubTask service
            var(subTask,message)=_service.GetSubTasks(id);

            if(subTask!=null){
                return Ok(new{subTask,message});
            }
            else{
                return BadRequest(message);
            }
          
        }

        // Updates a specific SubTask for a given TaskId and SubTaskId
        // PUT: api/SubTasks/5
        [HttpPut("{taskId}/subtasks/{id}")]
        public async Task<IActionResult> PutSubTask(int taskId, int id, SubTask subTask)
        {

             // Retrieve the specific sub-task and message  from the SubTask service
            var(result,message)=_service.UpdateSubTask(taskId,id,subTask);

            if(result){
                return Ok(new{message});
            }
            else{
                return BadRequest(new{message});
            }

        }

        // // POST: api/SubTasks
        // Adds a new SubTask to the specified TaskId
        [HttpPost("{id}/subtasks")]
        public async Task<ActionResult<SubTask>> PostSubTask(SubTask subTask, int id)
        {
            
            subTask.TaskItemId = id;
            
             // Retrieve the specific sub-task and message  from the SubTask service
            var(subTaskItem,message)=_service.AddSubTask(subTask);

            if(subTaskItem!=null){
                return Ok(new{subTask,message});
            }
            else{
                return BadRequest(new{message});
            }

        }

        // Deletes a specific SubTask for a given TaskId and SubTaskId
        // DELETE: api/SubTasks/5
        [HttpDelete("{taskId}/SubTasks/{id}")]
        public async Task<IActionResult> DeleteSubTaskMethod(int taskId,int id)
        {
            
             // Retrieve the specific sub-task and message  from the SubTask service
            var(result,message)=_service.DeleteSubTask(taskId,id);

            if(result){
                return Ok(new{message});
            }
            else{
                return  BadRequest(new{message});
            }
        }

    }
}
