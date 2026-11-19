using Blend.BusinessService.GardenMarks;
using Blend.BusinessService.Grades;
using Blend.DataService.GardenMarks;
using Blend.DataService.Grades;

namespace Blend.BusinessService.Tests.GardenMarksAndGrades;

[TestClass]
public sealed class LookupServiceTests
{
    [TestMethod]
    public void GardenMarksMapValuesAndDelegateInserts()
    {
        var data = new FakeGardenMarks { Records = [new() { GardenMarkName = "Garden" }, new() { GardenMarkName = null }] };
        var service = new GardenMarkService(data);
        var result = service.GetGardenMarksList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("Garden", result[0].GardenMarkName);
        Assert.IsNull(result[1].GardenMarkName);
        Assert.IsTrue(service.InsertGardenMark(" Garden ").Succeeded);
        Assert.AreEqual(" Garden ", data.Inserted);
        data.Result = false;
        Assert.AreEqual("The garden mark could not be inserted.", service.InsertGardenMark("Garden").ErrorMessage);
    }

    [TestMethod]
    public void GradesMapValuesAndDelegateInserts()
    {
        var data = new FakeGrades { Records = [new() { GradeName = "BOP" }, new() { GradeName = null }] };
        var service = new GradeService(data);
        var result = service.GetGradeList();
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("BOP", result[0].GradeName);
        Assert.IsNull(result[1].GradeName);
        Assert.IsTrue(service.InsertGrade(" BOP ").Succeeded);
        Assert.AreEqual(" BOP ", data.Inserted);
        data.Result = false;
        Assert.AreEqual("The grade could not be inserted.", service.InsertGrade("BOP").ErrorMessage);
    }

    [TestMethod]
    public void NullInputsAndDataFailuresPropagate()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new GardenMarkService(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GradeService(null!));
        var gardenData = new FakeGardenMarks();
        var gradeData = new FakeGrades();
        var garden = new GardenMarkService(gardenData);
        var grade = new GradeService(gradeData);
        Assert.ThrowsExactly<ArgumentNullException>(() => garden.InsertGardenMark(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => grade.InsertGrade(null!));
        var failure = new InvalidOperationException("Database failure");
        gardenData.Failure = gradeData.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => garden.GetGardenMarksList()));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => garden.InsertGardenMark("Garden")));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => grade.GetGradeList()));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => grade.InsertGrade("BOP")));
    }

    private sealed class FakeGardenMarks : IGardenMarkDataService
    {
        public IReadOnlyList<GardenMarkRecord> Records { get; init; } = [];
        public string? Inserted { get; private set; }
        public bool Result { get; set; } = true;
        public Exception? Failure { get; set; }
        public IReadOnlyList<GardenMarkRecord> GetGardenMarksList() { if (Failure is not null) throw Failure; return Records; }
        public bool InsertGardenMark(string gardenMark) { if (Failure is not null) throw Failure; Inserted = gardenMark; return Result; }
    }

    private sealed class FakeGrades : IGradeDataService
    {
        public IReadOnlyList<GradeRecord> Records { get; init; } = [];
        public string? Inserted { get; private set; }
        public bool Result { get; set; } = true;
        public Exception? Failure { get; set; }
        public IReadOnlyList<GradeRecord> GetGradeList() { if (Failure is not null) throw Failure; return Records; }
        public bool InsertGrade(string grade) { if (Failure is not null) throw Failure; Inserted = grade; return Result; }
    }
}
