using ReadingList.Application.Books;
using ReadingList.Application.Common.Results;
using ReadingList.Application.Tests.Fakes;
using ReadingList.Domain.Common.Enums;
using ReadingList.Domain.Entities;

namespace ReadingList.Application.Tests.Books
{
    public class BookServiceTests
    {
        private static readonly Guid OwnerId = Guid.NewGuid();
        private static readonly Guid OtherOwnerId = Guid.NewGuid();

        private readonly InMemoryBookRepository _repository = new();
        private readonly BookService _service;

        public BookServiceTests() => _service = new BookService(_repository);

        private Book SeedBook(Guid ownerId)
        {
            var book = Book.Create(ownerId, "The Silmarillion", "J.R.R Tolkien");
            _repository.AddAsync(book).GetAwaiter().GetResult();
            return book;
        }


        [Fact(DisplayName = "Shoud create a book when a valid data is provided and return it")]
        public async Task CreateAsync_WithValidRequest_PersistsAndReturnsBook()
        {
            //Arrange
            var request = new CreateBookRequest("Dune", "Frank Herbert");

            //Act
            var result = await _service.CreateAsync(OwnerId, request);


            //Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Dune", result.Value.Title);

            var saved = Assert.Single(_repository.Books);
            Assert.Equal(OwnerId, saved.OwnerId);
            Assert.Equal(saved.Id, result.Value.Id);
        }

        [Fact(DisplayName = "Shoud update a book when a valid data is provided and return it")]
        public async Task UpdateAsync_WithValidRequest_UpdateAndReturnsBook()
        {
            //Arrange
            var request = new UpdateBookRequest("Dune - Part One", "Frank Herbert", ReadingStatus.Reading, null);
            var book = SeedBook(OwnerId);

            //Act
            var result = await _service.UpdateAsync(OwnerId, book.Id, request);

            //Assert
            Assert.True(result.IsSuccess);

            var updated = Assert.Single(_repository.Books);
            Assert.Equal("Dune - Part One", updated.Title);
            Assert.Equal(ReadingStatus.Reading, updated.Status);
        }

        [Fact(DisplayName = "Shoud return not found error, when try to get another users book")]
        public async Task GetByIdAsync_WhenBookBelongsToAnotherUser_ReturnsNotFound()
        {
            //Arrange
            var book = SeedBook(OtherOwnerId);

            //Act
            var result = await _service.GetByIdAsync(OwnerId, book.Id);

            Assert.True(result.IsFailure);
            Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        }

        [Fact(DisplayName = "Shoud return not found error, when try to delete another users book")]
        public async Task DeleteAsync_WhenBookBelongsToAnotherUser_DoesNotDelete()
        {
            //Arrange
            var book = SeedBook(OtherOwnerId);

            //Act
            var result = await _service.DeleteAsync(OwnerId, book.Id);

            //Assert
            Assert.Equal(ErrorType.NotFound, result.Error!.Type);
            Assert.Single(_repository.Books);   // still there
        }
    }
}
