using Api.LibraryScan;

namespace Api.Tests;

public class EnumerateMp3FilesTests
{
    [Fact]
    public void EnumeratesOnlyMp3sRecursively()
    {
        var root = Directory.CreateTempSubdirectory();
        try
        {
            var d1 = Directory.CreateDirectory(Path.Combine(root.FullName, "a"));
            var d2 = Directory.CreateDirectory(Path.Combine(root.FullName, "b"));
            File.WriteAllText(Path.Combine(root.FullName, "root.mp3"), "");
            File.WriteAllText(Path.Combine(d1.FullName, "track1.MP3"), "");
            File.WriteAllText(Path.Combine(d1.FullName, "note.txt"), "");
            File.WriteAllText(Path.Combine(d2.FullName, "song.mp3"), "");

            var files = ScanHelpers.EnumerateMp3Files(root.FullName).ToArray();
            Assert.Contains(Path.Combine(root.FullName, "root.mp3"), files);
            Assert.Contains(Path.Combine(d1.FullName, "track1.MP3"), files);
            Assert.Contains(Path.Combine(d2.FullName, "song.mp3"), files);
            Assert.DoesNotContain(Path.Combine(d1.FullName, "note.txt"), files);
        }
        finally
        {
            root.Delete(true);
        }
    }
}

