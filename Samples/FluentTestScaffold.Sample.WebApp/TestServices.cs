namespace FluentTestScaffold.Sample.WebApp;

public interface IOverrideTestService { }

public class RealOverrideService : IOverrideTestService { }

public interface IOtherTestService { }

public class RealOtherService : IOtherTestService { }
