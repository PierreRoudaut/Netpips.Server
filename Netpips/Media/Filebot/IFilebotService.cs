namespace Netpips.Media.Filebot
{
    public interface IFilebotService
    {
        /// <summary>
        /// Fetch and create the subtitle file in the same location as the .paramref name="path"
        /// </summary>
        /// <param name="path"></param>
        /// <param name="srtPath">The full path of the create .paramref name="srtPath"</param>
        /// <param name="lang"></param>
        /// <returns></returns>
        bool GetSubtitles(string path, out string srtPath, string lang = "eng", bool nonStrict = false);

        /// <summary>
        /// Guess the Plex standardized path for a local video file
        /// Input: 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        RenameResult Rename(RenameRequest request);
    }
}