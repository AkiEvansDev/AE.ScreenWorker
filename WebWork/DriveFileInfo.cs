namespace WebWork;

public class DriveFileInfo
{
    public string Id { get; }
    public long Size { get; }
    public string Name { get; set; }
    public string Description { get; set; }

    public DriveFileInfo(string id, long size, string name, string description)
    {
        Id = id;
        Size = size;
        Name = name;
        Description = description;
    }
}
