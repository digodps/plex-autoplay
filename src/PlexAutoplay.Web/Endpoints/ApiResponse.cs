namespace PlexAutoplay.Web.Endpoints;

/// <summary>
/// Standard API response model used across endpoints.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the message describing the result.
    /// </script>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets additional data.
    /// </summary>
    public object? Data { get; set; }
}
