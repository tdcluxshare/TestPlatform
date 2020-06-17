using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nile;
using Nile.Definitions;

namespace Nile.CommonInstrument
{
    public class CommonInstruments
    {
        public delegate void Send2LogEventHanler(object sender, Send2LogEventArgs send2LogEvArgs);
        public event Send2LogEventHanler eventSent2Log;

        public CommonInstruments()
        {
            
        }

        /// <summary>
        /// The settings of the specified module must have not children.
        /// </summary>
        /// <param name="sender">ojbect of caller</param>
        /// <param name="FileName">file name of settings</param>
        /// <param name="RootName">the root noot of file has the child of expected module</param>
        /// <returns></returns>
        protected Dictionary<string, List<object>> LoadSetting(object sender, string FileName, string RootName)
        {
            StreamReader file = null;
            JsonTextReader reader;
            string strModuleName = sender.GetType().Name;
            Dictionary<string, List<object>> dictSettings = null;

            try
            {
                //Load json file for init settings
                file = File.OpenText(FileName);
                reader = new JsonTextReader(file);
                JToken jtFile = JToken.ReadFrom(reader);
                JToken jtRootNode = null;
                JToken jtModule = null;

                //Get jtoken of specified module. the setting of the module should be at the second level. 
                jtRootNode = jtFile[RootName];

                //Get jtoken of specified module. the setting of the module should be at the second level. 
                jtModule = jtRootNode[strModuleName];

                if (true == jtModule.HasValues)
                {
                    dictSettings = new Dictionary<string, List<object>>();
                    JObject joModule = (JObject)jtModule;
                    foreach (var pair in joModule)
                    {
                        List<object> listValue = new List<object>();
                        listValue.Add((object)pair.Value);
                        dictSettings.Add(pair.Key, listValue);
                    }
                }
                file.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                }
            }
            return dictSettings;
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
    }
}
