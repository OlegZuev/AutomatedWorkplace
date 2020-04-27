using System;
using System.Data.Entity;

namespace AutomatedWorkplace.Models {
    public partial class BookOrdersContext : DbContext {
        public BookOrdersContext()
            : base("name=BookOrdersContext") {
        }

        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Book> Books { get; set; }
        public virtual DbSet<BookAndAuthor> BooksAndAuthors { get; set; }
        public virtual DbSet<Publisher> Publishers { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Entity<Author>()
                .HasMany(e => e.BooksAndAuthors)
                .WithRequired(e => e.Author)
                .HasForeignKey(e => e.AuthorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Book>()
                .HasMany(e => e.BooksAndAuthors)
                .WithRequired(e => e.Book)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Publisher>()
                .HasMany(e => e.Books)
                .WithRequired(e => e.Publisher)
                .HasForeignKey(e => e.PublisherId)
                .WillCascadeOnDelete(false);
        }

        public static event Action NewEntityAdded;

        public static void RaiseNewEntityAdded() {
            NewEntityAdded?.Invoke();
        }

        public static event Action<User> UserStatusLoaded;

        public static void RaiseUserStatusLoaded(User user) {
            UserStatusLoaded?.Invoke(user);
        }
    }
}
