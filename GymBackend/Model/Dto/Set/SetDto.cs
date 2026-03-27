namespace GymBackend.Model.Dto.Set;

public class SetDto
{
    public int Id { get; set; }
    public int SetNumber { get; set; }
    public float WeightKg { get; set; }
    public int Reps { get; set; }
    public bool IsPersonalBest { get; set; }
    public DateTime LoggedAt { get; set; }
}
