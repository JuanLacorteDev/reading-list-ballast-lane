using System.Net;
using System.Net.Http.Json;
using ReadingList.Api.Tests.Infrastructure;
using ReadingList.Application.Books;
using ReadingList.Domain.Common.Enums;

namespace ReadingList.Api.Tests.Books;

/// <summary>
/// End to end through both APIs: the token comes from the Identity API and is used on the Books API.
/// </summary>
public sealed class BookEndpointsTests : IAsyncLifetime
{
    private readonly ApiTestContext _ctx = new();
    private string _token = null!;

    public async Task InitializeAsync()
    {
        await _ctx.InitializeAsync();
        _token = await _ctx.RegisterAndLoginAsync("reader@example.com");
    }

    public Task DisposeAsync() => _ctx.DisposeAsync();

    private async Task<BookResponse> CreateBookAsync(string title = "Clean Code", string author = "Robert C. Martin", string? token = null)
    {
        var response = await _ctx.BooksClient(token ?? _token)
            .PostAsJsonAsync("/api/books", new CreateBookRequest(title, author), ApiTestContext.Json);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BookResponse>(ApiTestContext.Json))!;
    }

    [Theory]
    [InlineData("GET", "/api/books")]
    [InlineData("POST", "/api/books")]
    [InlineData("DELETE", "/api/books/6f9619ff-8b86-d011-b42d-00cf4fc964ff")]
    public async Task Endpoints_WithoutToken_Return401(string method, string url)
    {
        var response = await _ctx.BooksClient().SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201WithLocationAndDefaults()
    {
        var response = await _ctx.BooksClient(_token)
            .PostAsJsonAsync("/api/books", new CreateBookRequest("Clean Code", "Robert C. Martin"), ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var book = await response.Content.ReadFromJsonAsync<BookResponse>(ApiTestContext.Json);
        Assert.Equal(ReadingStatus.WantToRead, book!.Status);
        Assert.Null(book.Rating);
        Assert.EndsWith($"/api/books/{book.Id}", response.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_Returns400()
    {
        var response = await _ctx.BooksClient(_token)
            .PostAsJsonAsync("/api/books", new CreateBookRequest("   ", "Author"), ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_ReturnsCreatedBook()
    {
        var created = await CreateBookAsync();

        var book = await _ctx.BooksClient(_token).GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}", ApiTestContext.Json);

        Assert.Equal(created.Id, book!.Id);
        Assert.Equal("Clean Code", book.Title);
    }

    [Fact]
    public async Task GetById_UnknownId_Returns404()
    {
        var response = await _ctx.BooksClient(_token).GetAsync($"/api/books/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OtherUsersBook_IsInvisible_AndCannotBeChanged()
    {
        var mine = await CreateBookAsync();
        var otherToken = await _ctx.RegisterAndLoginAsync("other@example.com");
        var other = _ctx.BooksClient(otherToken);

        var get = await other.GetAsync($"/api/books/{mine.Id}");
        var put = await other.PutAsJsonAsync($"/api/books/{mine.Id}", new UpdateBookRequest("Hacked", "Hacker", ReadingStatus.Reading, null), ApiTestContext.Json);
        var delete = await other.DeleteAsync($"/api/books/{mine.Id}");
        var list = await other.GetFromJsonAsync<List<BookResponse>>("/api/books", ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Empty(list!);
    }

    [Fact]
    public async Task Update_ToReadWithRating_PersistsChanges()
    {
        var created = await CreateBookAsync();

        var response = await _ctx.BooksClient(_token).PutAsJsonAsync($"/api/books/{created.Id}", new UpdateBookRequest("Clean Code (2nd ed.)", "Robert C. Martin", ReadingStatus.Read, 5), ApiTestContext.Json);
        var reloaded = await _ctx.BooksClient(_token).GetFromJsonAsync<BookResponse>($"/api/books/{created.Id}", ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Clean Code (2nd ed.)", reloaded!.Title);
        Assert.Equal(ReadingStatus.Read, reloaded.Status);
        Assert.Equal(5, reloaded.Rating);
        Assert.NotNull(reloaded.UpdatedAt);
    }

    [Fact]
    public async Task Update_RatingOnUnreadBook_Returns400()
    {
        var created = await CreateBookAsync();

        var response = await _ctx.BooksClient(_token).PutAsJsonAsync($"/api/books/{created.Id}",
            new UpdateBookRequest("Clean Code", "Robert C. Martin", ReadingStatus.Reading, 4), ApiTestContext.Json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_WithStatus_FiltersBooks()
    {
        var read = await CreateBookAsync("Refactoring", "Martin Fowler");
        await CreateBookAsync("Clean Code", "Robert C. Martin");
        await _ctx.BooksClient(_token).PutAsJsonAsync($"/api/books/{read.Id}",
            new UpdateBookRequest(read.Title, read.Author, ReadingStatus.Read, 4), ApiTestContext.Json);

        var all = await _ctx.BooksClient(_token).GetFromJsonAsync<List<BookResponse>>("/api/books", ApiTestContext.Json);
        var onlyRead = await _ctx.BooksClient(_token).GetFromJsonAsync<List<BookResponse>>("/api/books?status=Read", ApiTestContext.Json);

        Assert.Equal(2, all!.Count);
        var single = Assert.Single(onlyRead!);
        Assert.Equal(read.Id, single.Id);
    }

    [Fact]
    public async Task Delete_Returns204_AndBookIsGone()
    {
        var created = await CreateBookAsync();

        var delete = await _ctx.BooksClient(_token).DeleteAsync($"/api/books/{created.Id}");
        var get = await _ctx.BooksClient(_token).GetAsync($"/api/books/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }
}
