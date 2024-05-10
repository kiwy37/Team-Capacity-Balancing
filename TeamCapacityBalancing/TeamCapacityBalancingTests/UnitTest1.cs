using Avalonia.Media;
using TeamCapacityBalancing.Models;

namespace TeamCapacityBalancingTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void CalculateCoverage_WithValidData()
    {
        var userStory = new IssueData("Test Story", 10.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story);
        var userStoryAssociation = new UserStoryAssociation(userStory, true, 5.0f, new List<float> { 1.0f, 2.0f, 3.0f }, 3);

        userStoryAssociation.CalculateCoverage();

        Assert.AreEqual(6.0f, userStoryAssociation.Coverage.Value);
    }

    [TestMethod]
    public void CalculateCoverage_WithZeroDays()
    {
        var userStory = new IssueData("Zero Work Story", 5.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story);
        var userStoryAssociation = new UserStoryAssociation(userStory, true, 5.0f, new List<float> { 0.0f, 0.0f, 0.0f }, 3);

        userStoryAssociation.CalculateCoverage();

        Assert.AreEqual(0.0f, userStoryAssociation.Coverage.Value);
    }

    [TestMethod]
    public void ColorBackgroundBalancingListInitializationTest()
    {
        var userStory = new UserStoryAssociation(new IssueData("Story", 5.0f, "Release", "Sprint", true, IssueData.IssueType.Story), true, 10.0f, new List<Tuple<User, float>>(), 5);

        foreach (var brush in userStory.ColorBackgroundList)
        {
            Assert.IsInstanceOfType(brush, typeof(SolidColorBrush));
            Assert.AreEqual(Colors.White, ((SolidColorBrush)brush).Color);
        }
    }

    [TestMethod]
    public void Sprint_CreationWithStartAndEndDate()
    {
        var sprint = new Sprint("Sprint 1", 2.0f, "2023-09-10", "2023-09-24");

        Assert.AreEqual("Sprint 1", sprint.Name);
        Assert.AreEqual(2.0f, sprint.NumberOfWeeks);
        Assert.AreEqual("2023-09-10", sprint.StartDate);
        Assert.AreEqual("2023-09-24", sprint.EndDate);
    }

    [TestMethod]
    public void Sprint_WithShortTermTrue_ShouldSetIsInShortTermToTrue()
    {
        string name = "Test Sprint";
        float numberOfWeeks = 2.0f;

        var sprint = new Sprint(name, numberOfWeeks, true);

        Assert.IsTrue(sprint.IsInShortTerm);
    }

    [TestMethod]
    public void Sprint_WithShortTermFalse_ShouldSetIsInShortTermToFalse()
    {
        string name = "Another Sprint";
        float numberOfWeeks = 3.0f;

        var sprint = new Sprint(name, numberOfWeeks, false);

        Assert.IsFalse(sprint.IsInShortTerm);
    }

    [TestMethod]
    public void UserStoryDataSerialization_Creation()
    {
        var user = new User("testuser");
        var story = new IssueData("Test Story", 10.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story);
        var userStoryAssociation = new UserStoryAssociation(story, true, 5.0f, new List<Tuple<User, float>>(), 3);

        var serialization = new UserStoryDataSerialization(story, true, 5.0f, new List<Tuple<User, float>> { new Tuple<User, float>(user, 2.0f), new Tuple<User, float>(user, 3.0f) });

        Assert.AreEqual(story, serialization.Story);
        Assert.AreEqual(true, serialization.ShortTerm);
        Assert.AreEqual(5.0f, serialization.Remaining);
        Assert.AreEqual(2, serialization.UsersCapacity.Count);
        Assert.AreEqual(user, serialization.UsersCapacity[0].Item1);
        Assert.AreEqual(2.0f, serialization.UsersCapacity[0].Item2);
        Assert.AreEqual(user, serialization.UsersCapacity[1].Item1);
        Assert.AreEqual(3.0f, serialization.UsersCapacity[1].Item2);
    }

    [TestMethod]
    public void UserStoryDataSerialization_WithValidData_ShouldInitializePropertiesCorrectly()
    {
        var story = new IssueData("Test Story", 10.0f, "Release 1", "Sprint 1", true, IssueData.IssueType.Story);
        var user1 = new User("User1", "John Doe", 1);
        var user2 = new User("User2", "Jane Smith", 2);
        var usersCapacity = new List<Tuple<User, float>>()
            {
                new Tuple<User, float>(user1, 5.0f),
                new Tuple<User, float>(user2, 3.0f)
            };

        var serialization = new UserStoryDataSerialization(story, true, 5.0f, usersCapacity);

        Assert.AreEqual(story, serialization.Story);
        Assert.IsTrue(serialization.ShortTerm);
        Assert.AreEqual(5.0f, serialization.Remaining);
        Assert.AreEqual(usersCapacity, serialization.UsersCapacity);
    }

    [TestMethod]
    public void UserStoryDataSerialization_WithEmptyUsersCapacity_ShouldInitializeEmptyList()
    {
        var story = new IssueData("Empty Users Capacity Story", 8.0f, "Release 2", "Sprint 2", true, IssueData.IssueType.Story);

        var serialization = new UserStoryDataSerialization(story, false, 8.0f, new List<Tuple<User, float>>());

        Assert.AreEqual(story, serialization.Story);
        Assert.IsFalse(serialization.ShortTerm);
        Assert.AreEqual(8.0f, serialization.Remaining);
        Assert.IsNotNull(serialization.UsersCapacity);
        Assert.AreEqual(0, serialization.UsersCapacity.Count);
    }
}
