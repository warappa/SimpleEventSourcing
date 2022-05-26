namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointInfo
    {
        // TODO: Rename
        string StateModel { get; set; }
        int CheckpointNumber { get; set; }
    }
}
