using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Nile.Definitions;
using System.Collections;

namespace Nile
{
    public class TestClassBase : ITestClass
    {
        protected int refID;
        protected Type instanceType;
        protected object testInstance;
        public ArrayList Input { get; protected set; }

        public delegate void Send2LogEventHanler(object sender, Send2LogEventArgs send2LogEvArgs);
        public event Send2LogEventHanler eventSent2Log;
        internal int CreateInstance()
        {
            try
            {
                this.testInstance = Activator.CreateInstance(this.instanceType);
            }
            catch (Exception exception)
            {
                //BridgeExceptionCatch.ReportException(new ResourceLoaderException(exception.InnerException.Message), this.instanceType.FullName);
                throw new Exception(exception.InnerException.Message + this.instanceType.FullName);
            }
            return this.refID;
        }
        public ArrayList Do(object[] args)
        {
            try
            {
                if (args[0] != null)
                {
                    Dictionary<string, object> Input = (Dictionary<string, object>)args[0];
                }
                ArrayList alOutputValue;
                if (this.testInstance == null)
                {
                    this.CreateInstance();
                }
                Type type = this.testInstance.GetType();
                try
                {
                    alOutputValue = (ArrayList)type.InvokeMember("Do", BindingFlags.InvokeMethod, null, this.testInstance, args);
                }
                catch (MissingMethodException)
                {
                    throw new Exception("Measure method is missing. Explicit implementation of testmethod interface not allowed");
                }
                return alOutputValue;
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Error: Failed at call TestClassBase.Do"));
            }
        }

        #region Send info to log file
        public void Send2Log(object sender, string Message)
        {
            Send2LogEventArgs e = new Send2LogEventArgs(Message, DateTime.Now);
            e.Message = Message;
            if (eventSent2Log != null)
            {
                eventSent2Log(sender, e);
            }
        }
        #endregion

        protected bool GetInput(Dictionary<string, object> argsInput, string InputName, out object InputValue)
        {
            InputValue = null;
            if (true == argsInput.ContainsKey(InputName))
            {
                InputValue = argsInput[InputName];
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool GetInput(Dictionary<string, object> argsInput, string InputName, out int InputValue)
        {
            InputValue = int.MinValue;
            if (true == argsInput.ContainsKey(InputName))
            {
                InputValue = Convert.ToInt32(argsInput[InputName]);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected bool GetInput(Dictionary<string, object> argsInput, string InputName, out double InputValue)
        {
            InputValue = double.MinValue ;
            if (true == argsInput.ContainsKey(InputName))
            {
                InputValue = Convert.ToDouble(argsInput[InputName]);
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool GetInput(Dictionary<string, object> argsInput, string InputName, out string InputValue)
        {
            InputValue = null;
            if (true == argsInput.ContainsKey(InputName))
            {
                InputValue = Convert.ToString(argsInput[InputName]);
                return true;
            }
            else
            {
                return false;
            }
        }
        protected bool GetInput(Dictionary<string, object> argsInput, string InputName, out bool InputValue)
        {
            InputValue = false;
            if (true == argsInput.ContainsKey(InputName))
            {
                InputValue = Convert.ToBoolean(argsInput[InputName]);
                return true;
            }
            else
            {
                return false;
            }
        }
        protected void GetInput(string SettingFile, string Module, string Method, string InputName, ref int Input)
        {
            if (false == System.IO.File.Exists(SettingFile))
            {
                throw new Exception(string.Format("[TestClassBase][LoadSetting]:{0} does not exist", SettingFile));
            }
            Utilties.GetInput(SettingFile, Module, Method, InputName, ref Input);
        }

        protected void GetInput(string SettingFile, string Module, string Method, string InputName, ref double Input)
        {
            if (false == System.IO.File.Exists(SettingFile))
            {
                throw new Exception(string.Format("[TestClassBase][LoadSetting]:{0} does not exist", SettingFile));
            }
            Utilties.GetInput(SettingFile, Module, Method, InputName, ref Input);
        }

        protected void GetInput(string SettingFile, string Module, string Method, string InputName, ref string Input)
        {
            if (false == System.IO.File.Exists(SettingFile))
            {
                throw new Exception(string.Format("[TestClassBase][LoadSetting]:{0} does not exist", SettingFile));
            }
            Utilties.GetInput(SettingFile, Module, Method, InputName, ref Input);
        }

        protected void GetInput(string SettingFile, string Module, string Method, string InputName, ref bool Input)
        {
            if (false == System.IO.File.Exists(SettingFile))
            {
                throw new Exception(string.Format("[TestClassBase][LoadSetting]:{0} does not exist", SettingFile));
            }
            Utilties.GetInput(SettingFile, Module, Method, InputName, ref Input);
        }
    }
}
