using ReadingList.Domain.Commom.Enums;
using ReadingList.Domain.Commom.Exceptions;
using ReadingList.Domain.Entities;
using ReadingList.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReadingList.Domain.Tests.Entities
{
    public class BookTests
    {
        private static readonly Guid ValidOwnerId = Guid.NewGuid();
        private const string ValidTitle = "The Silmarillion";
        private const string ValidAuthor = "J.R.R. Tolkien";

        [Fact(DisplayName = "Should return a valid book when data is valid")]
        public void Create_WithValidData_ReturnsBook()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;

            // Act
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);

            // Assert
            Assert.NotEqual(Guid.Empty, book.Id);
            Assert.Equal(ValidOwnerId, book.OwnerId);
            Assert.Equal(ValidTitle, book.Title);
            Assert.Equal(ValidAuthor, book.Author);
            Assert.Equal(ReadingStatus.WantToRead, book.Status);
            Assert.Null(book.Rating);
            Assert.InRange(book.CreatedAt, beforeCreate, DateTime.UtcNow);
            Assert.Null(book.UpdatedAt);
        }


        [Theory(DisplayName = "Should throw erros when empty Title")]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyTitle_ThrowsDomainException(string title)
        {
            var exception = Assert.Throws<DomainException>(
                () => Book.Create(ValidOwnerId, title, ValidAuthor));

            Assert.Contains("title", exception.Message);
        }

        [Theory(DisplayName = "Should throw erros when empty Author")]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptAuthor_ThrowsDomainException(string author)
        {
            var exception = Assert.Throws<DomainException>(
                () => Book.Create(ValidOwnerId, ValidTitle, author));

            Assert.Contains("author", exception.Message);
        }

        [Theory(DisplayName = "Should throw erros when title longer then 200 chars")]
        [InlineData("Designing a Scalable and Maintainable Reading List Application with C#, .NET, and Domain-Driven Design: A Practical Approach to Clean Architecture, Value Objects, and Robust Domain Validation for Modern Software Engineers")]
        [InlineData("Building a Personal Reading Tracker from Scratch: How to Model Books, Reading Statuses, and Ratings in a Clean, Testable Domain Layer Using C#, Guard Clauses, and Immutable Value Objects Without Over-Engineering")]
        public void Create_WithTitleLongerThan200Chars_Fails(string title)
        {
            var exception = Assert.Throws<DomainException>(
                () => Book.Create(ValidOwnerId, title, ValidAuthor));

            Assert.Contains($"{nameof(title)} can not have more than 200 carcateres, given {title.Length}.", exception.Message);
        }


        [Fact(DisplayName = "Should throw erros when try to set rating into not reading book")]
        public void SetRating_WhenStatusIsNotRead_Fails()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);

            // Act
            var exception = Assert.Throws<DomainException>(
               () => book.Rate(1));

            // Assert
            Assert.Contains("Should only be able to set rating on readed books.", exception.Message);
        }

        [Theory(DisplayName = "Should throw erros when try to set rating out of 1 to 5 range")]
        [InlineData(-1)]
        [InlineData(6)]
        [InlineData(0)]
        public void SetRating_WhenOutOfRange_Fails(int rating)
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);
            book.ChangeStatus(ReadingStatus.Read);

            // Act
            var exception = Assert.Throws<DomainException>(
               () => book.Rate(rating));

            // Assert
            Assert.Contains("Should only be able to set rating in 1 to 5 range.", exception.Message);
        }

        [Theory(DisplayName = "Should throw erros when try to set rating out of 1 to 5 range")]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void SetRating_WithValidData(int rating)
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);
            book.ChangeStatus(ReadingStatus.Read);

            // Act
            book.Rate(rating);

            // Assert
            Assert.Equal(rating, book.Rating);
        }

        [Fact(DisplayName = "Should return rating null when Status change from Read to another")]
        public void ChangeStatus_ToNotRead_ClearsRating()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);
            book.ChangeStatus(ReadingStatus.Read);
            book.Rate(5);

            // Act
            book.ChangeStatus(ReadingStatus.WantToRead);

            // Assert
            Assert.Null(book.Rating);
        }

        [Fact(DisplayName = "Shouw throw error when pass a invalid value for status")]
        public void ChangeStatus_NoValidEnum_ThrowsDomainException()
        {
            // Arrange
            var beforeCreate = DateTime.UtcNow;
            var book = Book.Create(ValidOwnerId, ValidTitle, ValidAuthor);
            book.ChangeStatus(ReadingStatus.Read);
            book.Rate(5);

            // Act
            var exception = Assert.Throws<DomainException>(
               () => book.ChangeStatus((ReadingStatus)99));

            // Assert
            Assert.Contains("The provided enum value is not defined.", exception.Message);
        }
    }
}
