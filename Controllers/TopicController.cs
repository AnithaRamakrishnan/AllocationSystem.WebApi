using AllocationSystem.WebApi.Models;
using AllocationSystem.WebApi.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TopicController : BaseController
    {
        private readonly AllocationSystemDbContext _context;
        private readonly IMapper _mapper;

        public TopicController(
            IActionContextAccessor accessor,
            AllocationSystemDbContext context
            , IMapper mapper) : base(accessor)
        {
            _context = context;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TopicListDto>>> GetTopics()
        {
            var topics = await _context
                .Topics
                .ToListAsync();
            var usedTopicsStudent = await _context.Preferences.Select(x => x.TopicID).Distinct().ToListAsync();
            var usedTopicsSupervisor = await _context.SupervisorChoices.Select(x => x.TopicID).Distinct().ToListAsync();
            var topicsDto = _mapper.Map<IEnumerable<TopicListDto>>(topics);
            foreach (var item in topicsDto)
            {
                item.IsAlreadyUsed = usedTopicsStudent.Exists(x => x == item.Id);
                if(!item.IsAlreadyUsed)
                    item.IsAlreadyUsed = usedTopicsSupervisor.Exists(x => x == item.Id);
            }
            return Ok(topicsDto);
        }
        [HttpPost]  
        public async Task<ActionResult<TopicResponseDto>> PostTopic(TopicDto topicDto)
        {
            var topicExists = await _context.Topics.FirstOrDefaultAsync(x => x.TopicName.ToUpper() == topicDto.TopicName.ToUpper());

            if (topicExists != null)
            {
                return BadRequest();
            }
            var topic = new Topic
            {
                TopicName = topicDto.TopicName,
                CreatedBy = UserId,
                CreatedDate = DateTimeOffset.UtcNow,
            };

            _context.Topics.Add(topic);
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<TopicDto>(topic));
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTopic(long id, TopicListDto topicText)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(x => x.TopicID == id);
            if (topic == null)
            {
                return NotFound();
            }

            var alreadyExists = await _context.Topics.FirstOrDefaultAsync(x => x.TopicID != id && x.TopicName.Trim().ToUpper() == topicText.Text.Trim().ToUpper());
            if (alreadyExists != null)
            {
                return Conflict();
            }

            topic.TopicName = topicText.Text;
            topic.LastUpdatedBy = UserId;
            topic.LastUpdatedDate = DateTimeOffset.UtcNow;

            _context.Entry(topic).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTopic(long id)
        {
            var topic = await _context.Topics.FirstOrDefaultAsync(x => x.TopicID == id);
            if (topic == null)
            {
                return NotFound();
            }          

            _context.Entry(topic).State = EntityState.Deleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
