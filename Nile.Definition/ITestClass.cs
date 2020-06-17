using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nile.Definitions
{
    public interface ITestClass
    {
        ArrayList Do(object[] args);//return values
        
    }
    #region LogFileChangedEventArgs
    public class LogFileChangedEventArgs : EventArgs
    {
        public string FileName;
        #region Tag
        /// <summary>
        /// An optional tag used to filter the log
        /// </summary>
        public string Tag;
        #endregion
        public LogFileChangedEventArgs(string NewName, DateTime timeStamp)
        {
            this.FileName = NewName;
            //this.Severity = severity;
            this.Tag = "";
        }
    }
    #endregion
    #region SendLogEventArgs
    /// <summary>
    /// This class is used to encapsulate the data passed when logging a debug event.
    /// </summary>
    /// <remarks>
    /// This class is used to pass required information to the Everest communications logger that
    /// is integrated in Session Factory.  The user creates an instance of the class and passes it
    /// as an argument to the <see cref="I5Logger.ILog_DebugInfo"/> event. All fields are mandatory.
    /// </remarks>
    public class Send2LogEventArgs : EventArgs
    {
        #region Message
        /// <summary>
        /// The data or message that is to be logged, in byte format.
        /// </summary>
        /// <remarks>
        /// The data is transfered byte format.  If a string is being sent it should be ASCII
        /// encoded.
        /// </remarks>
        public string Message;
        #endregion

        #region TimeStamp
        /// <summary>
        /// The time and date when the message was logged.
        /// </summary>
        public DateTime TimeStamp;
        #endregion


        #region Tag
        /// <summary>
        /// An optional tag used to filter the log
        /// </summary>
        public string Tag;
        #endregion

        #region Constructor
        /// <summary>
        /// The constructor for the class.
        /// </summary>
        /// <param name="data">The data to be logged.</param>
        /// <param name="timeStamp">The time and date that the message was logged.</param>
        /// <param name="severity">The severity of the message.</param>
        /// <param name="sessionName">The name of the session where the message was logged.</param>
        /// <param name="resourceDescriptor">The resource descriptor of the physical device that the
        /// driver is controlling.  This should be set to null if there is no physical device.</param>
        //public DebugEventArgs(byte[] data, DateTime timeStamp, DebugSeverityTypes severity, string sessionName, string resourceDescriptor)
        public Send2LogEventArgs(string Message, DateTime timeStamp)
        {
            this.Message = Message;
            this.TimeStamp = timeStamp;
            //this.Severity = severity;
            this.Tag = "";
        }
        #endregion
        #endregion
    }
}