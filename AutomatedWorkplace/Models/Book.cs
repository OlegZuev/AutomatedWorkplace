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
    [Table("public.books")]
    public class Book : Entity {
        [SuppressMessage("Microsoft.Usage",
            "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Book() {
            // ReSharper disable once VirtualMemberCallInConstructor
            BooksAndAuthors = new HashSet<BookAndAuthor>();
        }

        [Key]
        [StringLength(8000)]
        [Column("isbn")]
        public string ISBN { get; set; }

        public void OnISBNChanged() {
            if (ISBNUtils.IsValid(ISBN))
                PublisherId = ISBNUtils.GetPublisherId(ISBN);
        }

        [DoNotCheckEquality]
        [Column("publisher_id")]
        public int PublisherId { get; set; }

        public void OnPublisherIdChanged() {
            if (ISBN == null) return;
            ISBN = Regex.Replace(ISBN, @"(?<=[\d]{3}-\d+-)\d+",
                                 PublisherId < 10 ? "0" + PublisherId : PublisherId.ToString());
        }

        [Required]
        [StringLength(8000)]
        [Column("name")]
        public string Name { get; set; }

        [Column("release_date", TypeName = "date")]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [StringLength(8000)]
        [Column("age_restriction")]
        public string AgeRestriction { get; set; }

        [SuppressMessage("Microsoft.Usage",
            "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BookAndAuthor> BooksAndAuthors { get; set; }

        public virtual Publisher Publisher { get; set; }

        public override void BeginEdit() {
            if (InEdit) return;
            InEdit = true;
            BackupCopy = MemberwiseClone() as Book;
        }

        public override void CopyProperties<T>(T source) {
            if (!(source is Book tempSource)) return;
            ISBN = tempSource.ISBN;
            PublisherId = tempSource.PublisherId;
            Name = tempSource.Name;
            ReleaseDate = tempSource.ReleaseDate;
            AgeRestriction = tempSource.AgeRestriction;
        }

        public override bool EqualsPrimaryKey<T>(T source) {
            return source is Book tempSource && tempSource.ISBN == ISBN;
        }

        public override void InitializeValidator(params object[] entities) {
            var books = entities.Length > 0 ? entities[0] as IList<Book> : null;
            var publishers = entities.Length > 1 ? entities[1] as IList<Publisher> : null;
            Validator = GetValidator(books, publishers);
        }

        private IObjectValidator GetValidator(IList<Book> books = null, IList<Publisher> publishers = null) {
            var builder = new ValidationBuilder<Book>();

            builder.RuleFor(book => book.ISBN)
                   .Must(ISBNUtils.IsValid)
                   .WithMessage("ISBN is not valid");
            builder.RuleFor(book => book.ISBN)
                   .Must(isbn => publishers == null ||
                                 publishers.Any(publisher => publisher.Id == ISBNUtils.GetPublisherId(isbn)))
                   .WithMessage("Publisher that owns such ISBN does not exist")
                   .Must(isbn => books == null ||
                                 !books.Any(book => !ReferenceEquals(book, this) && book.ISBN == isbn))
                   .WithMessage("ISBN should be unique")
                   .AllWhen(book => ISBNUtils.IsValid(book.ISBN));
            builder.RuleFor(book => book.Name)
                   .NotEmpty()
                   .WithMessage("Name can't be empty");
            builder.RuleFor(book => book.AgeRestriction)
                   .NotEmpty()
                   .WithMessage("Age Restriction can't be empty")
                   .Matches(@"^\d+\+$")
                   .WithMessage("Age Restriction has wrong format");

            return builder.Build(this);
        }

        protected bool Equals(Book other) {
            return string.Equals(ISBN, other.ISBN) && PublisherId == other.PublisherId &&
                   string.Equals(Name, other.Name) && ReleaseDate.Equals(other.ReleaseDate) &&
                   string.Equals(AgeRestriction, other.AgeRestriction);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Book book && Equals(book);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                int hashCode = (ISBN != null ? ISBN.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ PublisherId;
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ ReleaseDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (AgeRestriction != null ? AgeRestriction.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}