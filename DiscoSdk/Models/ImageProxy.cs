namespace DiscoSdk.Models;

/// <summary>
/// Represents a proxy for an image URL.
/// </summary>
public class ImageProxy
{
	private readonly string _url;

	/// <summary>
	/// Initializes a new instance of the <see cref="ImageProxy"/> class.
	/// </summary>
	/// <param name="url">The image URL.</param>
	public ImageProxy(string url)
	{
		_url = url ?? throw new ArgumentNullException(nameof(url));
	}

	/// <summary>
	/// Gets the image URL.
	/// </summary>
	public string Url => _url;
}

