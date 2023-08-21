namespace DbcParserLib
{
    public interface IParseObserver
    {
        void DuplicateMessage(int messageId);
        void DuplicateSignalInMessage(int messageId, string signalName);
        void DuplicateValueTableKey(double key, string value);
        void CommentSintaxError(int messageId);
    }

    public class SilentParseObserver : IParseObserver
    {
        public void DuplicateMessage(int message)
        {
        }

        public void DuplicateSignalInMessage(int messageId, string signalName)
        {
        }

        public void DuplicateValueTableKey(double key, string value)
        {
        }

        public void CommentSintaxError(int lineNumber)
        {
        }
    }
}