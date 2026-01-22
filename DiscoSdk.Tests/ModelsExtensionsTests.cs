using DiscoSdk.Models.Enums;

namespace DiscoSdk.Tests;

public class ModelsExtensionsTests
{
	[Fact]
	public void HasPermission_WithAdministrator_ReturnsTrue()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.Administrator);
		var permission = DiscordPermission.SendMessages;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void HasPermission_WithAdministrator_ReturnsTrueForAnyPermission()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.Administrator);

		// Act & Assert
		Assert.True(permissible.HasPermission(DiscordPermission.None));
		Assert.True(permissible.HasPermission(DiscordPermission.SendMessages));
		Assert.True(permissible.HasPermission(DiscordPermission.ManageChannels));
		Assert.True(permissible.HasPermission(DiscordPermission.KickMembers));
		Assert.True(permissible.HasPermission(DiscordPermission.BanMembers));
	}

	[Fact]
	public void HasPermission_WithExactPermission_ReturnsTrue()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.SendMessages);
		var permission = DiscordPermission.SendMessages;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void HasPermission_WithMultiplePermissionsIncludingRequested_ReturnsTrue()
	{
		// Arrange
		var permissions = DiscordPermission.SendMessages | DiscordPermission.ReadMessageHistory | DiscordPermission.EmbedLinks;
		var permissible = new MockPermissible(permissions);
		var permission = DiscordPermission.SendMessages;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.True(result);
	}

	[Fact]
	public void HasPermission_WithMultiplePermissionsNotIncludingRequested_ReturnsFalse()
	{
		// Arrange
		var permissions = DiscordPermission.SendMessages | DiscordPermission.ReadMessageHistory;
		var permissible = new MockPermissible(permissions);
		var permission = DiscordPermission.EmbedLinks;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void HasPermission_WithNoPermissions_ReturnsFalse()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.None);
		var permission = DiscordPermission.SendMessages;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void HasPermission_WithNonePermission_ReturnsTrueWhenHasNone()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.None);
		var permission = DiscordPermission.None;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void HasPermission_WithNonePermission_ReturnsFalseWhenHasOtherPermissions()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.SendMessages);
		var permission = DiscordPermission.None;

		// Act
		var result = permissible.HasPermission(permission);

		// Assert
		Assert.False(result);
	}

	[Fact]
	public void HasPermission_WithComplexPermissionSet_ReturnsCorrectResults()
	{
		// Arrange
		var permissions = DiscordPermission.SendMessages |
			DiscordPermission.ReadMessageHistory |
			DiscordPermission.EmbedLinks |
			DiscordPermission.AttachFiles;
		var permissible = new MockPermissible(permissions);

		// Act & Assert
		Assert.True(permissible.HasPermission(DiscordPermission.SendMessages));
		Assert.True(permissible.HasPermission(DiscordPermission.ReadMessageHistory));
		Assert.True(permissible.HasPermission(DiscordPermission.EmbedLinks));
		Assert.True(permissible.HasPermission(DiscordPermission.AttachFiles));
		Assert.False(permissible.HasPermission(DiscordPermission.ManageChannels));
		Assert.False(permissible.HasPermission(DiscordPermission.ManageMessages));
		Assert.False(permissible.HasPermission(DiscordPermission.KickMembers));
	}

	[Fact]
	public void HasPermission_WithAllPermissions_ReturnsTrueForAny()
	{
		// Arrange
		var allPermissions = (DiscordPermission)ulong.MaxValue;
		var permissible = new MockPermissible(allPermissions);

		// Act & Assert
		Assert.True(permissible.HasPermission(DiscordPermission.SendMessages));
		Assert.True(permissible.HasPermission(DiscordPermission.ManageChannels));
		Assert.True(permissible.HasPermission(DiscordPermission.KickMembers));
		Assert.True(permissible.HasPermission(DiscordPermission.BanMembers));
		Assert.True(permissible.HasPermission(DiscordPermission.Administrator));
	}

	[Fact]
	public void HasPermission_WithSingleBitPermission_WorksCorrectly()
	{
		// Arrange
		var permissible = new MockPermissible(DiscordPermission.ViewChannel);

		// Act & Assert
		Assert.True(permissible.HasPermission(DiscordPermission.ViewChannel));
		Assert.False(permissible.HasPermission(DiscordPermission.SendMessages));
	}

	private class MockPermissible : IPermissible
	{
		public DiscordPermission Permissions { get; }

		public MockPermissible(DiscordPermission permissions)
		{
			Permissions = permissions;
		}
	}
}

