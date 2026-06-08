using DiscoSdk.Models;

namespace DiscoSdk.Tests.Models;

public class DiscordSoundBufferTests
{
    [Fact]
    public void Ctor_DetectsOggFromMagicHeader()
    {
        // "OggS" container header.
        var buffer = new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x00, 0x02 };

        var sound = new DiscordSoundBuffer(buffer);

        Assert.Equal("audio/ogg", sound.MimeType);
        Assert.Same(buffer, sound.Buffer);
    }

    [Fact]
    public void Ctor_DetectsMp3FromId3Header()
    {
        // "ID3" tag at offset 0.
        var buffer = new byte[] { 0x49, 0x44, 0x33, 0x03, 0x00, 0x00 };

        var sound = new DiscordSoundBuffer(buffer);

        Assert.Equal("audio/mpeg", sound.MimeType);
    }

    [Fact]
    public void Ctor_DetectsMp3FromRawSyncWord()
    {
        // MPEG audio sync word 0xFFFB (MPEG1 Layer III).
        var buffer = new byte[] { 0xFF, 0xFB, 0x90, 0x44 };

        var sound = new DiscordSoundBuffer(buffer);

        Assert.Equal("audio/mpeg", sound.MimeType);
    }

    [Fact]
    public void Ctor_ExplicitMime_BypassesDetection()
    {
        var buffer = new byte[] { 0x00, 0x01, 0x02 };

        var sound = new DiscordSoundBuffer(buffer, "audio/wav");

        Assert.Equal("audio/wav", sound.MimeType);
    }

    [Fact]
    public void Ctor_EmptyBuffer_Throws()
    {
        Assert.Throws<ArgumentException>(() => new DiscordSoundBuffer([]));
    }

    [Fact]
    public void Ctor_NullBuffer_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new DiscordSoundBuffer(null!));
    }

    [Fact]
    public void Ctor_OverSizeCap_Throws()
    {
        var oversized = new byte[DiscordSoundBuffer.MaxSizeBytes + 1];
        oversized[0] = 0x4F;
        oversized[1] = 0x67;
        oversized[2] = 0x67;
        oversized[3] = 0x53;

        Assert.Throws<ArgumentException>(() => new DiscordSoundBuffer(oversized));
    }

    [Fact]
    public void Ctor_UnknownFormat_Throws()
    {
        var buffer = new byte[] { 0x00, 0x01, 0x02, 0x03 };

        Assert.Throws<InvalidOperationException>(() => new DiscordSoundBuffer(buffer));
    }

    [Fact]
    public void ToBase64_RoundTrips()
    {
        var buffer = new byte[] { 0x4F, 0x67, 0x67, 0x53, 0x01, 0x02 };
        var sound = new DiscordSoundBuffer(buffer);

        var base64 = sound.ToBase64();

        Assert.Equal(Convert.ToBase64String(buffer), base64);
    }

    [Fact]
    public void ToDataUri_BuildsRfc2397DataUri()
    {
        var buffer = new byte[] { 0x4F, 0x67, 0x67, 0x53 };
        var sound = new DiscordSoundBuffer(buffer);

        var uri = sound.ToDataUri();

        Assert.Equal($"data:audio/ogg;base64,{Convert.ToBase64String(buffer)}", uri);
    }

    [Fact]
    public void LoadFile_ReadsBytesFromDisk()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, [0x4F, 0x67, 0x67, 0x53, 0x00]);

            var sound = DiscordSoundBuffer.LoadFile(tempFile);

            Assert.Equal("audio/ogg", sound.MimeType);
            Assert.Equal(5, sound.Buffer.Length);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}
