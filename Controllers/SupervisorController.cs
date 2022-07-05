using AllocationSystem.WebApi.Business;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SupervisorController : BaseController
    {
        private readonly AllocationSystemDbContext _context;
        private readonly IBusiness _business;
        public SupervisorController(
           IActionContextAccessor accessor,
           AllocationSystemDbContext context, IBusiness business) : base(accessor)
        {
            _context = context;
            _business = business;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<SupervisorTopicDto>>> GetChoiceById(long id)
        {
            var topicsDto = await _business.GetSelectedList(id, UserId);
            return Ok(topicsDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IEnumerable<SupervisorTopicDto>>> PutChoice(long id, IEnumerable<SupervisorTopicDto> supervisorChoice)
        {
            try
            {

                foreach (var item in supervisorChoice)
                {
                    var existinglist = await _context.SupervisorChoices.Where(x => x.SupervisorID == id && x.TopicID == item.Id).FirstOrDefaultAsync();
                    if (existinglist != null && existinglist.TopicID == item.Id)
                    {
                        if (!item.IsSelected)
                        {
                            _context.SupervisorChoices.Remove(existinglist);
                        }
                    }
                    else
                    {
                        if (item.IsSelected)
                        {
                            _context.SupervisorChoices.Add(new SupervisorChoice { SupervisorID = id, TopicID = item.Id, CreatedBy = UserId, CreatedDate = DateTimeOffset.UtcNow });
                        }
                    }
                }
                await _context.SaveChangesAsync();
                var response = await _business.GetSelectedList(id, UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("SupervisorResult/{id}")]
        public async Task<ActionResult<IEnumerable<SupervisorResult>>> GetResult(long id)
        {
            var sup = from c1 in _context.Groups
                      join u1 in _context.Users.Include(x => x.Supervisor) on c1.SupervisorID equals u1.Supervisor.ID
                      where u1.Supervisor.ID == id
                      select new SupervisorResult { groupname = c1.GroupName, topicname = _context.Topics.Where(a => a.TopicID == c1.TopicID).Select(x => x.TopicName).FirstOrDefault() };

            return Ok(sup);
        }
    }
}
