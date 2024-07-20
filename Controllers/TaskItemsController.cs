using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Models;
using TaskManager.Helpers;

namespace task_manager_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly TaskContext _context;
        private readonly ILogger<TaskItemsController> _logger; 


        public TaskItemsController(TaskContext context,ILogger<TaskItemsController>logger)
        {
            _context = context;
            _logger=logger;
        }

        // GET: api/TaskItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
        {
            
            try{
                _logger.LogInformationWithMethod($" Retreiving TaskItems===>");
                var taskItems=await _context.TaskItems.ToListAsync();
                _logger.LogInformationWithMethod("Sucessfully retreived TaskItems");
                return Ok(taskItems);
            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed to retrieve TaskItems:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
            
        }

        // GET: api/TaskItems/5
       [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetTaskItem(int id)
        {
            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");
            try
            {

                var taskItem = await _context.TaskItems
                .Include(t => t.SubTasks)
                .FirstOrDefaultAsync(t => t.TaskItemId == id);

            if (taskItem == null)
            {
                _logger.LogErrorWithMethod($"Failed to retrieve TaskItem with id:{id}");
                return NotFound();
            }


            //Creating a formatted response to include related subtasks a  well
            var response = new
            {
                taskItem.TaskItemId,
                taskItem.TaskItemName,
                taskItem.TaskItemDescription,
                taskItem.DataCreated,
                taskItem.DueDate,
                taskItem.IsCompleted,
                SubTasks = taskItem.SubTasks.Select(st => new
                {
                    st.SubTaskId,
                    st.SubTaskName,
                    st.SubTaskDescription,
                    st.DateCreated,
                    st.DueDate,
                    st.IsCompleted
                })
            };
            _logger.LogInformationWithMethod($"Successfully retrieved TaskItem with ID {id}");
            return Ok(response);
            }
            
            catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
            
        }

        // PUT: api/TaskItems/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTaskItem(int id, TaskItem taskItem)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogErrorWithMethod($"Invalid request");
                    return BadRequest(ModelState);
                }

                if (id != taskItem.TaskItemId)
                {
                    _logger.LogErrorWithMethod($"Invalid request");
                    return BadRequest();
                }
                
                _context.Entry(taskItem).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                 _logger.LogInformationWithMethod($"TaskItem with id:{id} successfully updated");
                return Ok(new { message = $"Changes made to  TaskItem with Id {taskItem.TaskItemId}" });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!TaskItemExists(id))
                {
                     _logger.LogErrorWithMethod($"TaskItem with id:{id} not found");
                    return NotFound();
                }
                _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");
                  return StatusCode(500,$"Failed with error:{ex.Message}");
            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }

            
        }

        // POST: api/TaskItems
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TaskItem>> PostTaskItem(TaskItem taskItem)
        {

            try{
                if (!ModelState.IsValid)
                {
                    _logger.LogErrorWithMethod($"Invalid request");
                    return BadRequest(ModelState);
                }

                _context.TaskItems.Add(taskItem);
                await _context.SaveChangesAsync();
                _logger.LogInformationWithMethod($"TaskItem with name:{taskItem.TaskItemName} added sucessfully");
                return CreatedAtAction("GetTaskItem", new { id = taskItem.TaskItemId }, taskItem);

            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
           
        }

        // DELETE: api/TaskItems/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {

            try{
                _logger.LogInformationWithMethod($"Searching for TaskItem with id:{id}");
                var taskItem = await _context.TaskItems.FindAsync(id);

                if (taskItem == null)
                {
                    _logger.LogErrorWithMethod($"TaskItem with id:{id} not found");
                    return NotFound();
                }

                _context.TaskItems.Remove(taskItem);
                await _context.SaveChangesAsync();

                _logger.LogInformationWithMethod($"TaskItem with id:{id} deleted sucessfully");
                
                return Ok($"TaskItem with id:{id} deleted sucessfully");
            } 
            catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
            
        }

        private bool TaskItemExists(int id)
        {
            return _context.TaskItems.Any(e => e.TaskItemId == id);
        }
    }
}
