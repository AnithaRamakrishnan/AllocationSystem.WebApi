using AutoMapper;
using AllocationSystem.WebApi.Data;
using AllocationSystem.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AllocationSystem.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly AllocationSystemDbContext _context;
        private readonly IMapper _mapper;

        public AuthController(
            IActionContextAccessor accessor,
            AllocationSystemDbContext context
            , IMapper mapper) : base(accessor)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> PostUser(UserDto userDto)
        {
            bool IsExists = false;
            var userExists = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToUpper() == userDto.Email.ToUpper() && x.IsActive);
           
            if (userExists != null)
            {
                return BadRequest(1);
            }

            if(userDto.UserType=="Student")
            {
                IsExists = _context.Students.Where(x => x.ID == userDto.StudentNumber).Any();
            }
            else if(userDto.UserType=="Supervisor")
            {
                IsExists = _context.Supervisors.Where(x => x.ID == userDto.EmployeeID).Any();
            }

            if (IsExists)
            {
                return BadRequest(2);
            }

            var user = _mapper.Map<User>(userDto);
            user.CreatedBy = UserId;
            user.CreatedDate = DateTimeOffset.UtcNow;
            user.IsActive = true;
            user.IsAdmin = false;
            user.PasswordHash = AuthHelper.CreatePashwordHashSha256(userDto.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            if (userDto.UserType == "Student")
            {
                _context.Students.Add(new Student
                {
                    UserID = user.ID,
                    Title = userDto.Title,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    CreatedBy = UserId,
                    CreatedDate = DateTimeOffset.UtcNow,
                    IsActive = true,
                    ID = userDto.StudentNumber,
                    Course = userDto.Course,
                    AcademicYear = userDto.AcademicYear,
                });
            }
            else if (userDto.UserType == "Supervisor")
            {
                _context.Supervisors.Add(new Supervisor
                {
                    UserID = user.ID,
                    Title = userDto.Title,
                    FirstName = userDto.FirstName,
                    LastName = userDto.LastName,
                    CreatedBy = UserId,
                    CreatedDate = DateTimeOffset.UtcNow,
                    IsActive = true,
                    ID = userDto.EmployeeID,
                });
            }
            await _context.SaveChangesAsync();

            return Ok(_mapper.Map<UserResponseDto>(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseDto>> Login(LoginDto login)
        {
            var passwordHash = AuthHelper.CreatePashwordHashSha256(login.Password);
            var user = await _context.Users.Include(x => x.Student).Include(x => x.Supervisor)
                .FirstOrDefaultAsync(x => x.Email.ToUpper() == login.Email.ToUpper() && x.PasswordHash == passwordHash && x.IsActive);

            if (user == null)
            {
                return BadRequest();
            }
            var userDto = _mapper.Map<UserResponseDto>(user);
            userDto.Token = AuthHelper.GetJwtToken(user);

            return Ok(userDto);
        }
    }
}
