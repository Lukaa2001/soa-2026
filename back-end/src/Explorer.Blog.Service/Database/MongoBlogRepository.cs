using Explorer.Blog.Core.Domain;
using Explorer.Blog.Core.Domain.RepositoryInterfaces;
using Explorer.BuildingBlocks.Core.Domain;
using Explorer.BuildingBlocks.Core.UseCases;
using MongoDB.Driver;

namespace Explorer.Blog.Service.Database;

// MongoDB-backed persistence for the Blog aggregate. A blog is stored as a single
// document with its comments and ratings embedded (natural document model).
public class MongoBlogRepository : ICrudRepository<BlogDom>, IBlogRepository
{
    private readonly IMongoCollection<BlogDom> _blogs;
    private readonly IMongoCollection<Counter> _counters;

    public MongoBlogRepository(IMongoDatabase database)
    {
        _blogs = database.GetCollection<BlogDom>("blogs");
        _counters = database.GetCollection<Counter>("counters");
    }

    public PagedResult<BlogDom> GetPaged(int page, int pageSize)
    {
        var total = (int)_blogs.CountDocuments(FilterDefinition<BlogDom>.Empty);
        var query = _blogs.Find(FilterDefinition<BlogDom>.Empty).SortBy(b => b.Id);
        var items = (page > 0 && pageSize > 0)
            ? query.Skip((page - 1) * pageSize).Limit(pageSize).ToList()
            : query.ToList();
        return new PagedResult<BlogDom>(items, total);
    }

    public BlogDom Get(long id)
    {
        var blog = _blogs.Find(b => b.Id == id).FirstOrDefault();
        if (blog == null) throw new KeyNotFoundException($"Blog {id} not found.");
        return blog;
    }

    public BlogDom GetUntracked(long id) => Get(id);

    public BlogDom Create(BlogDom entity)
    {
        SetId(entity, NextId());
        _blogs.InsertOne(entity);
        return entity;
    }

    public BlogDom Update(BlogDom entity)
    {
        var result = _blogs.ReplaceOne(b => b.Id == entity.Id, entity);
        if (result.MatchedCount == 0) throw new KeyNotFoundException($"Blog {entity.Id} not found.");
        return entity;
    }

    public void Delete(long id)
    {
        var result = _blogs.DeleteOne(b => b.Id == id);
        if (result.DeletedCount == 0) throw new KeyNotFoundException($"Blog {id} not found.");
    }

    // IBlogRepository
    public BlogDom? GetById(long blogId) => _blogs.Find(b => b.Id == blogId).FirstOrDefault();

    public BlogDom UpdateBlog(BlogDom blog) => Update(blog);

    private long NextId()
    {
        var filter = Builders<Counter>.Filter.Eq(c => c.Id, "blogs");
        var update = Builders<Counter>.Update.Inc(c => c.Seq, 1L);
        var options = new FindOneAndUpdateOptions<Counter>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After
        };
        var counter = _counters.FindOneAndUpdate(filter, update, options);
        return counter.Seq;
    }

    // Entity.Id has a protected setter; set it through reflection on create.
    private static void SetId(Entity entity, long id)
        => typeof(Entity).GetProperty(nameof(Entity.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(entity, new object[] { id });
}

public class Counter
{
    public string Id { get; set; } = string.Empty;
    public long Seq { get; set; }
}
