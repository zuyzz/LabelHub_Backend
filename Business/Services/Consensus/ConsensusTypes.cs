namespace DataLabelProject.Business.Services.Consensus;

public class BoxCandidate
{
    public Guid AnnotatorId { get; set; }
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
}

public class BoxCluster
{
    public List<BoxCandidate> Members { get; set; } = new();
}

public class ConsensusBboxDto
{
    public string Label { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public int Support { get; set; }
}
