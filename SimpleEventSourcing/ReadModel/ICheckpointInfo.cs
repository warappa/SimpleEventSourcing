namespace SimpleEventSourcing.ReadModel
{
    public interface ICheckpointInfo
    {
        // TODO: Rename
        string ProjectorIdentifier { get; set; }
        int CheckpointNumber { get; set; }
    }
}
