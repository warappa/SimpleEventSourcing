namespace SimpleEventSourcing.WriteModel
{
    public interface IHasCheckpointNumber
    {
        int CheckpointNumber { get; set; }
    }
}
