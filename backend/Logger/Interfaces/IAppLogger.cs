namespace Logger.Interfaces;

public interface IAppLogger {
    IAppLogger ForContext(string propertyName, object value, bool destructureObjects = false);
    IDisposable PushProperty(string propertyName, object value, bool destructureObjects = false);
    void Trace(string messageTemplate, params object[] propertyValues);
    void Trace(Exception exception, string messageTemplate, params object[] propertyValues);
    void TraceMessage(string message);
    void TraceException(Exception exception, string message);
    void Debug(string messageTemplate, params object[] propertyValues);
    void Debug(Exception exception, string messageTemplate, params object[] propertyValues);
    void DebugMessage(string message);
    void DebugException(Exception exception, string message);
    void Info(string messageTemplate, params object[] propertyValues);
    void Info(Exception exception, string messageTemplate, params object[] propertyValues);
    void InfoMessage(string message);
    void InfoException(Exception exception, string message);
    void Warning(string messageTemplate, params object[] propertyValues);
    void Warning(Exception exception, string messageTemplate, params object[] propertyValues);
    void WarningMessage(string message);
    void WarningException(Exception exception, string message);
    void Error(string messageTemplate, params object[] propertyValues);
    void Error(Exception exception, string messageTemplate, params object[] propertyValues);
    void ErrorMessage(string message);
    void ErrorException(Exception exception, string message);
    void Fatal(string messageTemplate, params object[] propertyValues);
    void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
    void FatalMessage(string message);
    void FatalException(Exception exception, string message);
    void LogEntryInformation(string messageTemplate, params object[] propertyValues);
    void LogEntryWarning(string messageTemplate, params object[] propertyValues);
    void LogEntryError(string messageTemplate, params object[] propertyValues);
    void LogEntryInformationMessage(string message);
    void LogEntryWarningMessage(string message);
    void LogEntryErrorMessage(string message);
    void LogEntryErrorException(Exception exception, string message);
    void LogTransactionInformation(string transactionId, string messageTemplate, params object[] propertyValues);

    void LogTransactionInformation(string transactionId, Exception exception, string messageTemplate, params object[] propertyValues);

    void LogTransactionWarning(string transactionId, string messageTemplate, params object[] propertyValues);

    void LogTransactionWarning(string transactionId, Exception exception, string messageTemplate, params object[] propertyValues);

    void LogTransactionError(string transactionId, string messageTemplate, params object[] propertyValues);

    void LogTransactionError(string transactionId, Exception exception, string messageTemplate, params object[] propertyValues);

    void LogTransactionInformationMessage(string transactionId, string message);
    void LogTransactionWarningMessage(string transactionId, string message);
    void LogTransactionErrorMessage(string transactionId, string message);
    void LogTransactionErrorException(string transactionId, Exception exception, string message);
}