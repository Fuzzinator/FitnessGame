public interface ISaver
{
    bool SaveRequested { get; set; }
    void Save();

    void Revert();
}
