using Explorer.Blog.Core.Domain;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Explorer.Blog.Service.Database;

public static class MongoConfiguration
{
    private static bool _mapsRegistered;

    public static IMongoDatabase BuildDatabase()
    {
        var connection = Environment.GetEnvironmentVariable("MONGO_CONNECTION")
            ?? "mongodb://root:super@blog-mongo:27017";
        var dbName = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? "explorer-blog";

        RegisterClassMaps();

        var client = new MongoClient(connection);
        return client.GetDatabase(dbName);
    }

    private static void RegisterClassMaps()
    {
        if (_mapsRegistered) return;
        _mapsRegistered = true;

        // The Blog aggregate uses constructors + init/protected setters. The driver's
        // immutable-type convention picks the matching constructor; remaining members
        // (Id, Comments, Ratings, Rating, Status) are set after construction.
        if (!BsonClassMap.IsClassMapRegistered(typeof(BlogDom)))
        {
            // "Id" (declared on the Entity base) is auto-detected as _id by the
            // default Id convention, so we only AutoMap here.
            BsonClassMap.RegisterClassMap<BlogDom>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Comment)))
            BsonClassMap.RegisterClassMap<Comment>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });

        if (!BsonClassMap.IsClassMapRegistered(typeof(Rating)))
            BsonClassMap.RegisterClassMap<Rating>(cm => { cm.AutoMap(); cm.SetIgnoreExtraElements(true); });
    }
}
