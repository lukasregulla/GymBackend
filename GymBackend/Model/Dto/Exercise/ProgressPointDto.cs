namespace GymBackend.Model.Dto.Exercise;

public class ProgressPointDto
{
    public DateOnly Date { get; set; }
    public float BestWeight { get; set; }
    public int TotalReps { get; set; }
    public int TotalSets { get; set; }
}
