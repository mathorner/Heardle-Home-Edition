using Api.LibraryScan;
using System.Text.RegularExpressions;

namespace Api.Tests;

public class ScanHelpersTests
{
    [Fact]
    public void DeterministicId_Is32HexAndStable()
    {
        var p1 = "/music/A/B/C.mp3";
        var id1 = ScanHelpers.DeterministicIdFromPath(p1);
        var id2 = ScanHelpers.DeterministicIdFromPath(p1);
        Assert.Equal(id1, id2);
        Assert.Equal(32, id1.Length);
        Assert.Matches(new Regex("^[0-9a-f]{32}$"), id1);

        var p2 = "/music/other.mp3";
        var id3 = ScanHelpers.DeterministicIdFromPath(p2);
        Assert.NotEqual(id1, id3);
    }

    [Fact]
    public void ParseFromFilename_ArtistDashTitle()
    {
        var (artist, title) = ScanHelpers.ParseFromFilename("/x/Daft Punk - One More Time.mp3");
        Assert.Equal("Daft Punk", artist);
        Assert.Equal("One More Time", title);
    }

    [Fact]
    public void ParseFromFilename_TitleOnly()
    {
        var (artist, title) = ScanHelpers.ParseFromFilename("/x/Track01.mp3");
        Assert.Equal(string.Empty, artist);
        Assert.Equal("Track01", title);
    }

    [Fact]
    public void Normalize_CollapsesWhitespace()
    {
        var n = ScanHelpers.Normalize("  A  B\t C\nD   ");
        Assert.Equal("A B C D", n);
    }
}

