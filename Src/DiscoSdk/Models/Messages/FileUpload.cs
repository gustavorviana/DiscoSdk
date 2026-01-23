namespace DiscoSdk.Models.Messages;

/// <summary>
/// Represents a file to be uploaded with a message.
/// </summary>
public class FileUpload
{
	/// <summary>
	/// Gets or sets the file name.
	/// </summary>
	public string FileName { get; set; } = default!;

	/// <summary>
	/// Gets or sets the file content.
	/// </summary>
	public byte[] Content { get; set; } = default!;

	/// <summary>
	/// Gets or sets the content type of the file.
	/// </summary>
	public string? ContentType { get; set; }
}

