using DiscoSdk.Exceptions;
using DiscoSdk.Hosting.Rest.Clients;
using DiscoSdk.Rest;
using System.Net;

namespace DiscoSdk.Hosting.Tests.Rest.Clients;

/// <summary>
/// Covers the global error-mapping logic in <see cref="DiscordRestClient.BuildException"/>.
/// The contract: 403 + Discord codes 50001/50013 surface as <see cref="InsufficientPermissionException"/>;
/// every other failure stays as the generic <see cref="DiscordApiException"/>.
/// </summary>
public class DiscordRestClientExceptionMappingTests
{
	[Fact]
	public void BuildException_403WithCode50013_ReturnsInsufficientPermissionException()
	{
		var error = new DiscordApiError { Code = 50013, Message = "Missing Permissions" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.Forbidden, "Forbidden", error);

		var typed = Assert.IsType<InsufficientPermissionException>(ex);
		Assert.Equal(50013, typed.DiscordCode);
		Assert.Equal("Missing Permissions", typed.DiscordMessage);
		Assert.Null(typed.RequiredPermission);
		Assert.IsAssignableFrom<DiscordApiException>(typed);
	}

	[Fact]
	public void BuildException_403WithCode50001_ReturnsInsufficientPermissionException()
	{
		var error = new DiscordApiError { Code = 50001, Message = "Missing Access" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.Forbidden, "Forbidden", error);

		Assert.IsType<InsufficientPermissionException>(ex);
		Assert.Equal(50001, ex.DiscordCode);
	}

	[Fact]
	public void BuildException_403WithUnrelatedCode_ReturnsGenericDiscordApiException()
	{
		// 50005 = Cannot edit a message authored by another user. Not a permission failure.
		var error = new DiscordApiError { Code = 50005, Message = "Cannot edit a message authored by another user" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.Forbidden, "Forbidden", error);

		Assert.IsType<DiscordApiException>(ex);
		Assert.IsNotType<InsufficientPermissionException>(ex);
	}

	[Fact]
	public void BuildException_403WithoutDiscordError_ReturnsGenericDiscordApiException()
	{
		var ex = DiscordRestClient.BuildException(HttpStatusCode.Forbidden, "Forbidden", error: null);

		Assert.IsType<DiscordApiException>(ex);
	}

	[Fact]
	public void BuildException_404WithCode50013_ReturnsGenericDiscordApiException()
	{
		// 50013 code outside a 403 status — defensive: never seen in practice, but the mapping
		// is keyed on the status code too.
		var error = new DiscordApiError { Code = 50013, Message = "Missing Permissions" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.NotFound, "Not Found", error);

		Assert.IsType<DiscordApiException>(ex);
	}

	[Fact]
	public void BuildException_500ServerError_ReturnsGenericDiscordApiException()
	{
		var error = new DiscordApiError { Code = 0, Message = "Internal Server Error" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.InternalServerError, "Internal Server Error", error);

		Assert.IsType<DiscordApiException>(ex);
	}

	[Fact]
	public void InsufficientPermission_FromApiError_IsCatchableAsDiscordApiException()
	{
		var error = new DiscordApiError { Code = 50013, Message = "Missing Permissions" };
		var ex = DiscordRestClient.BuildException(HttpStatusCode.Forbidden, "Forbidden", error);

		var caughtAsBase = false;
		try
		{
			throw ex;
		}
		catch (DiscordApiException)
		{
			caughtAsBase = true;
		}

		Assert.True(caughtAsBase);
	}

	[Fact]
	public void InsufficientPermission_SdkPreflight_CarriesRequiredPermissionName()
	{
		var ex = InsufficientPermissionException.Operation("MANAGE_MESSAGES", "edit messages from other users");

		Assert.Equal("MANAGE_MESSAGES", ex.RequiredPermission);
		Assert.Equal(HttpStatusCode.Forbidden, ex.StatusCode);
		Assert.Contains("edit messages from other users", ex.Message);
		Assert.Contains("MANAGE_MESSAGES", ex.Message);
		Assert.IsAssignableFrom<DiscordApiException>(ex);
	}
}
