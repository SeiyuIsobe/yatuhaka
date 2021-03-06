﻿#if WINDOWS_UWP
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions;
#endif
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// Wraps error details to pass back to the caller of a WebAPI
    /// </summary>
#if !WINDOWS_UWP
    [Serializable()]
#endif
    public class Error
    {
        public enum ErrorType
        {
            Exception = 0,
            Validation = 1
        }

        public ErrorType Type { get; set; }
        public string Message { get; set; }

        public Error(Exception exception)
        {
            Type = ErrorType.Exception;
            Message = Strings.UnexpectedErrorOccurred;
        }

        public Error(string validationError)
        {
            Type = ErrorType.Validation;
            Message = validationError;
        }
    }
}
