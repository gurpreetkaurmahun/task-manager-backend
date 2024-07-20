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
    public class SubTasksController : ControllerBase
    {
        private readonly TaskContext _context;
        private readonly ILogger<SubTasksController> _logger; 

        public SubTasksController(TaskContext context,ILogger<SubTasksController> logger)
        {
            _context = context;
            _logger=logger;
        }

        // GET: api/SubTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubTask>>> GetSubTasks()
        {
            try{
                _logger.LogInformationWithMethod($" Retreiving SubTasks===>");
                 var subTasks=await _context.SubTasks.ToListAsync();

                 _logger.LogInformationWithMethod("Sucessfully retreived SubTasks");
                  return Ok(subTasks);

            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed to retrieve TaskItems:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
          
        }

        // GET: api/SubTasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubTask>> GetSubTask(int id)
        {
            _logger.LogInformationWithMethod($"Retrieving TaskItem with ID {id}");
            try{
                
                var subTask = await _context.SubTasks.FindAsync(id);

                if (subTask == null)
                {
                     _logger.LogErrorWithMethod($"Failed to retrieve SubTask with id:{id}");
                    return NotFound();
                }

                _logger.LogInformationWithMethod($"Successfully retrieved SubTask with ID {id}");
                return subTask;
            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed to retrieve TaskItems:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
          
        }

        // PUT: api/SubTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubTask(int id, SubTask subTask)
        {
            

            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogErrorWithMethod($"Invalid request");
                    return BadRequest(ModelState);
                }

                if (id != subTask.SubTaskId)
                {
                     _logger.LogErrorWithMethod($"Invalid request");
                    return BadRequest();
                }

                _context.Entry(subTask).State = EntityState.Modified;
                _logger.LogInformationWithMethod($"SubTask with id:{id} successfully updated");
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Changes made to SubTask with Id {subTask.SubTaskId}" });

            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SubTaskExists(id))
                {
                    _logger.LogErrorWithMethod($"SubTask with id:{id} not found");
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

        // POST: api/SubTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<SubTask>> PostSubTask(SubTask subTask)
        {

            try{

                _logger.LogInformationWithMethod($"Checking if a TaskItem with id given in subtask exists");
                 var taskItem = await _context.TaskItems
                                .Include(t => t.SubTasks)
                                .FirstOrDefaultAsync(t => t.TaskItemId == subTask.TaskItemId);

                if (taskItem == null)
                {
                     _logger.LogWarningWithMethod($"TaskItem with ID {subTask.TaskItemId} not found. Cannot create SubTask.");
                    return NotFound($"TaskItem with ID {subTask.TaskItemId} not found.");
                }

                subTask.TaskItem = taskItem; // Set the navigation property
                _context.SubTasks.Add(subTask);
                await _context.SaveChangesAsync();


                //formatting the response to be sent 
                var response = new
                {
                    TaskId = taskItem.TaskItemId,
                    TaskName = taskItem.TaskItemName,
                    SubTasks = new
                    {
                        Id = "1",
                        Values = taskItem.SubTasks.Select((st, index) => new
                        {
                            Id = (index + 2).ToString(),
                            SubTaskId = st.SubTaskId,
                            SubTaskName = st.SubTaskName,   
                            SubTaskDescription = st.SubTaskDescription,
                            DateCreated = st.DateCreated,
                            DueDate = st.DueDate,
                            IsCompleted = st.IsCompleted,
                            TaskItemId = st.TaskItemId,
                            TaskItem = (object)null
                        }).ToList()
                    }
                };

                _logger.LogInformationWithMethod($"Successfully created SubTask with ID {subTask.SubTaskId} and related it to TaskItem with ID {subTask.TaskItemId}");
                return CreatedAtAction("GetSubTask", new { id = subTask.SubTaskId }, response);
            }

             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
        }

        // DELETE: api/SubTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTask(int id)
        {

            try{

                _logger.LogInformationWithMethod($"Searching for SubTask with id:{id}");
                 var subTask = await _context.SubTasks.FindAsync(id);

                if (subTask == null)
                {
                    _logger.LogErrorWithMethod($"SubTask with id:{id} not found");
                    return NotFound();
                }

                _context.SubTasks.Remove(subTask);
                await _context.SaveChangesAsync();

                _logger.LogInformationWithMethod($"SubTask with id:{id} deleted sucessfully");


                return Ok($"SubTask with id:{id} deleted sucessfully");
            }
             catch(Exception ex){
                 _logger.LogErrorWithMethod($"Failed with error:{ex.Message}");

                return StatusCode(500,$"Failed with error:{ex.Message}");

            }
           
        }

        private bool SubTaskExists(int id)
        {
            return _context.SubTasks.Any(e => e.SubTaskId == id);
        }
    }
}
