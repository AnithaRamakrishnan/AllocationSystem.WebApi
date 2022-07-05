using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace AllocationSystem.WebApi.Business
{
    public class Business:IBusiness
    {
        #region "Private Variables"
        private readonly AllocationSystemDbContext _context;
        private readonly IMapper _mapper;
        #endregion

        public Business(AllocationSystemDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<IEnumerable<SupervisorTopicDto>> GetSelectedList(long id,long _UserID)
        {
            var topics = await _context
                .Topics
                .ToListAsync();
            var choicemade = await _context.SupervisorChoices.Where(x => x.SupervisorID == id).Select(x => x.TopicID).ToListAsync();
            var topicsDto = _mapper.Map<IEnumerable<SupervisorTopicDto>>(topics);
            foreach (var item in topicsDto)
            {
                item.IsSelected = choicemade.Exists(x => x == item.Id);
            }
            return topicsDto;
        }
        public async Task<IEnumerable<long>> GetSelectedStudentPrefList(long id)
        {            
            var topicPriorities = await _context
                .Preferences.Where(x => x.StudentID == id).OrderBy(x => x.PreferenceOrder).Select(x => x.TopicID) 
                .ToListAsync();
            
            return topicPriorities;
        }
    }
}
