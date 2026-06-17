namespace AlvorKit.ECS.Test;

[TestClass]
public class EntObjDebugViewTest
{
    /// <summary>Verifies EntObj DebugView HasAllComponents.</summary>
    [TestMethod]
    public void EntObj_DebugView_HasAllComponents()
    {
        var ints = new List<int>() { 24, 425 };

        var ent = new EntObj();
        ent.Set<int, FirstComponent>(10);
        ent.Set<string, SecondComponent>("Hello");
        ent.Set<List<int>, ThirdComponent>(ints);

        var view = new EntDebugView(ent);
        var components = view.Components;

        var firstComponent = (EntDebugView.DebugViewComponentPrimitive)components[0];
        var secondComponent = (EntDebugView.DebugViewComponentPrimitive)components[1];
        var thirdComponent = (EntDebugView.DebugViewComponent)components[2];

        Assert.AreEqual("FirstComponent", firstComponent.Name);
        Assert.AreEqual("SecondComponent", secondComponent.Name);
        Assert.AreEqual("ThirdComponent", thirdComponent.Name);

        Assert.AreEqual(10, firstComponent.Value);
        Assert.AreEqual("Hello", secondComponent.Value);
        Assert.AreEqual(ints, thirdComponent.Value);
    }
}
