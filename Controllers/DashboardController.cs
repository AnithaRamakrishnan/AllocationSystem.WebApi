using Microsoft.AspNetCore.Mvc;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AllocationSystem.WebApi.Business;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AdminAuthorize(Roles = AuthConstants.Roles.ADMINS)]
    public class DashboardController : BaseController
    {
        private readonly AllocationSystemDbContext _context;
        public DashboardController(
           IActionContextAccessor accessor,
           AllocationSystemDbContext context) : base(accessor)
        {
            _context = context;
        }

        [HttpGet("Submission")]
        public async Task<ActionResult<PreferenceProvidedStudents>> GetSubmissionList()
        {
            var stu = await _context.Students.CountAsync();
            var sub_stu = await _context.Students.Where(c => c.Preferences.Any()).CountAsync();
            var result = new PreferenceProvidedStudents { Submitted = sub_stu, NotSubmitted = stu - sub_stu };
            return Ok(result);
        }
        [HttpGet("LikedTopics")]
        public async Task<ActionResult<IEnumerable<LikedTopics>>> GetLikedTopics()
        {
            var stu = await _context.Students.CountAsync();
            var res = await _context.Topics.Select(b => new LikedTopics
            {
                TopicName = b.TopicName,
                Percentage = b.Preferences.Select(v => v.TopicID).Any() ?
                Math.Round((100 * Convert.ToDouble(b.Preferences.Count)) / stu) : 0
            }).ToListAsync();

            return Ok(res);
        }
        [HttpGet("Total")]
        public async Task<ActionResult<Total>> GetCount()
        {
            var stu = await _context.Students.CountAsync();
            var topi = await _context.Topics.CountAsync();
            var group = await _context.Groups.CountAsync();
            var to = new Total
            {
                Groups = group,
                Topics = topi,
                Students = stu
            };
            return Ok(to);
        }
        [HttpGet("FinalPreference")]
        public async Task<ActionResult<IEnumerable<StudentsFinalChoice>>> GetFinalPreferenceList()
        {
            var result = await _context.Students
               .GroupJoin(_context.Preferences, s => new { StudentID = s.ID, s.TopicID },
                       p => new { p.StudentID, TopicID = (long?)p.TopicID }, (s, p) => new { s, p })
               .SelectMany(v => v.p.DefaultIfEmpty(), (v, p) => new { v, p })
               .Select(x => new AllocatedStudents()
               {
                   MatchedPreferenceOrder = x.p.PreferenceOrder,
                   StudentName = x.v.s.Title + " " + x.v.s.FirstName + " " + x.v.s.LastName
               }).GroupBy(c => c.MatchedPreferenceOrder).Select(c => new StudentsFinalChoice { Preference = c.Key, Count = c.Any() ? c.Count() : 0 }).ToListAsync();

            return Ok(result);
        }
        [HttpGet("Result")]
        public async Task<ActionResult<IEnumerable<Result>>> GetAllocationResults()
        {
            var result = await _context.Users.Include(c => c.Student)
                .Join(_context.Groups, sp => sp.Student.GroupID, g => g.GroupID, (sp, g) => new { sp, g })
                .Join(_context.Topics, spg => spg.g.TopicID, t => t.TopicID, (spg, t) => new { spg, t })
                .GroupJoin(_context.Preferences, spgt => new { StudentID = spgt.spg.sp.Student.ID, spgt.spg.sp.Student.TopicID },
                        p => new { p.StudentID, TopicID = (long?)p.TopicID }, (spgt, p) => new { spgt, p })
                .SelectMany(spgt => spgt.p.DefaultIfEmpty(), (spgt, p) => new { f = spgt, p = p })
                .GroupBy(c => new { c.f.spgt.spg.g.GroupName, c.f.spgt.t.TopicName })
                .Select(x => new Result
                {
                    Students = (List<AllocatedStudents>)x.Select(e => new AllocatedStudents()
                    {
                        MatchedPreferenceOrder = e.p.PreferenceOrder,
                        StudentName = (string.IsNullOrEmpty(e.f.spgt.spg.sp.Student.Title) ? "" : e.f.spgt.spg.sp.Student.Title + " ") + 
                                      (string.IsNullOrEmpty(e.f.spgt.spg.sp.Student.FirstName) ? "" : e.f.spgt.spg.sp.Student.FirstName + " ") +
                                      (string.IsNullOrEmpty(e.f.spgt.spg.sp.Student.LastName) ? "" : e.f.spgt.spg.sp.Student.LastName) + 
                                      "(" + e.f.spgt.spg.sp.Email.Substring(0, e.f.spgt.spg.sp.Email.IndexOf('@')).Trim() + ")"
                    }),
                    GroupName = x.Key.GroupName,
                    TopicName = x.Key.TopicName,
                }).ToListAsync();

            var reswsup = new List<Result>();
            foreach (var i in result)
            {
                var supname = "";
                var supid = _context.Groups.Where(x => x.GroupName == i.GroupName).Select(x => x.SupervisorID).FirstOrDefault();
                if (supid != null)
                {
                    supname = await _context.Supervisors.Where(x => x.ID == supid).
                            Select(c => (string.IsNullOrEmpty(c.Title) ? "" : c.Title + " ") +
                            (string.IsNullOrEmpty(c.FirstName) ? "" : c.FirstName + " ") +
                            (string.IsNullOrEmpty(c.LastName) ? "" : c.LastName)).FirstOrDefaultAsync();
                }
                reswsup.Add(new Result { GroupName = i.GroupName, Students = i.Students, SupervisorName = supname, TopicName = i.TopicName });
            }
            return Ok(reswsup);
        }
    }
}
