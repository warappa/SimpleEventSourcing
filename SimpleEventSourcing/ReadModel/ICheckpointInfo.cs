namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointInfo
    {
        string StateModel { get; set; }
        int CheckpointNumber { get; set; }
    }
}
