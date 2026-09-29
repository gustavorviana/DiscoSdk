namespace DiscoSdk.Tests;

public class DiscordLocalesTests
{
	[Theory]
	[InlineData("pt-BR")]
	[InlineData("es-ES")]
	[InlineData("es-419")]
	[InlineData("zh-TW")]
	public void Has_AcceptsDiscordLocales(string locale)
	{
		Assert.True(DiscordLocales.Has(locale));
	}

	[Fact]
	public void Has_RejectsUnknownLocale()
	{
		Assert.False(DiscordLocales.Has("pt-PT"));
	}
}
