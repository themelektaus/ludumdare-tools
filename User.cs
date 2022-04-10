namespace LudumDareTools;

public class User : LudumDareNode
{
    public override void BeforeSave(dynamic data)
    {

    }

    public override Task BeforeReturn()
    {
        return Task.CompletedTask;
    }
}