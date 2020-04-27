using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace AutomatedWorkplace.Models {
    [AddINotifyPropertyChangedInterface]
    [Table("public.users")]
    public class User : Entity {
        [Column("id")] public int Id { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("login")]
        public string Login { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("password")]
        public string Password { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("role")]
        public string Role { get; set; }

        [Column("registration_date", TypeName = "date")]
        public DateTime RegistrationDate { get; set; }

        public override void BeginEdit() {
            if (InEdit) return;
            InEdit = true;
            BackupCopy = MemberwiseClone() as User;
        }

        public override void CopyProperties<T>(T source) {
            if (!(source is User tempSource)) return;
            Login = tempSource.Login;
            Password = tempSource.Password;
            Role = tempSource.Role;
            RegistrationDate = tempSource.RegistrationDate;
        }

        public override bool EqualsPrimaryKey<T>(T source) {
            return source is User tempSource && tempSource.Id == Id;
        }

        public override void InitializeValidator(params object[] entities) {
            var users = entities.Length > 0 ? entities[0] as IList<User> : null;
            Validator = GetValidator(users);
        }

        private IObjectValidator GetValidator(IList<User> users = null) {
            var builder = new ValidationBuilder<User>();

            builder.RuleFor(user => user.Login)
                   .NotEmpty()
                   .WithMessage("Login can't be empty")
                   .Must(login => users == null || !users.Any(user => !user.EqualsPrimaryKey(this) && user.Login == login))
                   .WithMessage("This login is already used")
                   .Must(login => string.IsNullOrWhiteSpace(login) ||
                                  Regex.IsMatch(login.Trim().ToLower(),
                                                @"^[à-ÿ0-9a-z¸`!@#$%^&*()_\-=+{}\[\];:'"",<.>/?\\*¹~|\s]{6,50}$"))
                   .WithMessage(
                       "Login must contain from 6 to 50 characters shown on the classic Russian-English keyboard layout");
            builder.RuleFor(user => user.Password)
                   .NotEmpty()
                   .WithMessage("Password can't be empty");
            builder.RuleFor(user => user.Role)
                   .Must(role => new[] {"ADMIN", "USER"}.Contains(role))
                   .WithMessage("Such role does not exist");

            return builder.Build(this);
        }

        protected bool Equals(User other) {
            return string.Equals(Login, other.Login) && string.Equals(Password, other.Password) &&
                   string.Equals(Role, other.Role) && RegistrationDate.Equals(other.RegistrationDate);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is User user && Equals(user);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                int hashCode = (Login != null ? Login.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Password != null ? Password.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Role != null ? Role.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ RegistrationDate.GetHashCode();
                return hashCode;
            }
        }
    }
}