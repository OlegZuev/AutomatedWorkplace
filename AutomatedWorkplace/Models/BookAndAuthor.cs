using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using PropertyChanged;
using ReactiveValidation;
using ReactiveValidation.Extensions;

namespace AutomatedWorkplace.Models {
    [Table("public.books_and_authors")]
    public class BookAndAuthor : Entity {
        [Key]
        [Column("isbn", Order = 0)]
        [StringLength(8000)]
        [DoNotCheckEquality]
        public string ISBN { get; set; }

        [Key]
        [Column("author_id", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DoNotCheckEquality]
        public int AuthorId { get; set; }

        [Column("author_book_number")]
        [DependsOn(nameof(AuthorBookNumberText))]
        public int AuthorBookNumber { get; set; }

        private string _authorBookNumber;

        [NotMapped]
        public string AuthorBookNumberText {
            get => _authorBookNumber;
            set {
                if (int.TryParse(value, out int result)) {
                    AuthorBookNumber = result;
                }

                _authorBookNumber = value;
            }
        }

        public virtual Author Author { get; set; }

        public virtual Book Book { get; set; }

        public override void BeginEdit() {
            if (InEdit) return;
            InEdit = true;
            BackupCopy = MemberwiseClone() as BookAndAuthor;
        }

        public override void CopyProperties<T>(T source) {
            if (!(source is BookAndAuthor tempSource)) return;
            ISBN = tempSource.ISBN;
            AuthorId = tempSource.AuthorId;
            AuthorBookNumberText = tempSource.AuthorBookNumberText;
            AuthorBookNumber = tempSource.AuthorBookNumber;
        }

        public override bool EqualsPrimaryKey<T>(T source) {
            return source is BookAndAuthor tempSource && tempSource.ISBN == ISBN && tempSource.AuthorId == AuthorId;
        }

        public override void InitializeValidator(params object[] entities) {
            var books = entities.Length > 0 ? entities[0] as IList<Book> : null;
            var authors = entities.Length > 1 ? entities[1] as IList<Author> : null;
            Validator = GetValidator(books, authors);
        }

        private IObjectValidator GetValidator(IList<Book> books = null, IList<Author> authors = null) {
            var builder = new ValidationBuilder<BookAndAuthor>();

            builder.RuleFor(booksAndAuthor => booksAndAuthor.ISBN)
                   .Must(isbn => books == null || books.Any(book => book.ISBN == isbn))
                   .WithMessage("Book that owns such ISBN does not exist");
            builder.RuleFor(booksAndAuthor => booksAndAuthor.AuthorId)
                   .Must(authorId => authors == null || authors.Any(author => author.Id == authorId))
                   .WithMessage("Such author does not exist");
            builder.RuleFor(booksAndAuthor => booksAndAuthor.AuthorBookNumberText)
                   .Must(authorBookNumberText =>
                             int.TryParse(authorBookNumberText, out int result) && result >= 0)
                   .WithMessage("Author book number should be positive number");

            return builder.Build(this);
        }

        protected bool Equals(BookAndAuthor other) {
            return string.Equals(ISBN, other.ISBN) && AuthorId == other.AuthorId &&
                   AuthorBookNumber == other.AuthorBookNumber;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is BookAndAuthor bookAndAuthor && Equals(bookAndAuthor);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode() {
            unchecked {
                int hashCode = (ISBN != null ? ISBN.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ AuthorId;
                hashCode = (hashCode * 397) ^ AuthorBookNumber;
                return hashCode;
            }
        }
    }
}