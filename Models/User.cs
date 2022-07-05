using FluentValidation;

namespace AllocationSystem.WebApi.Models
{
    public class User : BaseEntity
    {
        public long ID { get; set; }
        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string UserType { get; set; }

        public bool IsAdmin { get; set; }

        public bool IsActive { get; set; }
        public virtual Student Student { get; set; }
        public virtual Supervisor Supervisor { get; set; }
    }

    public class UserResponseDto : BaseEntityDto
    {
        public string Email { get; set; }

        public string UserType { get; set; }

        public bool IsAdmin { get; set; }

        public string Token { get; set; }

        public bool IsActive { get; set; }
        public Student Student { get; set; }
        public Supervisor Supervisor { get; set; }
    }
    public class UserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserType { get; set; }
        public string Course { get; set; }
        public string AcademicYear { get; set; }
        public long EmployeeID { get; set; }
        public long StudentNumber { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class CreateUserValidator : AbstractValidator<UserDto>
    {
        public CreateUserValidator()
        {
            RuleFor(x => x.Email).EmailAddress().MaximumLength(30).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
            RuleFor(x => x.Title).MaximumLength(10);
            RuleFor(x => x.FirstName).MaximumLength(75).NotEmpty();
            RuleFor(x => x.LastName).MaximumLength(75).NotEmpty();
            RuleFor(x => x.UserType).NotEmpty();
            RuleFor(x => x.EmployeeID).NotEmpty().When(s => s.UserType == "Supervisor");
            RuleFor(x => x.StudentNumber).NotEmpty().When(s => s.UserType == "Student");
        }
    }   
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).EmailAddress().MaximumLength(100).NotEmpty();
            RuleFor(x => x.Password).NotEmpty();
        }
    }
}
