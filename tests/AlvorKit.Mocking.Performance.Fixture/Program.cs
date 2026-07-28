MockDynamic.Enable();
MockInterception.Enable();

var options = MockPerformanceOptions.Parse(args);
var environment = MockPerformanceEnvironment.Capture();
var fixture = new MockPerformanceFixture(options);
var results = fixture.MeasureAll();
var report = new MockPerformanceReport(
    environment,
    options,
    results,
    MockPerformanceFixture.MeasurementBoundaries);

MockPerformanceOutput.Print(report);
MockPerformanceOutput.WriteJson(report);
