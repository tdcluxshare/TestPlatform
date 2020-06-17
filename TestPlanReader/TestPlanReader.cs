using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nile.Definition;


//ToDo:
//1. Add read section "Properties"
//2. Add get a property from TP ojbect.
//3. To verify the functions of get*** from TP object

namespace Nile.TestPlanReader
{
    public class TPReader
    {
        string m_strFileName = string.Empty;
        TestPlan m_TP = new TestPlan();

        public TPReader() ////init a new file
        {
        }

        public static bool ValidateTP(string fileName, out string errMessage)
        {
            errMessage = null;
            try
            {
                if (true == System.IO.File.Exists(fileName))
                {
                }
                else
                    throw new Exception(string.Format("The file {0} does not exist", fileName));

                StreamReader file = File.OpenText(fileName);
                JsonTextReader reader = new JsonTextReader(file);
                JObject joTP = (JObject)JToken.ReadFrom(reader);

                JArray jarSe = (JArray)joTP["Sequence"];
                for (int i = 0; i < jarSe.Count; i++)
                {
                    JObject joItem = (JObject)jarSe[i];
                    if (false == joItem.ContainsKey("Name")
                        || false == joItem.ContainsKey("FileName")
                        || false == joItem.ContainsKey("Method"))
                    {
                        throw new Exception(string.Format("Some required tags are absent at {0}", joItem.ToString()));
                    }
                    if (joItem.ContainsKey("Input") && ((JObject)joItem["Input"]).Count < 1)
                    {
                        throw new Exception(string.Format("It's empty input at {0}", joItem.ToString()));
                    }
                    if (joItem.ContainsKey("Output") && ((JArray)joItem["Output"]).Count < 1)
                    {
                        JArray jarOutput = (JArray)joItem["Output"];
                        for (int j = 0; j < jarOutput.Count; j++)
                        {
                            JObject joPoint = (JObject)jarOutput[j];
                            if (false == joPoint.ContainsKey("ID")
                                || false == joPoint.ContainsKey("Name"))
                            {
                                throw new Exception(string.Format("Some tags are absent with error message {0}", joPoint.ToString()));
                            }
                        }
                    }
                }
                reader.Close();
                file.Close();
            }
            catch (Exception ex)
            {
                errMessage = string.Format("It's not recogized TestPlan. The error message is {0}", ex.ToString());
                return false;
            }
            return true;
        }

        public TPReader(string fileName)
        {
            //check if file exists
            if (true == System.IO.File.Exists(fileName))
            {
                m_strFileName = fileName;
            }
            else
                throw new Exception(string.Format("The file {0} does not exist", fileName));

            //Try to open test plan
            try
            {
                StreamReader file = File.OpenText(m_strFileName);
                JsonTextReader reader = new JsonTextReader(file);
                JObject joTP = (JObject)JToken.ReadFrom(reader);

                //read test plan
                if(false == ReadFile(m_strFileName))
                {
                    //if failed to load test plan
                    m_strFileName = string.Empty;
                    throw new Exception(string.Format("The file could not be parsed as test plan"));
                }
                //close resource
                reader.Close();
                file.Close();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in loading test plan: {0}", ex.Message));
            }
        }

        private bool ReadFile(string fileName)
        {
            try
            {
                StreamReader file = File.OpenText(m_strFileName);
                JsonTextReader reader = new JsonTextReader(file);
                JObject joTP = (JObject)JToken.ReadFrom(reader);

                //load sequence section
                var jo = joTP[CommonTags.TestPlan_Sequence];
                ParseSequecne(ref m_TP, (JArray)jo);
            }
            catch (Exception ex)
            {
                //ToDo: send the error message to log.
                return false;
            }

            return true;
        }

