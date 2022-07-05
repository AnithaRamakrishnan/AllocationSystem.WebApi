using Microsoft.AspNetCore.Mvc;
using AllocationSystem.WebApi.Models;
using AllocationSystem.WebApi.Data;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace AllocationSystem.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AdminAuthorize(Roles = AuthConstants.Roles.ADMINS)]
    public class BulkGeneratorController : BaseController
    {

        private readonly AllocationSystemDbContext _context;

        public BulkGeneratorController(
            IActionContextAccessor accessor,
            AllocationSystemDbContext context) : base(accessor)
        {
            _context = context;
        }

        private static readonly string[] Courses = new[]
        {
            "Cloud Computing", "Advance Computer Science", "Human Computer Interaction", "Advance Software Engineering"
        };
        private static readonly string[] AcademicYears = new[]
        {
            "2021-2022", "2022-2023"
        };

        [HttpPost("Students")]
        public async Task<ActionResult<string>> GenerateStudents(BulkStudentsCreator bulkStudentsCreator)
        {
            var dbcount = _context.Students.Count();
            var end = dbcount + bulkStudentsCreator.NoOfStudents;
            var init = dbcount + 1;
            
            for (int i = init; i <= end; i++)
            {
                string name = bulkStudentsCreator.StudentPrefix + (i - dbcount);
                if (!_context.Students.Any(o => o.FirstName == name))
                {
                    var usr = new User
                    {
                        CreatedBy = UserId,
                        CreatedDate = DateTimeOffset.UtcNow,
                        IsActive = true,
                        IsAdmin = false,
                        PasswordHash = AuthHelper.CreatePashwordHashSha256("123"),
                        Email = name + "@student.le.ac.uk",
                        UserType = "Student"
                    };
                    _context.Users.Add(usr);
                    _context.SaveChanges();

                    _context.Students.Add(new Student
                    {
                        UserID = usr.ID,
                        FirstName = name,
                        ID = i,
                        CreatedBy = UserId,
                        CreatedDate = DateTimeOffset.UtcNow,
                        Course = Courses[Random.Shared.Next(Courses.Length)],
                        AcademicYear = AcademicYears[Random.Shared.Next(AcademicYears.Length)],
                        IsActive = true,
                    });
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Students created successfully");
        }
        [HttpPost]
        public async Task<ActionResult<string>> GenerateStudentsTopicsanditsPrefernce(BulkPreferenceCreator bulkPreferenceCreator)
        {
            if (bulkPreferenceCreator.NoOfPreferences > bulkPreferenceCreator.NoOfTopics)
            {
                return BadRequest(1);
            }

            var lst = AddTopicsToDBContext(bulkPreferenceCreator.NoOfTopics, bulkPreferenceCreator.TopicPrefix);
            var topicidslist = _context.Topics.Where(s => lst.Contains(s.TopicName)).Select(s => s.TopicID).ToArray();

            var stucount = _context.Students.Count();
            var end = stucount + bulkPreferenceCreator.NoOfStudents;
            var init = stucount + 1;
            for (int i = init; i <= end; i++)
            {
                string name = bulkPreferenceCreator.StudentPrefix + (i - stucount);
                if (!_context.Students.Any(o => o.FirstName == name))
                {
                    var usr = new User
                    {
                        CreatedBy = UserId,
                        CreatedDate = DateTimeOffset.UtcNow,
                        IsActive = true,
                        IsAdmin = false,
                        PasswordHash = AuthHelper.CreatePashwordHashSha256("123"),
                        Email = name + "@student.le.ac.uk",
                        UserType = "Student"
                    };
                    _context.Users.Add(usr);
                    _context.SaveChanges();

                    _context.Students.Add(new Student
                    {
                        UserID = usr.ID,
                        FirstName = name,
                        ID = i,
                        CreatedBy = UserId,
                        CreatedDate = DateTimeOffset.UtcNow,
                        Course = Courses[Random.Shared.Next(Courses.Length)],
                        AcademicYear = AcademicYears[Random.Shared.Next(AcademicYears.Length)],
                        IsActive = true,
                    });
                    
                    var rng = new Random();
                    var shuftopic = topicidslist.OrderBy(x => rng.Next()).ToList();
                    var shufpref = Enumerable.Range(1, bulkPreferenceCreator.NoOfPreferences).OrderBy(x => rng.Next()).ToList();

                    for (int j = 1; j <= bulkPreferenceCreator.NoOfPreferences; j++)
                    {
                        _context.Preferences.Add(new Preference
                        {
                            TopicID = shuftopic[j - 1],
                            PreferenceOrder = shufpref[j - 1],
                            CreatedBy = UserId,
                            CreatedDate = DateTimeOffset.UtcNow,
                            StudentID = i,
                        });
                    }
                }
            }
            await _context.SaveChangesAsync();

            return Ok("Students,Topics and their random preferences created successfully");
        }

        [HttpPost("Topics")]
        public ActionResult<string> GenerateTopics(BulkTopicsCreator bulkTopicsCreator)
        {
            AddTopicsToDBContext(bulkTopicsCreator.NoOfTopics, bulkTopicsCreator.TopicPrefix);
            return Ok("Topics Generated successfully");
        }
        private List<String> AddTopicsToDBContext(int NoOfTopics, string TopicPrefix)
        {
            var lst = new List<string>();
            for (int i = 1; i <= NoOfTopics; i++)
            {
                if (!_context.Topics.Any(o => o.TopicName == TopicPrefix + i))
                {

                    _context.Topics.Add(new Topic
                    {
                        TopicName = TopicPrefix + i,
                        CreatedBy = UserId,
                        CreatedDate = DateTimeOffset.UtcNow,
                    });
                    lst.Add(TopicPrefix + i);
                }
            }
            _context.SaveChanges();
            return lst;
        }
    }
}
