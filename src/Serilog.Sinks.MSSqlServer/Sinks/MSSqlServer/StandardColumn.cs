namespace Serilog.Sinks.MSSqlServer
{
    /// <summary>
    ///     List of columns that are available to be written to the database, excluding Id and additional columns.
    /// </summary>
    public enum StandardColumn
    {
        /// <summary>
        /// The optional primary key
        /// </summary>
        Id,

        /// <summary>
        /// The message rendered with the template given the properties associated with the event.
        /// </summary>
        Message,

        /// <summary>
        /// The message template describing the event.
        /// </summary>
        MessageTemplate,

        /// <summary>
        /// The level of the event.
        /// </summary>
        Level,

        /// <summary>
        /// The time at which the event occurred.
        /// </summary>
        TimeStamp,

        /// <summary>
        /// An exception associated with the event, or null.
        /// </summary>
        Exception,

        /// <summary>
        /// Properties associated with the event, including those presented in <see cref="MessageTemplate"/>.
        /// </summary>
        Properties,

        /// <summary>
        /// A log event.
        /// </summary>
        LogEvent
    }
}
