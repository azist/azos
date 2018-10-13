
using System;

using Azos.Apps;


namespace Azos.Glue.Protocol
{
    /// <summary>
    /// Represents a response message sent by called party
    /// </summary>
    [Serializable]
    public sealed class ResponseMsg : Msg
    {

        public ResponseMsg(FID requestID, object returnValue) : this(requestID, null, returnValue)
        {}


        public ResponseMsg(FID requestID, Guid? instance,  object returnValue) : base()
        {
          m_RequestID = requestID;
          m_RemoteInstance = instance;
          m_ReturnValue = returnValue;
        }


        /// <summary>
        /// This .ctor is handy for message inspectors.
        /// Creates a substitute message for the original one with new value.
        /// Binding-specific context is cloned and headers/correlation data are cloned conditionaly
        /// </summary>
        public ResponseMsg(ResponseMsg inspectedOriginal, object newReturnValue, bool cloneHeaders = true, bool cloneCorrelation = true) : base()
        {
          m_RequestID = inspectedOriginal.m_RequestID;
          m_RemoteInstance = inspectedOriginal.m_RemoteInstance;
          m_ReturnValue = newReturnValue;

          CloneState(inspectedOriginal, cloneHeaders, cloneCorrelation);
        }


       private FID m_RequestID;

       //note: out and ref parameters ar not supported
       //may contain remote exception data
       private object m_ReturnValue;

       private Guid? m_RemoteInstance;


       /// <summary>
       /// Returns request ID this response is for
       /// </summary>
       public override FID RequestID
       {
         get { return m_RequestID;}
       }

       /// <summary>
       /// Returns return value of the called method. Note: out and ref params are not supported
       /// </summary>
       public object ReturnValue
       {
         get { return m_ReturnValue; }
       }


       /// <summary>
       /// For stateful servers returns instance ID
       /// </summary>
       public Guid? RemoteInstance
       {
         get { return m_RemoteInstance; }
       }

       /// <summary>
       /// Returns remote exception data if any
       /// </summary>
       public WrappedExceptionData ExceptionData { get {return m_ReturnValue as WrappedExceptionData; } }

       /// <summary>
       /// Returns true when response does not contain remote server exception which is represented by WrappedExceptionData
       /// </summary>
       public bool OK { get {return !(m_ReturnValue is WrappedExceptionData); }  }
    }




}
