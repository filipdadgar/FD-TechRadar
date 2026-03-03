namespace TechRadar.Core.Domain;

public enum Quadrant
{
    ConnectivityProtocols = 0,
    EdgePlatforms = 1,
    ToolsAndFrameworks = 2,
    StandardsAndTechniques = 3
}

public enum Ring
{
    Adopt = 0,
    Trial = 1,
    Assess = 2,
    Hold = 3
}

public enum EntryStatus
{
    Active = 0,
    Archived = 1
}

public enum ProposalType
{
    NewEntry = 0,
    RingChange = 1
}

public enum ProposalStatus
{
    Pending = 0,
    Accepted = 1,
    EditedAndAccepted = 2,
    Rejected = 3
}

public enum SourceType
{
    RssFeed = 0,
    GitHubTopics = 1,
    NewsApi = 2
}

public enum RunStatus
{
    Running = 0,
    Completed = 1,
    Failed = 2
}
