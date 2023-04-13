using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace WebAppDemo;

internal class Program
{
    static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorPages();
        WebApplication app = builder.Build();

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapRazorPages();

        // ...
        // 连接数据库
        IMongoDatabase demoDb = ConnectToMongoDB();
        // 访问 http://localhost/hello 时将返回 "Hello World!"
        app.MapGet("/hello", () => "Hello World!");
        // 访问 http://localhost/get 时将返回数据库内所有的 Cat 类型 Document
        app.MapGet("/get", async () => await GetAllCats(demoDb));
        // 访问 http://localhost/add 时将在数据库中新增一个名为 amiao 的 Cat 类型 Document
        app.MapGet("/add", async () => await InsertOneCat(demoDb, "amiao"));

        app.Run();
    }

    /// <summary> 通过环境变量获取 uri, 并连接 MongoDB </summary>
    static IMongoDatabase ConnectToMongoDB()
    {
        // 从环境变量 MONGODB_URI 中获取 uri
        string? mongoDbUri = Environment.GetEnvironmentVariable("MONGODB_URI");
        if (mongoDbUri is null)
            throw new Exception("Please set environment variable MONGODB_URI");

        // 通过 uri 连接 MongoDB
        MongoClient client = new(mongoDbUri.Replace("\"", ""));

        // 获取或创建名为 demo 的数据库
        IMongoDatabase demoDb = client.GetDatabase("demo");
        return demoDb;
    }

    /// <summary> 在名为 cats 的 Collection 中新增一个 Cat 类型的 Document </summary>
    static async ValueTask<string> InsertOneCat(IMongoDatabase db, string name)
    {
        IMongoCollection<Cat> catsCollection = db.GetCollection<Cat>("cats");
        int age = Random.Shared.Next(1, 20);
        await catsCollection.InsertOneAsync(new Cat(name, age));
        return $"name: {name}, age: {age}";
    }

    /// <summary> 从名为 cats 的 Collection 中获取所有 Cat 类型的 Document </summary>
    static async ValueTask<string> GetAllCats(IMongoDatabase db)
    {
        IMongoCollection<Cat> catsCollection = db.GetCollection<Cat>("cats");
        // 不设置筛选条件, 因此会找到 Collection 内所有的 Document
        FilterDefinition<Cat> filter = Builders<Cat>.Filter.Empty;
        var allCats = await catsCollection.FindAsync(filter);

        System.Text.StringBuilder sb = new();

        foreach (Cat? cat in allCats.ToEnumerable())
            if (cat is not null)
                sb.AppendLine($"name: {cat.name}, age: {cat.age}, id: {cat.id}");

        string result = sb.ToString();
        if (string.IsNullOrEmpty(result))
            result = "empty";
        Console.WriteLine(result);
        return result;
    }
}

public class Cat
{
    public Cat(string name, int age) =>
        (this.name, this.age) = (name, age);
    public ObjectId id;
    [BsonElement] public string? name;
    [BsonElement] public int age;
}