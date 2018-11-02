namespace Netpips.Media.Service
{
    public interface IArchiveExtractorService
    {
        bool HandleRarFile(string fsInfoFullName, out string destDir);
    }
}
