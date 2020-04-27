using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace AutomatedWorkplace.Models {
    [Table("public.authors")]
    public partial class Author : Entity {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Author() {
            // ReSharper disable once VirtualMemberCallInConstructor
            BooksAndAuthors = new HashSet<BookAndAuthor>();
        }

        [Column("id")] public int Id { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("fio")]
        public string FIO { get; set; }

        [StringLength(8000)]
        [Column("pseudonym")]
        public string Pseudonym { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookAndAuthor> BooksAndAuthors { get; set; }

        public override void BeginEdit() {
            if (InEdit) return;
            InEdit = true;
            BackupCopy = MemberwiseClone() as Author;
        }

        public override void InitializeValidator(params object[] entities) {
            var authors = entities.Length > 0 ? entities[0] as IList<Author> : null;
            Validator = GetValidator(authors);
        }

        public override void CopyProperties<T>(T source) {
            if (!(source is Author tempSource)) return;
            FIO = tempSource.FIO;
            Pseudonym = tempSource.Pseudonym;
        }

        public override bool EqualsPrimaryKey<T>(T source) {
            return source is Author tempSource && tempSource.Id == Id;
        }

        private IObjectValidator GetValidator(IList<Author> authors = null) {
            var builder = new ValidationBuilder<Author>();

            builder.RuleFor(author => author.FIO)
                   .NotEmpty()
                   .WithMessage("FIO can't be empty");
            builder.RuleFor(author => author.FIO)
                   .Must(fio => authors == null || !authors.Any(author => author.Id != Id && author.FIO == fio &&
                                                                          (author.Pseudonym == Pseudonym ||
                                                                           string.IsNullOrWhiteSpace(author.Pseudonym) &&
                                                                           string.IsNullOrWhiteSpace(Pseudonym))))
                   .WithMessage("Pair of FIO and Pseudonym should be unique");
            builder.RuleFor(author => author.Pseudonym)
                   .Must(pseudonym =>
                             authors == null || !authors.Any(author => author.Id != Id && author.FIO == FIO &&
                                                                       (author.Pseudonym == pseudonym ||
                                                                        string.IsNullOrWhiteSpace(author.Pseudonym) &&
                                                                        string.IsNullOrWhiteSpace(pseudonym))))
                   .WithMessage("Pair of FIO and Pseudonym should be unique");

            return builder.Build(this);
        }

        protected bool Equals(Author other) {
            return string.Equals(FIO, other.FIO) && string.Equals(Pseudonym, other.Pseudonym);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Author author && Equals(author);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                return ((FIO != null ? FIO.GetHashCode() : 0) * 397) ^
                       (Pseudonym != null ? Pseudonym.GetHashCode() : 0);
            }
        }
    }
}