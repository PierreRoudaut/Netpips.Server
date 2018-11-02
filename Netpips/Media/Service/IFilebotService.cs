namespace Netpips.Media.Service {
    public interface IFilebotService {

        /// <summary>
        /// Fetch and create the subtitle file in the same location as the .paramref name="path"
        /// </summary>
        /// <param name="path"></param>
        /// <param name="srtPath">The full path of the create .paramref name="srtPath"</param>
        /// <param name="lang"></param>
        /// <returns></returns>
        bool GetSubtitles (string path, out string srtPath, string lang = "eng", bool nonStrict = false);

        /// <summary>
        /// Get the media location for the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="baseDestPath"></param>
        /// <param name="destPath"></param>
        /// <param name="db"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool TryRename (string path, string baseDestPath, out string destPath, string db = null, string action = "test");
    }
}