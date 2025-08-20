namespace FluentTestScaffold.Sample.Services;

public interface ITimeService
{
    TimeOnly GetTime();
}

public class TimeService : ITimeService
{
    public TimeOnly GetTime()
    {
        return TimeOnly.FromDateTime(DateTime.Now);
    }
}
