using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using API.Tests.Infrastructure;
using Domain.Enums;

namespace API.Tests;

/// <summary>
/// One image rule for every resource: a new file replaces (and deletes) the old one,
/// removeImage clears it, otherwise the current image is kept. Only real jpg/png/webp ≤ 5 MB.
/// </summary>
public class ImageTests(ApiFactory api)
{
    // A real 1x1 PNG.
    private static readonly byte[] Png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");

    private sealed record Resource(string Url, string ImageField, Func<MultipartFormDataContent> Form);

    private async Task<Resource> ResourceAsync(string name, HttpClient client)
    {
        switch (name)
        {
            case "therapists":
                return new("/api/therapists", "image", () => Http.Form(
                    ("Name", Http.Unique("T")), ("Bio", "Bio"), ("Experience", "1"), ("Education", "E")));
            case "services":
                var categoryId = await client.CreateAndGetId("/api/categories", new { name = Http.Unique("Cat") });
                return new("/api/services", "image", () => Http.Form(
                    ("Name", Http.Unique("S")), ("Description", "D"), ("Price", "10"), ("Currency", "BRL"),
                    ("category", JsonSerializer.Serialize(new { id = categoryId }))));
            case "courses":
                return new("/api/courses", "image", () => Http.Form(
                    ("Title", Http.Unique("C")), ("Description", "D"), ("Instructor", "Ana"), ("Type", "Curso"), ("Status", "Disponivel")));
            case "testimonials":
                return new("/api/testimonials", "authorAvatarUrl", () => Http.Form(
                    ("AuthorName", Http.Unique("A")), ("Rating", "5"), ("Content", "Great")));
            default:
                throw new ArgumentOutOfRangeException(nameof(name));
        }
    }

    private static MultipartFormDataContent WithFile(MultipartFormDataContent form, byte[] bytes, string fileName, string contentType = "image/png")
    {
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(file, "image", fileName);
        return form;
    }

    private static string? ImageOf(JsonElement item, Resource resource) =>
        item.GetProperty(resource.ImageField).GetString();

    private async Task<(Guid Id, string ImageUrl)> CreateWithImage(HttpClient client, Resource resource)
    {
        var created = await (await client.PostAsync(resource.Url, WithFile(resource.Form(), Png, "photo.png"))).Json();
        return (created.Id(), ImageOf(created, resource)!);
    }

    [Theory]
    [InlineData("therapists")]
    [InlineData("services")]
    [InlineData("courses")]
    [InlineData("testimonials")]
    public async Task Uploaded_image_is_stored_and_served(string name)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync(name, client);

        var (_, imageUrl) = await CreateWithImage(client, resource);

        Assert.StartsWith($"{ApiFactory.UploadsBaseUrl}/{name}/", imageUrl);
        Assert.EndsWith(".png", imageUrl);
        var served = await api.CreateClient().GetAsync(imageUrl);
        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal(Png, await served.Content.ReadAsByteArrayAsync());
    }

    [Theory]
    [InlineData("therapists")]
    [InlineData("services")]
    [InlineData("courses")]
    [InlineData("testimonials")]
    public async Task Editing_without_a_file_keeps_the_image(string name)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync(name, client);
        var (id, imageUrl) = await CreateWithImage(client, resource);

        var update = await client.PutAsync($"{resource.Url}/{id}", resource.Form());

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var item = await api.CreateClient().GetJson($"{resource.Url}/{id}");
        Assert.Equal(imageUrl, ImageOf(item, resource));
    }

    [Theory]
    [InlineData("therapists")]
    [InlineData("services")]
    [InlineData("courses")]
    [InlineData("testimonials")]
    public async Task Replacing_the_image_deletes_the_old_file(string name)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync(name, client);
        var (id, oldUrl) = await CreateWithImage(client, resource);

        var update = await client.PutAsync($"{resource.Url}/{id}", WithFile(resource.Form(), Png, "new.png"));

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        var newUrl = ImageOf(await api.CreateClient().GetJson($"{resource.Url}/{id}"), resource);
        Assert.NotEqual(oldUrl, newUrl);
        Assert.False(File.Exists(api.UploadedFilePath(oldUrl)), "old file should be deleted");
        Assert.True(File.Exists(api.UploadedFilePath(newUrl!)), "new file should exist");
    }

    [Fact]
    public async Task Remove_image_clears_it_and_deletes_the_file()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync("therapists", client);
        var (id, oldUrl) = await CreateWithImage(client, resource);
        var form = resource.Form();
        form.Add(new StringContent("true"), "removeImage");

        var update = await client.PutAsync($"{resource.Url}/{id}", form);

        Assert.Equal(HttpStatusCode.NoContent, update.StatusCode);
        Assert.Null(ImageOf(await api.CreateClient().GetJson($"{resource.Url}/{id}"), resource));
        Assert.False(File.Exists(api.UploadedFilePath(oldUrl)));
    }

    [Theory]
    [InlineData("notes.txt", "text/plain")]
    [InlineData("animation.gif", "image/gif")]
    public async Task Unsupported_file_types_are_rejected(string fileName, string contentType)
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync("therapists", client);

        var response = await client.PostAsync(resource.Url, WithFile(resource.Form(), Png, fileName, contentType));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task A_file_whose_content_is_not_an_image_is_rejected_even_with_an_image_name()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync("therapists", client);

        var response = await client.PostAsync(resource.Url,
            WithFile(resource.Form(), "<html><script>alert(1)</script></html>"u8.ToArray(), "photo.png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Images_larger_than_5_MB_are_rejected()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync("therapists", client);
        var big = new byte[5 * 1024 * 1024 + 1];
        Png.CopyTo(big, 0);

        var response = await client.PostAsync(resource.Url, WithFile(resource.Form(), big, "big.png"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Testimonial_without_a_photo_is_allowed()
    {
        var client = await api.ClientAsAsync(UserRole.Admin);
        var resource = await ResourceAsync("testimonials", client);

        var created = await (await client.PostAsync(resource.Url, resource.Form())).Json();

        Assert.Null(ImageOf(created, resource));
    }
}
