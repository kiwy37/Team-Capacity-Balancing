using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Services.ServicesAbstractions;


namespace TeamCapacityBalancing.Services.LocalDataSerialization
{
    public class JsonSerialization : IDataSerialization
    {
        public const string BasePath = "../../../LocalFiles/";
        public const string UserStoryFilePath = BasePath + "UserStoryData/";
        public const string UserFilePath = BasePath + "TeamData/";
        public const string SprintPath = BasePath + "SprintData/";
        public const string SelectionShortTermPath = BasePath + "SelectionShortTerm/";

        public SprintSelectionShortTerm DeserializeSelectionShortTerm(string filename)
        {
            if (File.Exists(SelectionShortTermPath+ filename))
            {
                return JsonConvert.DeserializeObject<SprintSelectionShortTerm>(File.ReadAllText(SelectionShortTermPath + filename));
            }
            return new SprintSelectionShortTerm();
        }

        public List<Sprint> DeserializeSprint(string filename)
        {
            if (File.Exists(SprintPath + filename))
            {
                return JsonConvert.DeserializeObject<List<Sprint>>(File.ReadAllText(SprintPath + filename));
            }
            return new List<Sprint>();
        }

        public List<User> DeserializeTeamData(string filename)
        {
            if(File.Exists(UserFilePath + filename))
            {
                return JsonConvert.DeserializeObject<List<User>>(File.ReadAllText(UserFilePath + filename));
            }
            return new List<User>();

        }

        public List<UserStoryDataSerialization> DeserializeUserStoryData(string filename)
        {
            if (File.Exists(UserStoryFilePath + filename))
            {
                return JsonConvert.DeserializeObject<List<UserStoryDataSerialization>>(File.ReadAllText(UserStoryFilePath + filename));
            }
            return new List<UserStoryDataSerialization>();
        }

        public void SerializeSprintData(List<Sprint> sprints, string filename)
        {
            File.WriteAllText(SprintPath + filename, JsonConvert.SerializeObject(sprints));
        }

        public void SerializeTeamData(List<User> userDataSerializations, string filename)
        {
            File.WriteAllText(UserFilePath + filename, JsonConvert.SerializeObject(userDataSerializations));
        }

        public void SerializeUserStoryData(List<UserStoryDataSerialization> userStoryDataSerializations,string filename)
        {
            File.WriteAllText(UserStoryFilePath + filename, JsonConvert.SerializeObject(userStoryDataSerializations));
        }

        public void SerializeSelectionShortTermInfo(SprintSelectionShortTerm SelectShortTermInfoSerializations, string filename)
        {
            File.WriteAllText(SelectionShortTermPath + filename, JsonConvert.SerializeObject(SelectShortTermInfoSerializations));
        }
    }
}
