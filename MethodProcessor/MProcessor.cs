using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Nile.Definition;
using Nile.Definitions;
using Nile.TestPlanReader;

namespace Nile
{
    public delegate void delegateTestItemStart(Dictionary<string, string> NewTest, DateTime TimeStamp);
    public delegate void delegateTestItemEnd(Dictionary<string, string> NewTest, ArrayList OutputValue, DateTime TimeStamp);
    public class Processor
    {
        TestPlan CurrentPlan;
        delegateTestItemStart dlgtStart = null;
        delegateTestItemEnd dlgtEnd = null;
        public Processor(TestPlan TP) //test plan
                        //,
                        //,
                        //,)
        {
            CurrentPlan = TP;
            dlgtStart = new delegateTestItemStart(ItemStart);
            dlgtEnd = new delegateTestItemEnd(ItemEnd);

        }

        public void PlanExecution()
        {
            Dictionary<string, string> dictCurrentTest = null;
            string strCurrentGUID = string.Empty;
            Dictionary<string, object> dictCurrentInput = null;
            ArrayList alCurrentOutputSpec = null;

            try
            {
                List<Dictionary<string, string>> listSequence = CurrentPlan.TPList;
                Dictionary<string, Dictionary<string, object>> dictInput = CurrentPlan.InputDict;
                Dictionary<string, ArrayList> dictOutputSpec = CurrentPlan.OutputSpecDict;
                DateTime dtItemStart = DateTime.Now;

                foreach (Dictionary<string, string> loop in listSequence)
                {
                    //Timestampe of start
                    TimeSpan tsSpan;
                    dtItemStart = DateTime.Now;
                    //get info of current test
                    dictCurrentTest = loop;
                    strCurrentGUID = dictCurrentTest["GUID"];
                    dictCurrentInput = CurrentPlan.InputDict[strCurrentGUID];
                    alCurrentOutputSpec = CurrentPlan.OutputSpecDict[strCurrentGUID];

                    //call delegation for showing the next test
                    //TODO: implement delegations

                    ArrayList alOutputValue = RunningItem(dictCurrentTest,
                                                dictCurrentInput,
                                                alCurrentOutputSpec);

                    //restore current indicator
                    dictCurrentTest = null;
                    strCurrentGUID = string.Empty;
                    dictCurrentInput = null;
                    alCurrentOutputSpec = null;
                    tsSpan = DateTime.Now - dtItemStart;

                    //call delegation for reporting and showing test result
                    //TODO: implement delegations
                }
            }
            catch (Exception ex)
            {
                //TODO: Implement the exception handling
            }
        }

        public ArrayList RunningItem(Dictionary<string, string> ItemInfo,
                                Dictionary<string, object> Input,
                                ArrayList OutputSpec)
        {
            ArrayList alOutputValues = new ArrayList();
            Assembly asm = null;
            Type[] typeArray = null;
            Type type = null;
            object[] argsInput = new object[1];
            object objTest = null;
            MethodInfo[] miArray = null;
            MethodInfo miDo = null;

            try
            {
                dlgtStart(ItemInfo, DateTime.Now);
                asm = Assembly.LoadFrom(ItemInfo[CommonTags.TestPlan_ResourceName]);
                typeArray = asm.GetExportedTypes();
                foreach (Type a in typeArray)
                {
                    if (true == a.ToString().EndsWith(ItemInfo[CommonTags.TestPlan_MethodName]))
                    {
                        type = asm.GetType(a.ToString());
                        break;
                    }
                }
                if (type == null)
                {
                    throw new Exception(string.Format("Can't load method {0} in file {1}", ItemInfo[CommonTags.TestPlan_MethodName], ItemInfo[CommonTags.TestPlan_ResourceName]));
                }

                //create an instance of class of the test method
                //TODO: get initialized instrument handle
                //objTest = Activator.CreateInstance(type, handleInstruments);
                objTest = Activator.CreateInstance(type);

                //Get field info of the class
                FieldInfo[] myfields = type.GetFields();

                //read the function list of the method and check, initialize
                miArray = type.GetMethods();
                foreach (MethodInfo mi in miArray)
                {
                    if (true == mi.Name.Equals("Do"))
                    {
                        miDo = type.GetMethod("Do");
                    }
                }
                if (miDo == null)
                {
                    if (type == null)
                    {
                        throw new Exception(string.Format("Can't load the measurement function from specified method {0} in file {1}", ItemInfo[CommonTags.TestPlan_MethodName], ItemInfo[CommonTags.TestPlan_ResourceName]));
                    }
                }

                //initial args with dictionary of all input
                argsInput[0] = Input;

                //Call the test
                alOutputValues = (ArrayList)miDo.Invoke(objTest, argsInput);
                dlgtEnd(ItemInfo, alOutputValues, DateTime.Now);
            }
            catch (Exception ex)
            {
                //TODO: classifying the exception, such as exception source, exception level
            }
            return alOutputValues;
        }

        public void ItemStart(Dictionary<string, string> NewTest, DateTime TimeStamp)//TODO: add DutInfo)
        {
            
        }
        public void ItemEnd(Dictionary<string, string> NewTest, ArrayList OuputValue, DateTime TimeStamp)//TODO: add DutInfo)
        {

        }
    }
}
