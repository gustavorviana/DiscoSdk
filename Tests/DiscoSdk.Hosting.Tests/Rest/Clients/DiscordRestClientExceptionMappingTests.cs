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

	// --- InvalidTokenException (401 + 40001/50014) ----------------------------------------

	[Theory]
	[InlineData(40001, "Unauthorized")]
	[InlineData(50014, "Invalid authentication token provided")]
	public void BuildException_401WithTokenCode_ReturnsInvalidTokenException(int code, string message)
	{
		var error = new DiscordApiError { Code = code, Message = message };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.Unauthorized, "Unauthorized", error);

		var typed = Assert.IsType<InvalidTokenException>(ex);
		Assert.Equal(code, typed.DiscordCode);
		Assert.Equal(message, typed.DiscordMessage);
		Assert.IsAssignableFrom<DiscordApiException>(typed);
	}

	[Fact]
	public void BuildException_401WithUnrelatedCode_ReturnsGenericDiscordApiException()
	{
		var error = new DiscordApiError { Code = 0, Message = "401 Unauthorized" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.Unauthorized, "Unauthorized", error);

		Assert.IsType<DiscordApiException>(ex);
		Assert.IsNotType<InvalidTokenException>(ex);
	}

	// --- DiscordResourceNotFoundException (404 + 10001..10068) ----------------------------

	[Theory]
	[InlineData(10003, "Unknown Channel")]
	[InlineData(10008, "Unknown Message")]
	[InlineData(10013, "Unknown User")]
	[InlineData(10001, "Unknown Account")]
	[InlineData(10068, "Unknown Voice State")]
	public void BuildException_404WithUnknownResourceCode_ReturnsResourceNotFoundException(int code, string message)
	{
		var error = new DiscordApiError { Code = code, Message = message };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.NotFound, "Not Found", error);

		var typed = Assert.IsType<DiscordResourceNotFoundException>(ex);
		Assert.Equal(code, typed.DiscordCode);
		Assert.IsAssignableFrom<DiscordApiException>(typed);
	}

	[Fact]
	public void BuildException_404WithoutDiscordError_ReturnsGenericDiscordApiException()
	{
		var ex = DiscordRestClient.BuildException(HttpStatusCode.NotFound, "Not Found", error: null);

		Assert.IsType<DiscordApiException>(ex);
	}

	[Fact]
	public void BuildException_404WithCodeOutsideUnknownRange_ReturnsGenericDiscordApiException()
	{
		var error = new DiscordApiError { Code = 20012, Message = "Not authorized" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.NotFound, "Not Found", error);

		Assert.IsType<DiscordApiException>(ex);
		Assert.IsNotType<DiscordResourceNotFoundException>(ex);
	}

	[Fact]
	public void BuildException_200WithUnknownResourceCode_ReturnsGenericDiscordApiException()
	{
		// Defensive: code 10003 outside a 404 status should not flip the type. Never observed in
		// practice, but the mapping is keyed on the status code too.
		var error = new DiscordApiError { Code = 10003, Message = "Unknown Channel" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.OK, "OK", error);

		Assert.IsType<DiscordApiException>(ex);
	}

	// --- InvalidRequestBodyException (400 + 50035) ----------------------------------------

	[Fact]
	public void BuildException_400WithCode50035_ReturnsInvalidRequestBodyException()
	{
		var validation = new DiscordValidationError
		{
			Name = "name",
			Messages = ["Must be 1-100 characters."],
		};
		var error = new DiscordApiError
		{
			Code = 50035,
			Message = "Invalid Form Body",
			ValidationErrors = [validation],
		};

		var ex = DiscordRestClient.BuildException(HttpStatusCode.BadRequest, "Bad Request", error);

		var typed = Assert.IsType<InvalidRequestBodyException>(ex);
		Assert.Equal(50035, typed.DiscordCode);
		Assert.Single(typed.ValidationErrors);
		Assert.Equal("name", typed.ValidationErrors[0].Name);
		Assert.IsAssignableFrom<DiscordApiException>(typed);
	}

	[Fact]
	public void BuildException_400WithDifferentCode_ReturnsGenericDiscordApiException()
	{
		var error = new DiscordApiError { Code = 50006, Message = "Cannot send an empty message" };

		var ex = DiscordRestClient.BuildException(HttpStatusCode.BadRequest, "Bad Request", error);

		Assert.IsType<DiscordApiException>(ex);
		Assert.IsNotType<InvalidRequestBodyException>(ex);
	}

	// --- Hierarchy contract ----------------------------------------------------------------

	[Theory]
	[InlineData(50013, HttpStatusCode.Forbidden, typeof(InsufficientPermissionException))]
	[InlineData(40001, HttpStatusCode.Unauthorized, typeof(InvalidTokenException))]
	[InlineData(10003, HttpStatusCode.NotFound, typeof(DiscordResourceNotFoundException))]
	[InlineData(50035, HttpStatusCode.BadRequest, typeof(InvalidRequestBodyException))]
	public void EveryTypedException_IsCatchableAsDiscordApiException(int code, HttpStatusCode status, Type expectedType)
	{
		var error = new DiscordApiError { Code = code, Message = "test" };
		var ex = DiscordRestClient.BuildException(status, status.ToString(), error);

		Assert.IsType(expectedType, ex);

		var caughtAsBase = false;
		try { throw ex; }
		catch (DiscordApiException) { caughtAsBase = true; }

		Assert.True(caughtAsBase);
	}
}
