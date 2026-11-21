using ReadingList.Domain.Commom.Base;
using ReadingList.Domain.Commom.Enums;
using ReadingList.Domain.Commom.Exceptions;
using ReadingList.Domain.Commom.Validations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace ReadingList.Domain.Entities
{
    public class Book : Entity
    {
        public Guid OwnerId { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public ReadingStatus Status { get; private set; }
        public int? Rating { get; private set; }


        private Book(Guid ownerId, string title, string author) : base()
        {
            SetDetails(ownerId, title, author);
            Status = ReadingStatus.WantToRead;
        }

        private Book(Guid id, Guid ownerId, string title, string author, ReadingStatus status, int? rating, DateTime createdAt, DateTime? updatedAt) : base(id, createdAt, updatedAt)
        {
            SetDetails(ownerId, title, author);
            Status = status;
            Rating = rating;
        }

        public static Book Create(Guid ownerId, string title, string author)
        {
            return new(ownerId, title, author);
        }

        public static Book Restore(Guid id, Guid ownerId, string title, string author, ReadingStatus status, int? rating, DateTime createdAt, DateTime? updatedAt)
        {
            return new(id, ownerId, title, author, status, rating, createdAt, updatedAt);
        }

        [MemberNotNull(nameof(Title), nameof(Author))]
        private void SetDetails(Guid ownerId, string title, string author)
        {
            title = title.Trim();
            author = author.Trim();

            Guard.AgainstEmptyGuid(ownerId, nameof(ownerId));

            Guard.AgainstNullOrWhiteSpace(title, nameof(title));
            Guard.AgainstMaxLength(title, 200, nameof(title));

            Guard.AgainstNullOrWhiteSpace(author, nameof(author));
            Guard.AgainstMaxLength(author, 100, nameof(author));

            OwnerId = ownerId;
            Title = title;
            Author = author;
        }


        public void ChangeStatus(ReadingStatus status)
        {
            Guard.Against<DomainException>(!ReadingStatus.IsDefined(status), "The provided enum value is not defined.");

            if (status is not ReadingStatus.Read)
                Rating = null;

            Status = status;
            SetUpdatedAt();
        }

        public void Rate(int rating)
        {
            Guard.Against<DomainException>(Status != ReadingStatus.Read, "Should only be able to set rating on readed books.");
            Guard.Against<DomainException>(!(1 <= rating && rating <= 5), "Should only be able to set rating in 1 to 5 range.");

            Rating = rating;
            SetUpdatedAt();
        }
    }
}
