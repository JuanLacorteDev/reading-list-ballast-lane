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
            Guard.AgainstEmptyGuid(ownerId, nameof(ownerId));

            Guard.AgainstNullOrWhiteSpace(title, nameof(title));
            Guard.AgainstMaxLength(title, 200, nameof(title));

            Guard.AgainstNullOrWhiteSpace(author, nameof(author));
            Guard.AgainstMaxLength(author, 100, nameof(author));

            OwnerId = ownerId;
            Title = title.Trim();
            Author = author.Trim();
        }


        public void SetStatus(ReadingStatus status)
        {
            //if change the status from read or abandoned to another, should clear the rating
            var isReadOrAbandoned = Status.Equals(ReadingStatus.Read) || Status.Equals(ReadingStatus.Abandoned);
            if (isReadOrAbandoned && !Status.Equals(status))
                Rating = null;

            Status = status;
        }

        public void SetRating(int rating)
        {
            Guard.AgainstNull(rating, nameof(rating));
            Guard.Against<DomainException>(
               !Status.Equals(ReadingStatus.Read),
               "Should only be able to set rating on readed books.");

            Rating = rating;
        }
    }
}
