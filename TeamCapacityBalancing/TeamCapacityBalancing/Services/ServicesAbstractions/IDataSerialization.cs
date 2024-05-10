using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamCapacityBalancing.Models;

namespace TeamCapacityBalancing.Services.ServicesAbstractions
{
    interface IDataSerialization
    {
        public void SerializeUserStoryData(List<UserStoryDataSerialization> userStoryDataSerializations, string filename);
        public List<UserStoryDataSerialization> DeserializeUserStoryData(string filename);

        public void SerializeTeamData(List<User> userDataSerializations, string filename);

        public List<User> DeserializeTeamData(string filename);

        public void SerializeSprintData(List<Sprint> sprints, string filename);

        public List<Sprint> DeserializeSprint(string filename);

        public void SerializeSelectionShortTermInfo(SprintSelectionShortTerm SelectShortTermInfoSerializations, string filename);

        public SprintSelectionShortTerm DeserializeSelectionShortTerm(string filename);

    }
}
