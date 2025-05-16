# BlueSky .NET Client Library Development Instructions

## Project Overview

This repository contains a .NET client library for interacting with the BlueSky social media platform API. The library should provide a clean, strongly-typed interface for .NET applications to integrate with BlueSky.

## API Documentation Reference

The BlueSky API (AT Protocol) documentation can be found at:

- Main documentation: https://atproto.com/docs
- API reference: https://docs.bsky.app/docs/api-guide
- HTTP Reference: https://docs.bsky.app/docs/api/app-bsky-actor-get-preferences
- Creating a post: https://docs.bsky.app/docs/api/com-atproto-repo-create-record
- Lexicon specifications: https://github.com/bluesky-social/atproto/tree/main/lexicons

## Core Requirements

1. **Target .NET Version**: .NET 9.0
2. **Architecture**:

   - Use a clean, modular design pattern
   - Implement proper dependency injection
   - Separate models from service implementations
   - Use interfaces for services to allow mocking in tests

3. **Authentication**:

   - Support session-based authentication
   - Handle token refresh automatically
   - Securely store credentials

4. **API Coverage**:

   - User authentication (create session, refresh session)
   - Post creation, fetching, and deletion
   - Timeline operations (home timeline, author timeline)
   - User profile operations (get profile, update profile)
   - Social graph operations (follow, unfollow, block, mute)
   - Likes and reposts
   - Media upload support

5. **Error Handling**:
   - Create custom exception types for BlueSky API errors
   - Provide meaningful error messages
   - Implement proper retry logic for transient failures

## Code Structure

- `src/` - Source code
  - `HexMaster.BlueSky.Client/` - Core client library
  - `HexMaster.BlueSky.Client.Abstractions/` - Service absractions, models and custom exceptions
  - `HexMaster.BlueSky.Client.Tests/` - Unit tests
  - `HexMaster.BlueSky.Integration.Tests/` - Integration tests

## Naming Conventions

- Use PascalCase for class names and public members
- Use camelCase for local variables and parameters
- Prefix interfaces with "I"
- Suffix exception classes with "Exception"
- Suffix service interfaces with "Service"
- Suffix implementations with "Implementation" or nothing

## Implementation Details

### HTTP Layer

- Use `HttpClient` with proper lifecycle management
- Implement retry with the Microsoft.Extensions.Http.Resilience package
- Support request/response logging (with sensitive info redaction)

Documentation for the Microsoft HTTP Resilience package is here: https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli

### Serialization

- Use System.Text.Json for serialization/deserialization
- Implement custom converters for BlueSky-specific data types
- Handle date formats properly (UTC)

### Authentication Flow

1. Create client with application credentials
2. Obtain session via username/password or app password
3. Store session information
4. Refresh automatically when needed

### Core Models

The library should include models for:

- Users and profiles
- Posts/feed items
- Media attachments
- Likes and interactions
- Social graph relationships

## Testing Requirements

- Write comprehensive unit tests for all services
- Mock HTTP responses for testing
- Create integration tests for key flows
- Aim for >80% code coverage

## Documentation

- Use XML documentation comments on all public APIs
- Create usage examples for common scenarios
- Document rate limits and best practices

## Example Usage (Design Goal)

```csharp
// Client initialization
var client = new BlueSkyClient(new BlueSkyClientOptions
{
    // Configuration options
});

// Authentication
await client.CreateSessionAsync("username", "password");

// Post a message
await client.Posts.CreateAsync(new CreatePostRequest
{
    Text = "Hello from BlueSky .NET Client!"
});

// Get timeline
var timeline = await client.Timeline.GetHomeTimelineAsync();
foreach (var post in timeline.Posts)
{
    Console.WriteLine($"{post.Author.DisplayName}: {post.Text}");
}
```
