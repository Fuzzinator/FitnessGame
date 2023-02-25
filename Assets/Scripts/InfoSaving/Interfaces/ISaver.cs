public interface ISaver
{
    bool SaveRequested { get; set; }
    void Save(Profile overrideProfile = null);

    void Revert();
}
