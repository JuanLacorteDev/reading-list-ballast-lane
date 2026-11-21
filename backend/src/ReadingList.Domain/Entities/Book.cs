using ReadingList.Domain.Commom.Base;
using ReadingList.Domain.Commom.Enums;
using ReadingList.Domain.Commom.Validations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Domain.Entities
{
    public class Book : Entity
    {
        public Guid OwnerId { get; private set; }
        public string Title { get; private set; }
        public string Author { get; private set; }
        public ReadingStatus Status  { get; set; }
        public int? Rating { get; private set; }


        private Book(Guid ownerId, string  title, string author)
        {
            Guard.AgainstEmptyGuid(ownerId, nameof(ownerId));
            Guard.AgainstNullOrWhiteSpace(title, nameof(title));
            Guard.AgainstNullOrWhiteSpace(author, nameof(author));

            OwnerId = ownerId;
            Title = title;
            Author = author;
            Status = ReadingStatus.WantToRead;
        }

        public static Book CreateBook(Guid ownerId, string title, string author)
        {
            return new Book(ownerId, title, author);
        }
    }
}
