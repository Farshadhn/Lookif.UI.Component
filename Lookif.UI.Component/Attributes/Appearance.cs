[System.AttributeUsage(System.AttributeTargets.Property)]
public class AppearanceAttribute : System.Attribute
{
    public int LabelSize { get; set; }  
    public int DivSize { get; set; }
    public int InputSize { get; set; }
    public AppearanceAttribute(int divSize ,int labelSize, int inputSize)
    {
        DivSize = divSize;
        LabelSize = labelSize;
        InputSize = inputSize;
    }

   
}
 

public class Appearance
{
    public int LabelSize { get; set; }
    public int DivSize { get; set; }
    public int InputSize { get; set; }
    public Appearance( int divSize,int labelSize, int labelInputSize)
    {
        LabelSize = labelSize;
        InputSize = labelInputSize;
        DivSize = divSize;
    }

}