        private void ParseSequecne(ref TestPlan tpSequence, JArray jarSequence)
        {
            try
            {
                for (int i = 0; i < jarSequence.Count; i++)
                {
                    //generates new GUID for identify a test item.
                    Guid id = Guid.NewGuid();
                    Dictionary<string, object> dictAnInput = new Dictionary<string, object>(); //A dictionary of all inputs.
                    ArrayList alOutput = null; // An ArrayList of output 
                    JObject joItem = (JObject)jarSequence[i];

                    //check and get three necessary info
                    if (false == joItem.ContainsKey(CommonTags.TestPlan_TestName)//name of the test item
                        || false == joItem.ContainsKey(CommonTags.TestPlan_ResourceName)//file name
                        || false == joItem.ContainsKey(CommonTags.TestPlan_MethodName))//the method name in the resource file
                    {
                        //These three info are required becasue a test may not require input
                        throw new Exception(string.Format("Some required tags are absent at {0}", joItem.ToString()));
                    }
                    //define a dictionary to store all info of a test item
                    Dictionary<string, string> dictTestItem = new Dictionary<string, string>();
                    dictTestItem.Add("GUID", id.ToString());
                    dictTestItem.Add(CommonTags.TestPlan_TestName, joItem[CommonTags.TestPlan_TestName].ToString());

                    //read resource name, check if exists and add it to dict
                    string strFileName = joItem[CommonTags.TestPlan_ResourceName].ToString();
                    if (true == System.IO.File.Exists(strFileName))
                    {//do nothing
                    }
                    else
                    {
                        throw new Exception(string.Format("Files \"{0}\" does not exist.", strFileName));
                    }
                    dictTestItem.Add(CommonTags.TestPlan_ResourceName, strFileName);

                    //TODO:check if exists
                    dictTestItem.Add(CommonTags.TestPlan_MethodName, joItem[CommonTags.TestPlan_MethodName].ToString());
                    tpSequence.TPList.Add(dictTestItem); // add this test item into TPList

                    if (true == joItem.ContainsKey(CommonTags.TestPlan_Input))
                    {
                        JObject joInput = (JObject)joItem[CommonTags.TestPlan_Input];
                        dictAnInput = ParseInputSection(joInput);
                        tpSequence.InputDict.Add(id.ToString(), dictAnInput);
                    }
                    else
                    {
                        //ToDo: recheck later, add null to dictionary?
                    }
                    if (true == joItem.ContainsKey(CommonTags.TestPlan_Output))
                    {
                        JArray jarOutput = (JArray)joItem[CommonTags.TestPlan_Output];

                        alOutput = ParseOutputSpecSection(jarOutput);
                        tpSequence.OutputSpecDict.Add(id.ToString(), alOutput);
                    }
                    else
                    {
                        //ToDo: recheck later, add null to dictionary?
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error:{0}", ex.ToString()));
            }
        }
        private void ParseProperties(ref TestPlan tpSequence, JObject joProperties)
        {
            try
            {
                //assume there is no array data in this section
                foreach (JToken jt in joProperties.Children())
                {
                    if (jt.Type == JTokenType.Property)
                    {
                        KeyValuePair<string, object> kvpProperty = GetPropertyPair(jt);
                    }
                    else if (jt.Type == JTokenType.Array)
                    {

                    }
                    else if (jt.Type == JTokenType.Null)
                    {

                    }
                    else if (jt.Type == JTokenType.Object)
                    {
                    }
                    else
                    {
                        throw new Exception(string.Format("Unspported type:{0}", jt.Type.ToString()));
                    }
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("", ex.Message));
            }
        }

        private KeyValuePair<string, object> GetPropertyPair(JToken DataPair)
        {
            //No sub branch allowed
            if (((JObject)DataPair).Count > 1)
            {
                throw new Exception(string.Format("Children exist. It's not a base data pair with {0}", DataPair.First.ToString()));            
            }
            try
            {
                var property = DataPair as JProperty;
                KeyValuePair<string, object> kvp = new KeyValuePair<string, object>(property.Name, property.Value);
                return kvp;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in parse the data {0} with error message: {1}", DataPair.ToString(), ex.Message));
            }
        }

        private ArrayList ParseOutputSpecSection(JArray OutputSpecSection)
        {
            try
            {
                ArrayList alOutputSpec = new ArrayList();
                if (OutputSpecSection.Count < 1) //no measure point assigned.
                {
                }
                else
                {
                    Dictionary<string, Dictionary<string, object>> dictMeasGroup = new Dictionary<string, Dictionary<string, object>>();//<UniqueID,<name,value>>
                    for (int i = 0; i < OutputSpecSection.Count; ++i)  //遍历JArray
                    {
                        JObject joPoint = JObject.Parse(OutputSpecSection[i].ToString());
                        Dictionary<string, object> dictPoint = new Dictionary<string, object>();
                        //check if ID and Name present.
                        if (false == joPoint.ContainsKey(CommonTags.TestPlan_MeasPointID)
                            || false == joPoint.ContainsKey(CommonTags.TestPlan_MeasPointName))
                        {
                            throw new Exception(string.Format("Some tags are absent with error message {0}", joPoint.ToString()));
                        }
                        //get ID, get Name and add to dict
                        string strID = joPoint[CommonTags.TestPlan_MeasPointID].ToString();
                        dictPoint.Add(CommonTags.TestPlan_MeasPointName, joPoint[CommonTags.TestPlan_MeasPointName].ToString());

                        //get limit for the meas point, if neithor found, treat it as no verification
                        if (true == joPoint.ContainsKey(CommonTags.TestPlan_Limit1))//for compare rule: equal
                        {
                            dictPoint.Add(CommonTags.TestPlan_Limit1, joPoint[CommonTags.TestPlan_Limit1]);
                        }
                        if (true == joPoint.ContainsKey(CommonTags.TestPlan_Limit2))//for high and low limits compare
                        {
                            dictPoint.Add(CommonTags.TestPlan_Limit2, joPoint[CommonTags.TestPlan_Limit2]);
                        }

                        //add the point to the group
                        dictMeasGroup.Add(strID, dictPoint);//add the meas point to output dict
                    }
                    //add the group to arraylist
                    alOutputSpec.Add(dictMeasGroup);
                }
                return alOutputSpec;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in parse output section:\"{0}\" with error message:{1}", OutputSpecSection.ToString(), ex.Message));
            }
        }

