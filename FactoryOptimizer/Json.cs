public class Descriptor
{
    public ClassDescriptor[] Classes { get; set; }
}

public class ClassDescriptor
{
    public string ClassName { get; set; }
    public string mDisplayName { get; set; }
    public string mIngredients { get; set; }
    public string mProduct { get; set; }
    public string mManufactoringDuration { get; set; }
    public string mProducedIn { get; set; }
}