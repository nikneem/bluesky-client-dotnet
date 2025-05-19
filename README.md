# BlueSky .NET Client Library

A .NET client library for interacting with the BlueSky social media platform API.

## Features

- Authentication with BlueSky API
- Creating and managing posts
- Support for rich text features (mentions, hashtags, links)
- Media upload and attachment
- API error handling and rate limiting

## Installation

```bash
dotnet add package HexMaster.BlueSky.Client
```

## Getting Started

### Basic Usage

```csharp
// Create a client instance
var client = new BlueSkyClient();

// Authenticate with BlueSky
await client.CreateSessionAsync("your-username", "your-password");

// Create a simple text post
await client.Posts.CreateTextPostAsync("Hello from BlueSky .NET Client!");
```

### Dependency Injection

```csharp
// In your Startup.cs or Program.cs
services.AddBlueSkyClient(options =>
{
    options.BaseUrl = "https://bsky.social";
    options.EnableLogging = true;
});

// In your service class
public class YourService
{
    private readonly BlueSkyClient _client;

    public YourService(BlueSkyClient client)
    {
        _client = client;
    }

    public async Task PostMessage(string message)
    {
        await _client.Posts.CreateTextPostAsync(message);
    }
}
```

## Examples

### Authentication

```csharp
// Create a client
var client = new BlueSkyClient();

// Authenticate
await client.CreateSessionAsync("your-username", "your-password");

// Get the current session information
var session = client.Authentication.GetCurrentSession();
Console.WriteLine($"Authenticated as: {session.Handle}");
```

### Creating Posts

```csharp
// Create a simple text post
await client.Posts.CreateTextPostAsync("Hello from BlueSky .NET Client!");

// Create a post with mentions and hashtags
await client.Posts.CreateTextPostAsync("Hello @friend! Check out this #dotnet library!");

// Create a post with a link
await client.Posts.CreateTextPostAsync("Check out this cool website: https://example.com");
```

### Uploading and Attaching Media

```csharp
// Upload an image and create a post with it
using var imageStream = File.OpenRead("image.jpg");
await client.Media.CreatePostWithImageAsync(
    "Check out this cool image!",
    imageStream,
    "image/jpeg",
    "A description of the image for accessibility"
);

// Upload multiple images and create a post
var images = new List<ImageUpload>
{
    new ImageUpload
    {
        Content = File.OpenRead("image1.jpg"),
        ContentType = "image/jpeg",
        AltText = "Description of image 1"
    },
    new ImageUpload
    {
        Content = File.OpenRead("image2.jpg"),
        ContentType = "image/jpeg",
        AltText = "Description of image 2"
    }
};

await client.Media.CreatePostWithMultipleImagesAsync(
    "Check out these cool images!",
    images
);
```

## Advanced Configuration

```csharp
var options = new BlueSkyClientOptions
{
    BaseUrl = "https://bsky.social",
    TimeoutSeconds = 60,
    AutoRefreshTokens = true,
    MaxRetries = 5,
    RetryInitialDelayMs = 1000,
    RetryMaxDelayMs = 30000,
    EnableLogging = true
};

var client = new BlueSkyClient(options);
```

## Error Handling

```csharp
try
{
    await client.Posts.CreateTextPostAsync("Hello from BlueSky .NET Client!");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication error: {ex.Message}");
}
catch (RateLimitException ex)
{
    Console.WriteLine($"Rate limit exceeded. Retry after {ex.RetryAfterSeconds} seconds.");
}
catch (BlueSkyException ex)
{
    Console.WriteLine($"BlueSky API error: {ex.Message}, Status: {ex.StatusCode}, Error Code: {ex.ErrorCode}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## License

MIT
