using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3GameLogic
{
    public class ResourceEntry
    {
        public string Name { get; set; }
        public string Command { get; set; }
        public string Default { get; set; }
    }

    class ResourcesList
    {
        private IDictionary<string, Resource> resources;
        private IDictionary<string, Dictionary<string, string>> casesResource;
        private readonly string defaultCaseName = "default";
        private readonly string caseName = "case";
        public ResourcesList()
        {
            Resources = new Dictionary<string, Resource>();
        }

        public static ResourcesList readResources()
        {
            string folder = "jsons/";
            string baseFile = "resources.json";
            string baseCaseFile = "cases";

            string text = Asset.ReadAllText(folder + baseFile);

            List<ResourceEntry> resBaseList = JsonConvert.DeserializeObject<List<ResourceEntry>>(Asset.ReadAllText(folder + baseFile));
            ResourcesList resList = new ResourcesList();
            for (int i = 0; i < resBaseList.Count; i++)
            {
                Dictionary<string, string> resourceDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(Asset.ReadAllText(folder + resBaseList[i].Name + ".json"));
                resList.addResource(resBaseList[i].Command, resBaseList[i].Default, resourceDict);
            }

            resList.CasesResource = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(Asset.ReadAllText(folder + baseCaseFile + ".json"));

            return resList;
        }

        public void addResource(String name, String defaultStr, IDictionary<String, String> dict)
        {
            Resources.Add(name, new Resource(defaultStr, dict));
        }

        private string processInfo(string info, string key)
        {
            while (info.Contains("\\data()"))
                info = info.Replace("\\data()", key);
            return info;
        }

        public string getWantedInfo(string resource, params string[] parameters)
        {
            if (resource == caseName)
            {
                if (CasesResource.ContainsKey(parameters[1]))
                {
                    if (CasesResource[parameters[1]].ContainsKey(parameters[0]))
                        return processInfo(CasesResource[parameters[1]][parameters[0]], parameters[0]);
                    int val = 0;
                    if (int.TryParse(parameters[0], out val))
                    {
                        if (val < 0 && CasesResource[parameters[1]].ContainsKey("neg"))
                            return processInfo(CasesResource[parameters[1]]["neg"], val.ToString());
                        if (val >= 100 && CasesResource[parameters[1]].ContainsKey("100+"))
                            return processInfo(CasesResource[parameters[1]]["100+"], val.ToString());
                    }
                    return processInfo(CasesResource[parameters[1]][defaultCaseName], parameters[0]);
                }
                return "";
            }
            if (Resources.ContainsKey(resource))
            {
                if (Resources[resource].StrLookupTable.ContainsKey(parameters[0]))
                    return processInfo(Resources[resource].StrLookupTable[parameters[0]], parameters[0]);
                return processInfo(Resources[resource].DefaultStr, parameters[0]);
            }
            return "";

        }

        public IDictionary<string, Resource> Resources { get => resources; set => resources = value; }
        public IDictionary<string, Dictionary<string, string>> CasesResource { get => casesResource; set => casesResource = value; }
    }

    class Resource
    {
        private string defaultStr;
        private IDictionary<string, string> strLookupTable;

        public string DefaultStr { get => defaultStr; set => defaultStr = value; }
        public IDictionary<string, string> StrLookupTable { get => strLookupTable; set => strLookupTable = value; }

        public Resource(string defaultStr, IDictionary<string, string> strLookupTable)
        {
            DefaultStr = defaultStr;
            StrLookupTable = strLookupTable;
        }
    }
}
