using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Domain.Tests.Entities
{
    public class BookTests
    {
        public static readonly Guid _validUserId = Guid.NewGuid();

       

        public void Create_WithValidData_ReturnsBook()
        {
            //Act
            var book = Book.CreateBook(_validUserId, "The Silmarillion", "J.R.R Tolkien");

            //Assert
            //book.Should
        }
    }
}
