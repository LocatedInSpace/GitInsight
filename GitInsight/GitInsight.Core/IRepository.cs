namespace GitInsight.Core;

public interface IRepository
{
    // update acts as a create as well
    Response Update(RepositoryEntryDTO entry);
    RepositoryEntryDTO? Find(RepositoryFindDTO info);
}