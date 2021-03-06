﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpRaven;
using SharpRaven.Data;
using Vima.LoggingAbstractor.Core;
using Vima.LoggingAbstractor.Core.Extensions;
using Vima.LoggingAbstractor.Core.Parameters;

namespace Vima.LoggingAbstractor.Sentry
{
    /// <summary>
    /// Represents an instance of a Sentry logger.
    /// </summary>
    /// <seealso cref="AbstractLoggerBase" />
    /// <seealso cref="ISentryAbstractLogger" />
    public class RavenAbstractLogger : AbstractLoggerBase, ISentryAbstractLogger
    {
        private readonly RavenClient _ravenClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenAbstractLogger"/> class.
        /// </summary>
        /// <param name="ravenClient">The raven client.</param>
        /// <param name="minimalLoggingLevel">The minimal logging level.</param>
        public RavenAbstractLogger(RavenClient ravenClient, LoggingLevel minimalLoggingLevel = LoggingLevel.Verbose)
            : base(new AbstractLoggerSettings { MinimalLoggingLevel = minimalLoggingLevel })
        {
            _ravenClient = ravenClient ?? throw new ArgumentNullException(nameof(ravenClient));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RavenAbstractLogger"/> class.
        /// </summary>
        /// <param name="ravenClient">The raven client.</param>
        /// <param name="settings">The logger's settings.</param>
        public RavenAbstractLogger(RavenClient ravenClient, AbstractLoggerSettings settings)
            : base(settings ?? throw new ArgumentNullException(nameof(settings)))
        {
            _ravenClient = ravenClient ?? throw new ArgumentNullException(nameof(ravenClient));
        }

        /// <summary>
        /// Traces the message.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="parameters">The logging parameters.</param>
        public override void TraceMessage(string message, LoggingLevel loggingLevel, IEnumerable<ILoggingParameter> parameters)
        {
            if (!ShouldBeTraced(loggingLevel))
            {
                return;
            }

            var loggingParameters = GetGlobalAndLocalLoggingParameters(parameters);
            Dictionary<string, string> tags = GenerateTags(loggingParameters);
            _ravenClient.Capture(new SentryEvent(message)
            {
                Tags = tags,
                Extra = loggingParameters.ExtractData(),
                Level = LoggingLevelMapper.ConvertLoggingLevelToErrorLevel(loggingLevel)
            });
        }

        /// <summary>
        /// Traces the exception.
        /// </summary>
        /// <param name="exception">The exception to be logged.</param>
        /// <param name="loggingLevel">The logging level.</param>
        /// <param name="parameters">The logging parameters.</param>
        public override void TraceException(Exception exception, LoggingLevel loggingLevel, IEnumerable<ILoggingParameter> parameters)
        {
            if (!ShouldBeTraced(loggingLevel))
            {
                return;
            }

            var loggingParameters = GetGlobalAndLocalLoggingParameters(parameters);
            Dictionary<string, string> tags = GenerateTags(loggingParameters);
            _ravenClient.Capture(new SentryEvent(exception)
            {
                Tags = tags,
                Extra = loggingParameters.ExtractData(),
                Level = LoggingLevelMapper.ConvertLoggingLevelToErrorLevel(loggingLevel)
            });
        }

        private static Dictionary<string, string> GenerateTags(IEnumerable<ILoggingParameter> parameters)
        {
            return parameters.ExtractTags().ToDictionary(tag => tag);
        }
    }
}