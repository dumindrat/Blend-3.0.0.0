using Blend.DataService.GardenMarks;
using Blend.DataService.Grades;
using Blend.DataService.Mapping;

namespace Blend.DataService.Tests.GardenMarksAndGrades;

[TestClass]
public sealed class LookupDataServiceTests
{
    [TestMethod]
    public void GardenMarkReadAndInsertUseLegacyProcedureNamesAndParameters()
    {
        var rows = new[] { new GardenMarkRecord { GardenMarkName = "Garden" } };
        var mapper = new RecordingMapper { Rows = rows };
        var service = new GardenMarkDataService(mapper);

        Assert.AreSame(rows, service.GetGardenMarksList());
        Assert.AreEqual("trn_GardenMarkGet", mapper.Procedure);
        Assert.IsNull(mapper.Parameters);
        Assert.IsTrue(service.InsertGardenMark(" Garden "));
        Assert.AreEqual("trn_GardenMarkInsert", mapper.Procedure);
        Assert.AreEqual(" Garden ", Parameter(mapper.Parameters, "GardenMark"));
        Assert.AreEqual(1, mapper.Parameters!.GetType().GetProperties().Length);
    }

    [TestMethod]
    public void GradeReadAndInsertUseLegacyProcedureNamesAndParameters()
    {
        var rows = new[] { new GradeRecord { GradeName = "BOP" } };
        var mapper = new RecordingMapper { Rows = rows };
        var service = new GradeDataService(mapper);

        Assert.AreSame(rows, service.GetGradeList());
        Assert.AreEqual("trn_GradeGet", mapper.Procedure);
        Assert.IsNull(mapper.Parameters);
        Assert.IsTrue(service.InsertGrade(" BOP "));
        Assert.AreEqual("trn_GradeInsert", mapper.Procedure);
        Assert.AreEqual(" BOP ", Parameter(mapper.Parameters, "GradeName"));
        Assert.AreEqual(1, mapper.Parameters!.GetType().GetProperties().Length);
    }

    [TestMethod]
    public void NullArgumentsAndDatabaseErrorsAreNotHidden()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => new GardenMarkDataService(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => new GradeDataService(null!));
        var mapper = new RecordingMapper();
        var garden = new GardenMarkDataService(mapper);
        var grade = new GradeDataService(mapper);
        Assert.ThrowsExactly<ArgumentNullException>(() => garden.InsertGardenMark(null!));
        Assert.ThrowsExactly<ArgumentNullException>(() => grade.InsertGrade(null!));
        var failure = new InvalidOperationException("Database failure");
        mapper.Failure = failure;
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => garden.GetGardenMarksList()));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => garden.InsertGardenMark("Garden")));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => grade.GetGradeList()));
        Assert.AreSame(failure, Assert.ThrowsExactly<InvalidOperationException>(() => grade.InsertGrade("BOP")));
    }

    private static object? Parameter(object? parameters, string name) =>
        parameters!.GetType().GetProperty(name)!.GetValue(parameters);

    private sealed class RecordingMapper : IStoredProcedureMapper
    {
        public string? Procedure { get; private set; }
        public object? Parameters { get; private set; }
        public object? Rows { get; init; }
        public Exception? Failure { get; set; }
        public IReadOnlyList<T> Query<T>(string procedureName, object? parameters = null)
        {
            Record(procedureName, parameters);
            return (IReadOnlyList<T>)Rows!;
        }
        public T? QuerySingleOrDefault<T>(string procedureName, object? parameters = null) where T : class => throw new NotSupportedException();
        public int Execute(string procedureName, object? parameters = null) { Record(procedureName, parameters); return -1; }
        private void Record(string procedureName, object? parameters)
        {
            if (Failure is not null) throw Failure;
            Procedure = procedureName;
            Parameters = parameters;
        }
    }
}
