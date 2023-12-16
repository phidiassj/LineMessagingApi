using System;
using System.Threading.Tasks;

namespace LineMessaging
{
    public partial class LineMessagingClient
    {
        private const string MessageContentApiPath = "/v2/bot/message/{0}/content";

        /// <summary>
        /// Get Line message content and save to local path. Determine file extension based on content-type.<para />
        /// Create by phidiassj.
        /// </summary>
        /// <param name="messageId">Line message id</param>
        /// <param name="path">Local path to write</param>
        /// <param name="filename">Filename without extension</param>
        /// <returns>Return full filename with path if success. Otherwise null.</returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<string> GetMessageContentAndSave(string messageId, string path, string filename)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException($"{nameof(messageId)} is null or empty.");
            }
            return await GetContentAndSave(string.Format(MessageContentApiPath, messageId), path, filename);
        }

        public Task<byte[]> GetMessageContent(string messageId)
        {
            if (string.IsNullOrEmpty(messageId))
            {
                throw new ArgumentException($"{nameof(messageId)} is null or empty.");
            }

            return GetAsByteArray(string.Format(MessageContentApiPath, messageId));
        }
    }
}
