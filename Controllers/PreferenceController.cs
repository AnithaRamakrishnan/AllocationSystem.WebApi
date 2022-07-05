using AutoMapper;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AllocationSystem.WebApi.Business;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PreferenceController : BaseController
    {
        private readonly AllocationSystemDbContext _context;        
        private readonly IBusiness _business;

        public PreferenceController(
           IActionContextAccessor accessor,
           AllocationSystemDbContext context
           , IBusiness business) : base(accessor)
        {
            _context = context;
            _business = business;
        }
        [HttpGet]
        public async Task<ActionResult<AdminSettingValidate>> GetAdminSettingsforValidation()
        {
            var pref = await _context
                .AdminSettings.Select(s => new AdminSettingValidate { NoOfPreferences = String.IsNullOrEmpty(s.NoOfPreferences.ToString()) ? 1 : s.NoOfPreferences, 
                    LastSubmissionDate = s.LastSubmissionDate<=DateTime.Now, IsAllocationDone = s.IsAllocationDone }).FirstOrDefaultAsync();
            return Ok(pref);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<long>>> GetPreferences(long id)
        {
            var topicPriorities = await _business.GetSelectedStudentPrefList(id);
            return Ok(topicPriorities);
        }
        [HttpPost]
        public async Task<ActionResult<IEnumerable<long>>> PostPreference(SavePreferences Pref)
        {
            if (Pref == null)
                return BadRequest();

            var prefdel = _context.Preferences.Where(x => x.StudentID == Pref.ID);
            foreach (var p in prefdel)
            {
                if (p.PreferenceOrder > Pref.topicPriorities.Count)
                {
                    _context.Preferences.Remove(p);
                }
            }
            _context.SaveChanges();

            var date = DateTimeOffset.UtcNow;
            for (int i = 0; i < Pref.topicPriorities.Count; i++)
            {

                var IsUpdate = _context.Preferences.Where(c => c.StudentID == Pref.ID && c.PreferenceOrder == i + 1).FirstOrDefault();
                if (IsUpdate != null)
                {
                    IsUpdate.TopicID = Pref.topicPriorities[i];
                    IsUpdate.LastUpdatedBy = Pref.ID;
                    IsUpdate.LastUpdatedDate = date;
                }
                else
                {
                    _context.Preferences.Add(new Preference { StudentID = Pref.ID, PreferenceOrder = i + 1, TopicID = Pref.topicPriorities[i], CreatedBy = Pref.ID, CreatedDate = date });

                }
            }
            await _context.SaveChangesAsync();
            var res = await _business.GetSelectedStudentPrefList(Pref.ID);
            return Ok(res);
        }
        [HttpGet("StudentResult/{id}")]
        public async Task<ActionResult<StudentResult>> GetFinalList(long id)
        {
            var stu = from c1 in _context.Students
                       where c1.GroupID==(_context.Students.Where(x=>x.ID == id).Select(c=>c.GroupID).FirstOrDefault()) 
                       join u1 in _context.Users on c1.UserID equals u1.ID
                       select new string(c1.Title + " " + c1.FirstName + " " + c1.LastName + " (" + u1.Email.Substring(0, u1.Email.IndexOf('@')).Trim() + " )").Trim();

            var result =  new StudentResult
            {
                Studentlist = stu.ToList(),
                groupname = await _context.Students.Where(c => c.ID == id).Join(_context.Groups, c => c.GroupID, d => d.GroupID, (c, d) => new { c, d }).Select(x => x.d.GroupName).FirstOrDefaultAsync(),
                topicname = await _context.Students.Where(c => c.ID == id).Join(_context.Topics, c => c.TopicID, d => d.TopicID, (c, d) => new { c, d }).Select(x => x.d.TopicName).FirstOrDefaultAsync()
            };
            return Ok(result);
        }
    }
}