        private Dictionary<string, object> ParseInputSection(JObject Section)
        {
            try
            {
                Dictionary<string,object> dictInput = new Dictionary<string, object> ();
                if (false == Section.HasValues)
                {
                    return null;
                }
                else
                {
                    foreach (JToken jt in Section.Children())
                    {
                        JProperty jp = (JProperty)jt;

                        dictInput.Add(jp.Name, jp.Value);
                    }

                    if (dictInput.Count > 0)
                    {
                        return dictInput;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in parse input at \"{0}\" with message \"{1}\"", Section.ToString(), ex.ToString()));
            }
        }
    }

    public class TestPlan
    {
        public List<Dictionary<string, string>> TPList = null;
        public Dictionary<string, Dictionary<string, object>> InputDict = null;//<GUID,<Name,value>>
        public Dictionary<string, ArrayList> OutputSpecDict = null;//<GUID,[<MPID,<name,value>]>
        public Dictionary<string, string> PropDict = null;
        //  item1
        //  item2
        private Dictionary<string, string> m_dictAnItem = null;
        //Example:  "GUID":1234-abcd-*-*    //unique id
        //          "Name":Test1            //test name shows on screen
        //          "File":abcd.dll         //the resource file
        //          "Method":edfg           //the method to run the test
        private ArrayList m_alAnInput = null; //key is the GUID:1234-abcd-*-*    //unique id linked to the item
        //Example:  300              // or a dictionary for multi-value as an input
        //          true
        //contains vairants of different types without names
        private ArrayList m_alAnOutput = null; //key is the GUID "GUID":1234-abcd-*-*    //unique id linked to the item
        //Example:  key:"pointID" value:               // the name shows on screen
        //                              - key:"Name" value:"A test"

        public TestPlan()
        {
            TPList = new List<Dictionary<string, string>>();//to store all test items with indicator GUID.
            InputDict = new Dictionary<string, Dictionary<string, object>>();//to store input info with indicator GUID
            OutputSpecDict = new Dictionary<string, ArrayList>();//to store output info with indicator GUID
            PropDict = new Dictionary<string, string>();//to store info of "Properties"
        }

        public Dictionary<string, string> GetAnItemByGUID(string GUID)
        {
            if (TPList == null || TPList.Count == 0)
            {
                throw new Exception(string.Format("The TP list is null or empty."));
            }

            try
            {
                foreach (Dictionary<string, string> dictAnItem in TPList)
                {
                    string strGUID = dictAnItem["GUID"];
                    if (true == strGUID.Equals(GUID))
                    {
                        return dictAnItem;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in get test item by GUID:{0} with error message {1}", GUID, ex.Message));
            }
            return null;
        }

        public Dictionary<string, string> GetAnItemByName(string Name)
        {
            if (TPList == null || TPList.Count == 0)
            {
                throw new Exception(string.Format("The TP list is null or empty."));
            }

            try
            {
                foreach (Dictionary<string, string> dictAnItem in TPList)
                {
                    string strName = dictAnItem["Name"];
                    if (true == strName.Equals(Name))
                    {
                        return dictAnItem;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in get test item by name:{0} with error message {1}", Name, ex.Message));
            }
            return null;
        }

        public Dictionary<string, object> GetAnInputByGUID(string GUID)
        {
            if (TPList == null || TPList.Count == 0)
            {
                throw new Exception(string.Format("The TP list is null or empty."));
            }

            if (true == InputDict.ContainsKey(GUID))
            {
                return InputDict[GUID];
            }
            else
            {
                return null;
            }
        }

        public ArrayList GetAnOutputByGUID(string GUID)
        {
            if (TPList == null || TPList.Count == 0)
            {
                throw new Exception(string.Format("The TP list is null or empty."));
            }

            if (true == OutputSpecDict.ContainsKey(GUID))
            {
                return OutputSpecDict[GUID];
            }
            else
            {
                return null;
            }
        }
        public Dictionary<string, string> GetAnMeasPointByName(string Name)//id or point name
        {
            if (TPList == null || TPList.Count == 0)
            {
                throw new Exception(string.Format("The TP list is null or empty."));
            }

            try
            {
                foreach (KeyValuePair<string, ArrayList> kvpAnOutput in OutputSpecDict)
                {
                    ArrayList alMG = kvpAnOutput.Value;
                    foreach (object obMG in alMG)
                    {
                        Dictionary<string, Dictionary<string, string>> dictAnOutput = (Dictionary<string, Dictionary<string, string>>)obMG;
                        if (true == dictAnOutput.ContainsKey(Name))
                        {
                            return dictAnOutput[Name];
                        }
                        else
                        {
                            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dictAnOutput)
                            {
                                Dictionary<string, string> dictMP = kvp.Value;
                                if (true == dictMP.ContainsValue(Name))
                                {
                                    return dictMP;
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error in get test item by name:{0} with error message {1}", Name, ex.Message));
            }
        }
    }
}
