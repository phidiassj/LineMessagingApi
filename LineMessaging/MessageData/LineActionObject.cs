using Newtonsoft.Json;

namespace LineMessaging
{
    public interface ILineAction
    {
        [JsonProperty("type")]
        ActionType Type { get; }
    }

    public class PostbackAction : ILineAction
    {
        [JsonProperty("type")]
        public ActionType Type => ActionType.Postback;
        [JsonProperty("label")]
        public string Label { get; set; } = null!;
        /// <summary>
        /// String returned via webhook in the postback.data property of the postback event <para />
        /// Max character limit: 300
        /// </summary>
        [JsonProperty("data")]
        public string Data { get; set; } = null!;
        /// <summary>
        /// Text displayed in the chat as a message sent by the user when the action is performed. <para />
        /// Required for quick reply buttons. Optional for the other message types. <para />
        /// Max character limit: 300 <para />
        /// The displayText and text properties cannot both be used at the same time. 
        /// </summary>
        [JsonProperty("displayText")]
        public string DisplayText { get; set; } = null!;
        /// <summary>
        /// The display method of such as rich menu based on user action. Specify one of the following values: <para />
        /// closeRichMenu: Close rich menu <para />
        /// openRichMenu: Open rich menu <para />
        /// openKeyboard: Open keyboard <para />
        /// openVoice: Open voice message input mode
        /// </summary>
        [JsonProperty("inputOption")]
        public string InputOption { get; set; }
        /// <summary>
        /// String to be pre-filled in the input field when the keyboard is opened. <para />
        /// Valid only when the inputOption property is set to openKeyboard. <para />
        /// The string can be broken by a newline character (\n).
        /// </summary>
        [JsonProperty("fillInText")]
        public string FillInText { get; set; }
    }

    public class MessageAction : ILineAction
    {
        [JsonProperty("type")]
        public ActionType Type => ActionType.Message;
        [JsonProperty("label")]
        public string Label { get; set; } = null!;
        /// <summary>
        /// Text sent when the action is performed. <para />
        /// Max character limit: 300
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; } = null!;
    }

    public class UriAction : ILineAction
    {
        [JsonProperty("type")]
        public ActionType Type => ActionType.Uri;
        [JsonProperty("label")]
        public string Label { get; set; } = null!;
        /// <summary>
        /// URI opened when the action is performed (Max character limit: 1000) <para />
        /// The available schemes are http, https, line, and tel. For more information about the LINE URL scheme, see: <para />
        /// https://developers.line.biz/en/docs/messaging-api/using-line-url-scheme/
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; } = null!;

    }

    public class DatetimepickerAction : ILineAction
    {
        [JsonProperty("type")]
        public ActionType Type => ActionType.Datetimepicker;
        [JsonProperty("label")]
        public string Label { get; set; } = null!;
        [JsonProperty("data")]
        public string Data { get; set; } = null!;
        /// <summary>
        /// Action mode <para />
        /// date: Pick date <para />
        /// time: Pick time <para />
        /// datetime: Pick date and time
        /// </summary>
        [JsonProperty("mode")]
        public string Mode { get; set; } = null!;
        /// <summary>
        /// Initial value of date or time
        /// </summary>
        [JsonProperty("initial")]
        public string Initial { get; set; } = null!;
        /// <summary>
        /// Largest date or time value that can be selected. Must be greater than the min value.
        /// </summary>
        [JsonProperty("max")]
        public string Max { get; set; } = null!;
        /// <summary>
        /// Smallest date or time value that can be selected. Must be less than the max value.
        /// </summary>
        [JsonProperty("min")]
        public string Min { get; set; } = null!;
    }
}
