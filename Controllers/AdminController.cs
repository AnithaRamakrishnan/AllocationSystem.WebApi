using AutoMapper;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AllocationSystem.WebApi.Business;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorize(Roles = AuthConstants.Roles.ADMINS)]
    public class AdminController : BaseController
    {
        private readonly AllocationSystemDbContext _context;
        private readonly IMapper _mapper;
        private readonly IAutoAllocation _autoAllocation;
        public AdminController(
           IActionContextAccessor accessor,
           AllocationSystemDbContext context
           , IMapper mapper, IAutoAllocation autoAllocation) : base(accessor)
        {
            _context = context;
            _mapper = mapper;
            _autoAllocation = autoAllocation;
        }
        [HttpPost]
        public async Task<ActionResult<AdminSettingResponseDto>> PostAdminSettings(AdminSettingDto adminDto)
        {
            //retrive existing entity
            var adminExists = await _context.AdminSettings.FirstOrDefaultAsync();

            // if entity does not exist -> create new else update
            if (adminExists == null)
            {
                var admin = new AdminSetting
                {
                    TeamSize = adminDto.TeamSize,
                    NoOfPreferences = adminDto.NoOfPreferences,
                    LastSubmissionDate = adminDto.LastSubmissionDate,
                    IsAllocationDone = adminDto.IsAllocationDone,
                    NoOfGroups = adminDto.NoOfGroups,
                    IsTopicMultiple = adminDto.IsTopicMultiple,
                    CreatedBy = UserId,
                    CreatedDate = DateTimeOffset.UtcNow,
                };
                _context.AdminSettings.Add(admin);
            }
            else
            {
                adminExists.NoOfPreferences = adminDto.NoOfPreferences;
                adminExists.TeamSize = adminDto.TeamSize;
                adminExists.LastSubmissionDate = adminDto.LastSubmissionDate;
                adminExists.IsAllocationDone = adminDto.IsAllocationDone;
                adminExists.IsTopicMultiple = adminDto.IsTopicMultiple;
                adminExists.NoOfGroups = adminDto.NoOfGroups;
                adminExists.LastUpdatedDate = DateTimeOffset.UtcNow;
                adminExists.LastUpdatedBy = UserId;
            }

            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<AdminSettingResponseDto>(adminExists));
        }
        [HttpGet]
        public async Task<ActionResult<PreferenceDto>> GetAdminSettings()
        {
            var admin = await _context
                .AdminSettings.FirstOrDefaultAsync();
            var adminDto = _mapper.Map<AdminSettingResponseDto>(admin);
            adminDto.IsPreferenceSelected = await _context.Preferences.AnyAsync();
            //adminDto.IsTeamSizeUsed = await _context.Groups.AnyAsync();            
            return Ok(adminDto);
        }
        [HttpGet("notify")]
        public async Task<ActionResult<Boolean>> GetAdminWarning()
        {
            Boolean isExceed = false;
            var admin = await _context
                .AdminSettings.FirstOrDefaultAsync();
            var topiccount = await _context.Topics.LongCountAsync();
            var studentcount = await _context.Students.LongCountAsync();
            if (admin != null)
                isExceed = admin.IsTopicMultiple ? ((admin.NoOfGroups * (admin.TeamSize * topiccount)) < studentcount) : ((admin.TeamSize * topiccount) < studentcount);
            return Ok(isExceed);
        }
        [HttpGet("StudentSearch")]
        public async Task<ActionResult<StudentList>> GetStudents()
        {
            var pref = await _context.Users.Include(x => x.Student).Where(x=>x.UserType=="Student").
                Select(x => new StudentList { label = ((string.IsNullOrEmpty(x.Student.FirstName)?"": x.Student.FirstName + " ") + (string.IsNullOrEmpty(x.Student.LastName) ? "" : x.Student.LastName))+"("+ x.Email.Substring(0, x.Email.IndexOf('@')).Trim() + ")", id = x.Student.ID }).ToListAsync();
            
            return Ok(pref);
        }
        [HttpGet("Allocation")]
        public async Task<ActionResult<String>> AutoAllocation()
        {
            var topic = await _context.Topics.AnyAsync();
            if (!topic)
                return Ok("Topic Unavailable. Sorry Cannot run the process.");

            var student = await _context.Students.AnyAsync();
            if (!student)
                return Ok("Students Unavailable. Sorry Cannot run the process.");
            var supevisor = await _context.Supervisors.AnyAsync();
            if (!supevisor)
                return Ok("Supevisors Unavailable. Sorry Cannot run the process.");

            var admin = await _autoAllocation.RunAllocationProcess(UserId);

            if (admin.IsSuccess)
                return Ok("Success");
            else
                return Ok("Allocation Process Failed due to " + admin.Error);
        }
    }
}
