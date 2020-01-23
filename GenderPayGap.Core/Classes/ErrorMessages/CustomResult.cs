using System;

namespace GenderPayGap.Core.Classes.ErrorMessages
{
    [Serializable]
    public class CustomResult<T>
    {

        public CustomResult(T result)
        {
            Result = result;
        }

        public CustomResult(CustomError errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        /// <summary>
        ///     Creates a custom error result object. Generally used to report outcomes of method work to calling clients.
        /// </summary>
        /// <param name="errorMessage">Custom error object that contains a code and description of the issue</param>
        /// <param name="errorRelatedObject">
        ///     Object that contains the status of T during the execution of the method, i.e. if the
        ///     result T is expected to be an organisation and the method failed, 'errorRelatedObject' can contain the status of
        ///     the organisation before the error happened - that way the calling client should be aware of the org causing the
        ///     issue.
        /// </param>
        public CustomResult(CustomError errorMessage, T errorRelatedObject)
            : this(errorMessage)
        {
            ErrorRelatedObject = errorRelatedObject;
        }

        public T ErrorRelatedObject { get; }
        public T Result { get; }
        public CustomError ErrorMessage { get; set; }
        public bool Succeeded => ErrorMessage == null;
        public bool Failed => ErrorMessage != null;

    }
}
