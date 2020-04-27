using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace AutomatedWorkplace.Models {
    [Table("public.publishers")]
    public class Publisher : Entity {
        [SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Publisher() {
            // ReSharper disable once VirtualMemberCallInConstructor
            Books = new HashSet<Book>();
        }

        [Column("id")] public int Id { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("name")]
        public string Name { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("address")]
        public string Address { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Book> Books { get; set; }

        public override void BeginEdit() {
            if (InEdit) return;
            InEdit = true;
            BackupCopy = MemberwiseClone() as Publisher;
        }

        public override void CopyProperties<T>(T source) {
            if (!(source is Publisher tempSource)) return;
            Name = tempSource.Name;
            Address = tempSource.Address;
        }

        public override bool EqualsPrimaryKey<T>(T source) {
            return source is Publisher tempSource && tempSource.Id == Id;
        }

        public override void InitializeValidator(params object[] entities) {
            var publishers = entities.Length > 0 ? entities[0] as IList<Publisher> : null;
            Validator = GetValidator(publishers);
        }

        private IObjectValidator GetValidator(IList<Publisher> publishers = null) {
            var builder = new ValidationBuilder<Publisher>();

            builder.RuleFor(publisher => publisher.Name)
                   .NotEmpty()
                   .WithMessage("Name can't be empty");
            builder.RuleFor(publisher => publisher.Address)
                   .NotEmpty()
                   .WithMessage("Address can't be empty");
            builder.RuleFor(publisher => publisher.Name)
                   .Must(name => publishers == null || !publishers.Any(publisher =>
                                                                           !publisher.EqualsPrimaryKey(this) && publisher.Name == name &&
                                                                           publisher.Address == Address))
                   .WithMessage("Pair of Name and Address should be unique");
            builder.RuleFor(publisher => publisher.Address)
                   .Must(address => publishers == null || !publishers.Any(publisher =>
                                                                              !publisher.EqualsPrimaryKey(this) && publisher.Name == Name &&
                                                                              publisher.Address == address))
                   .WithMessage("Pair of Name and Address should be unique");

            return builder.Build(this);
        }

        protected bool Equals(Publisher other) {
            return string.Equals(Name, other.Name) && string.Equals(Address, other.Address);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Publisher publisher && Equals(publisher);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Address != null ? Address.GetHashCode() : 0);
            }
        }
    }
}