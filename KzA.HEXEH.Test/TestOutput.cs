using KzA.HEXEH.Base.Output;
using Xunit.Abstractions;

namespace KzA.HEXEH.Test;

public class TestOutput(ITestOutputHelper output) : TestBase(output)
{
    [Fact]
    public void Test()
    {
        var head = new DataNode("Head", "This is head", 0, 0);
        var child0 = new DataNode(
            "Child0", "This is first child",
            new string[] { "aaa", "bbb", "ccc" }, 0, 0
            );
        var child10 = new DataNode("Child10", "CHILD0 OF CHILD1", new string[] { "???", "!!!!!!!!!!!!!!!!!!!!!!!!!!!" }, 0, 0);
        var child11 = new DataNode("Child11", "CHILD1 OF CHILD1", 0, 0);
        child11.Detail.Add("THIS IS LAST DETAIL");
        var child1 = new DataNode(
            "Child1", "This is second child",
            new string[] { "aaa", "bbb", "ccc" },
            new DataNode[] { child10, child11 }, 0, 0
            );

        var child2 = new DataNode(
            "Child1", "This is third child",
            new DataNode[] { child10, child11 }, 0, 0
            );
        child2.Detail.Add("THIS IS NOT LAST DETAIL");
        head.Children.Add(child0);
        head.Children.Add(child1);
        head.Children.Add(child2);
        Output.WriteLine(head.ToString());
        Output.WriteLine(head.ToStringVerbose());
    }
}