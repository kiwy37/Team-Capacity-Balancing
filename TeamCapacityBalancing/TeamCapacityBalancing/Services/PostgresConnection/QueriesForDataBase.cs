using Npgsql;
using System;
using System.Collections.Generic;
using TeamCapacityBalancing.Models;
using TeamCapacityBalancing.Services.ServicesAbstractions;

namespace TeamCapacityBalancing.Services.Postgres_connection
{
    public class QueriesForDataBase : IDataProvider
    {
        private const string JiraissueTable = "jiraissue";
        private const string IssuelinkTable = "issuelink";
        private const string CustomFieldTable = "Customfieldvalue";
        private const string UserTable = "cwd_user";
        private const string StoryIssueType = "10001";
        private const string EpicStoryLinkType = "10201";
        private const string StoryTaskLinkType = "10100";
        private const string SubTaskIssueType = "10003";
        private const string OpenStatus = "1";


        private float CalculateRemainingTimeForStory(int storyId)
        {
            float timeEstimate = 0;
            float timeSpent = 0;
            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand($"SELECT {JiraissueTable}.timeestimate, {JiraissueTable}.timespent " +
                        $"FROM {JiraissueTable} " +
                        $"WHERE {JiraissueTable}.id = {storyId} ", connection);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        timeEstimate += reader.GetInt32(reader.GetOrdinal("timeestimate"));
                        timeSpent += reader.GetInt32(reader.GetOrdinal("timespent"));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return (timeEstimate +timeSpent)-timeSpent;
        }

        private int GetEpicIdFromStory(int storyId)
        {
            int epicId = 0;
            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand($"SELECT {IssuelinkTable}.source " +
                        $"FROM {IssuelinkTable} " +
                        $"WHERE {IssuelinkTable}.linktype = {EpicStoryLinkType} " +
                        $"AND {IssuelinkTable}.destination = {storyId}", connection);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        epicId = reader.GetInt32(reader.GetOrdinal("source"));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return epicId;
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();

            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand("SELECT user_name, display_name, id " +
                        "FROM " + UserTable, connection);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        string username = reader.GetString(reader.GetOrdinal("user_name"));
                        string displayName = reader.GetString(reader.GetOrdinal("display_name"));
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        users.Add(new User(username, displayName, id));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return users;
        }


        public List<IssueData> GetAllStoriesByTeamLeader(User teamLeader)
        {
            List<IssueData> stories = new List<IssueData>();

            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand($"SELECT {JiraissueTable}.id, {JiraissueTable}.assignee, {JiraissueTable}.issuenum, {JiraissueTable}.project, {JiraissueTable}.summary " +
                        $"From {JiraissueTable} " +
                        $"where {JiraissueTable}.assignee = 'JIRAUSER{teamLeader.Id}' " +
                        $"and {JiraissueTable}.issuetype = '{StoryIssueType}' " +
                        $"group by {JiraissueTable}.id", connection);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string name = reader.GetString(reader.GetOrdinal("summary"));
                        string assignee = reader.GetString(reader.GetOrdinal("assignee"));
                        int issueNumber = reader.GetInt32(reader.GetOrdinal("issuenum"));
                        int projectId = reader.GetInt32(reader.GetOrdinal("project"));
                        int epicId = GetEpicIdFromStory(id);

                        float remaining = ((CalculateRemainingTimeForStory(id) / 60) / 60) / 8; //from seconds to days
                        Math.Round(remaining, 2);

                        if (remaining > 0)
                        {
                            stories.Add(new IssueData(id, name, assignee, remaining, epicId));
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return stories;
        }

        public List<IssueData> GetAllEpicsByTeamLeader(User teamLeader)
        {
            List<IssueData> epics = new List<IssueData>();

            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand($@"
                        SELECT {JiraissueTable}.id, {JiraissueTable}.assignee, {JiraissueTable}.issuenum, {JiraissueTable}.project, {JiraissueTable}.summary, {CustomFieldTable}.textvalue
                        FROM {JiraissueTable}
                        JOIN {CustomFieldTable} ON {CustomFieldTable}.issue = {JiraissueTable}.id
                        WHERE {JiraissueTable}.id IN
                        (SELECT {IssuelinkTable}.source
                        FROM {IssuelinkTable}
                        WHERE {IssuelinkTable}.destination IN
                        (SELECT {JiraissueTable}.id
                        FROM {JiraissueTable}
                        WHERE {JiraissueTable}.assignee = 'JIRAUSER{teamLeader.Id}'
                        AND {JiraissueTable}.issuetype = '{StoryIssueType}'
                        AND {JiraissueTable}.summary LIKE '%#%'
                        AND {CustomFieldTable}.textvalue is not null
)
                        
        )", connection);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string name = reader.GetString(reader.GetOrdinal("summary"));
                        int issueNumber = reader.GetInt32(reader.GetOrdinal("issuenum"));
                        int projectId = reader.GetInt32(reader.GetOrdinal("project"));
                        string businesscase = reader.GetString(reader.GetOrdinal("textvalue"));
                        epics.Add(new IssueData(id, name,businesscase));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return epics;
        }
        public List<User> GetAllTeamLeaders()
        {
            List<User> users = new List<User>();

            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();

                    var cmd = new NpgsqlCommand($@"SELECT Distinct cu.id, cu.user_name, cu.display_name
                         FROM {JiraissueTable} AS i
                        JOIN app_user AS au ON i.assignee = au.user_key 
                        JOIN {UserTable} AS cu ON au.id = cu.id 
                       WHERE i.issuetype = '{StoryIssueType}' AND i.summary LIKE '%#%'", connection);
                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = reader.GetInt32(reader.GetOrdinal("id"));
                        string username = reader.GetString(reader.GetOrdinal("user_name"));
                        string displayName = reader.GetString(reader.GetOrdinal("display_name"));
                        users.Add(new User(username, displayName, id));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return users;
        }
        public List<OpenTasksUserAssociation> GetRemainingForUser()
        {
            List<OpenTasksUserAssociation> openTasks= new List<OpenTasksUserAssociation>();
            try
            {
                using (var connection = new NpgsqlConnection(DataBaseConnection.GetInstance().GetConnectionString()))
                {
                    connection.Open();
                    var cmd = new NpgsqlCommand($@"SELECT ji.assignee AS User, au.id, cu.user_name, cu.display_name,
                        SUM(((ji.timeestimate + ji.timespent) - ji.timespent) / 60 / 60 / 8) AS TotalRemaining 
                        FROM {JiraissueTable} AS ji
                        JOIN app_user AS au ON au.user_key = ji.assignee
                        JOIN {UserTable} AS cu ON cu.id = au.id 
                        WHERE ji.issuetype = '{SubTaskIssueType}' 
                        AND ji.assignee IS NOT NULL 
                        AND ji.issuestatus = '{OpenStatus}' 
                        GROUP BY ji.assignee, cu.user_name, au.id, cu.display_name", connection);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int id=  reader.GetInt32(reader.GetOrdinal("id"));
                        string username = reader.GetString(reader.GetOrdinal("user_name"));
                        string displayName = reader.GetString(reader.GetOrdinal("display_name"));
                        User user= new User(username, displayName, id);
                        float remaining = reader.GetFloat(reader.GetOrdinal("totalremaining"));
                        openTasks.Add(new OpenTasksUserAssociation(user, (float)Math.Round(remaining,2)));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return openTasks;
        }
    }
}
