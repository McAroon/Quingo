var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("postgresql-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: sqlPassword)
    .WithImage("postgres")
    .WithImageTag("latest")
    .WithDataVolume()
    .WithPgWeb()
    .WithLifetime(ContainerLifetime.Session);
    
    
var quingoDb = postgres.AddDatabase("quingoDb");

var quingoApp = builder.AddProject<Projects.Quingo>("quingoApp")
    .WithReference(quingoDb).WaitFor(quingoDb);

builder.Build().Run();