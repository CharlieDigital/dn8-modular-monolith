# A Practical Guide to Modular Monoliths (MoMo) with .NET

## Introduction

In the last decade, "microservices" architectures have come into fashion for a variety of reasons.

Like "Agile", this term has taken on many meanings and interpretations over the years and -- in many cases -- have come to represent even not-so-micro-services.  From true microservices like single-purpose serverless functions to "macroservices" where some parts of the system are broken out into different services connected by remote API calls, I have never been a fan of such architectures because of the inherent complexities and developer experience (DX) friction that accompanies such design choices.

It is not to say that there are no use cases for such services, but that the paradigm itself is often inappropriately overused and adds cost to every action.

This is especially true for startups where microservices can add too much complexity too early -- whether it's development complexity, deployment complexity, or operational complexity.

In more recent years, there has been a backlash against this movement.

In his essay *[Complexity is killing software developers](https://www.infoworld.com/article/3639050/complexity-is-killing-software-developers.html)*, Scott Carey writes:

> The shift from building applications in a monolithic architecture hosted on a server you could go and touch, to breaking them down into multiple microservices, packaged up into containers, orchestrated with Kubernetes, and hosted in a distributed cloud environment, marks a clear jump in the level of complexity of our software.
>
> “There is a clear increase in complexity when you move to such a pervasive microservices environment,” said Amazon CTO Werner Vogels during the AWS Summit in 2019. “Was it easier in the days when everything was in a monolith? Yes, for some parts definitely.”

Carey makes the case that such complexity is "accidental" as opposed to "necessary".

Jason Warner, former CTO of GitHub, [wrote in tweet in 2022](https://twitter.com/jasoncwarner/status/1592227285024636928):

> I'm convinced that one of the biggest architectural mistakes of the past decade was going full microservice
>
> On a spectrum of monolith to microservices, I suggest the following:
>
> Monolith > apps > services > microservices

Now [a new paper](https://dl.acm.org/doi/pdf/10.1145/3593856.3595909) titled *Towards Modern Development of Cloud Applications* has been released by a team from Google that throws more fuel on the (dumpster) fire of microservices architectures:

> When writing a distributed application, conventional wisdom says to split your application into separate services that can be rolled out independently. This approach is well-intentioned, but a microservices-based architecture like this often backfires, introducing challenges that counteract the benefits the architecture tries to achieve. Fundamentally, this is because microservices conflate logical boundaries (how code is written) with physical boundaries (how code is deployed).

In it, the authors cite 5 reasons why such practices can end up hindering teams:

1. **It hurts performance**. The overhead of serializing data and sending it across the network is increasingly becoming a bottleneck
2. **It hurts correctness**. It is extremely challenging to reason about the interactions between every deployed version of every microservice.
3. **It is hard to manage**. Rather than having a single bi-nary to build, test, and deploy, developers have to manage 𝑛 different binaries, each on their own release schedule.
4. **It freezes APIs**. Once a microservice establishes an API, it becomes hard to change without breaking the other services that consume the API.
5. **It slows down application development**. When mak-ing changes that affect multiple microservices, developers cannot implement and deploy the changes atomically.

These reasons are particularly important to consider in the context of a startup as the small team sizes and importance of speed of iteration and deployment negate many of the reasons why a microservices approach might be better (for example, team boundaries that define procedural boundaries).

For startups, understanding how to build "modular monoliths" ("**MoMo**") can help increase velocity by reducing development, deployment, and operational complexity.

## A Practical Guide in .NET

In the paper, the authors propose a solution paradigm:

> The two main parts of our proposal are (1) a programming model with abstractions that allow developers to write single-binary modular applications focused solely on business logic, and (2) a runtime for building, deploying, and optimizing these applications.

While the proposed solution design in the paper introduces appropriate complexity for Google's concerns, it is possible to build a simpler version of this paradigm that can be scaled accordingly in complexity as needed (e.g. replace the communication mechanism with an event bus like SQS or Google Pub/Sub).

.NET feels particularly suited for this because of its [generic host model](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder).  In summary, whether the goal is to run a REST API or a console app, in .NET, it can be run as the service workload of a "host".

In this model, the responsibility of the host is to simply configure the runtime by loading the required services and configuration and then executing those services.  In other words, *it separates the application and business logic from the runtime and how the application is deployed*.

### Our Sample Domain

In this example, the domain is a simple project management application with only 3 entities:

1. `Project`.  Our tasks or `WorkItem`s are associated with a `Project`.
2. `WorkItem`.  Each `WorkItem` is linked to a single `Project` and has one or more `User` collaborators.
3. `User`.  A simple entity representing the assignees of the `WorkItem`s

> 💡 I didn't use the nomenclature `Task` as this is a system type name.

The system is responsible for doing two things when a `WorkItem` has its status set to `Completed`:

1. Send a notification to each of the `User` collaborators
2. Check to see if all of the `WorkItem`s for the `Project` are completed and, if so, mark the `Project` as completed.

In this modular monolith, decoupling these two actions from the main action of updating the the status of the task remains a goal.  However, by choosing to contain the core logic in a single binary that can be parceled at the runtime level, it affords several luxuries as described by the authors of the Google paper such as better local DX.

This example is kept quite basic by integrating at the database layer, but it is not hard to imagine how this could also be done with eventing instead through SQS or Google Cloud Pub/Sub.

### Separating Service from Runtime

In total, the system will have 3 components:

1. The API that contains all of the logic to mutate the state of the system from external callers
2. The `WorkItemMonitorService` which will periodically check for new `WorkItem`s
3. The `WorkItemStatusMonitorService` which will periodically check for the status of `WorkItem`s for a given `Project`

A key to making this work is to separate the *logic* of the service from the *runtime* container for the service.  This is quite straightforward and natural with .NET's host paradigm because of the nature of the dependency injection container as will be evident in a moment.

All of the application logic is implemented in the `core` project and in development mode on the local machine, the API runtime simply loads both of the services:

```csharp
// The core API runtime, SetupServicesExtension.cs
// True when ASPNETCORE_ENVIRONMENT or DOTNET_ENVIRONMENT is "Development"
if (RuntimeEnv.IsDevelopment) {
  services.AddHostedService<WorkItemMonitorService>();

  services.AddHostedService<WorkItemStatusMonitorService>();
}
```

When running this in production, neither service is loaded into the API runtime which makes it suitable for scale-to-zero serverless container runtimes like Google Cloud Run or Azure Container Apps.  Without such a consideration, an option is to flag which services to add and simply have one single container image that can run difference "slices" of our codebase by simply loading different services based on environment variables.

To set up the other service runtimes, modifying `svc.csproj` to automatically copy the configuration files over on build can reduce mistakes from manually copying and synchronizing configuration files:

```xml
<!--
  Copies the runtime configuration
-->
<Target Name="CopyRuntimeConfig" AfterTargets="Build">
  <Message Text="Copying runtime config..." Importance="high" />
  <Exec Command="cp ../core/appsettings*.json ." />
</Target>
```

The service runtime simply sets up the dependency injection container and loads the relevant services:

```csharp
builder.Services.Configure<MoMoConfig>(
  builder.Configuration.GetSection(nameof(MoMoConfig))
);

var role = Environment.GetEnvironmentVariable("SVC_ROLE");

if (role == nameof(WorkItemMonitorService)) {
  // Run single service
  Console.WriteLine("Loading WorkItemMonitorService service");
  builder.Services.AddHostedService<WorkItemMonitorService>();
} else if (role == nameof(WorkItemStatusMonitorService)) {
  // Run single service
  Console.WriteLine("Loading WorkItemStatusMonitorService service");
  builder.Services.AddHostedService<WorkItemStatusMonitorService>();
} else {
  // Run both services
  Console.WriteLine("Loading both services");
  builder.Services.AddHostedService<WorkItemMonitorService>();
  builder.Services.AddHostedService<WorkItemStatusMonitorService>();
}

builder.Services.AddDataStore();

var host = builder.Build();
host.Run();
```

Simply by switching the environment variable, it is possible to determine which services to load into this runtime.

The Docker Compose file `docker-compose-run.yaml` shows how this might look:

```yaml
  # We run the work item status monitor
  svc1:
    image: momo/svc1
    build:
      context: ./
      dockerfile: ./Dockerfile.svc
    environment:
      - SVC_ROLE=WorkItemStatusMonitorService
    depends_on:
      - api
    networks:
      - momo

  # We run the new work item monitor
  svc2:
    image: momo/svc2
    build:
      context: ./
      dockerfile: ./Dockerfile.svc
    environment:
      - SVC_ROLE=WorkItemMonitorService
    depends_on:
      - api
    networks:
      - momo
```

Deployed as containers to Google Cloud Run, Azure Container Apps, or AWS ECS Fargate, it is possible to simply have one codebase and configure the environment variable on the container at runtime.  This is very efficient from a build perspective as there is only a need to build the one image despite having two distinct services because of the monolithic codebase.

Alternatively, it is possible to move the environment variable into the `.Dockerfile` and simply create multiple files instead.

This approach affords several positive outcomes:

1. The local DX is excellent: a single runtime to start, trace, and debug; there is no need to run multiple processes and perform complex tracing of inter-process data flows for local development.
2. Packaging and deployment is straight forward and all dependencies ship at the once.  The `.Dockerfile` shows just how straight forward it is to ship this.  In CI/CD, there's no need to set up complex build pipelines to build dependent projects and generate new bindings.
3. It is possible to deploy this application into a variety of topologies depending on how we need it to scale.  For example, a load balancer sitting in front of two API only instances and a single instance running both services.  If there is a need to scale the two services independently, it is possible to change the environment variable on the runtime to and split two instances out.

## Running the Sample

To run the sample in this repository, you'll need to have Docker installed.

There are two modes:

1. **Development mode**.  In this mode, we run all of the services in one single host
2. **Runtime mode**. In this mode, we run each of the services in separate hosts

### Development Mode

To run in development mode:

```shell
# Start the Postgres container
docker compose up

# API is at port 5228
cd src/core
dotnet run
```

In this mode, the Docker Compose file is responsible only for running our Postgres containers.  We load all of our services into a single .NET host runtime which includes our REST API as well as our two background services which perform notification and updates to the `Project`.

### Runtime Mode

To run in runtime mode:

```shell
# API is at port 8080
docker compose -f docker-compose-run.yaml up --build
```

In this mode, the Docker Compose file is responsible for building and running 4 containers:

1. Postgres
2. The .NET host for the API; we don't load the other services
3. The .NET host for the service to notify `WorkItem` collaborator `User`s
4. The .NET host for the service to check and update the `Project` for a `WorkItem` if all of the `Project`'s `WorkItem`s are completed.

Each container contains a host that loads a slice of the workload.

### Testing

To test the example, use the following URLs to access the Swagger UI:

```
# In development mode
http://localhost:5228/swagger

# In runtime mode
http://localhost:8080/swagger
```

From here, follow these steps:

1. Add a **Project** and get the ID from the response
2. Add a **User** and keep the ID of the user
3. Add a **Work Item** and link it to the **Project** and **User**
4. The service `WorkItemMonitorService` will detect the new **Work Item** and simulate sending notifications to the collaborators
5. Update the status of the **Work Item** to **Completed**
6. The service `WorkItemStatusMonitorService` will detect changes in **Work Item** status and check the **Project** to see if all of the **Work Item**s are completed and if so, update the status of the **Project** to **Completed** as well.

To execute this in `curl`:

```bash
# Add the project and note the response ID
curl -X 'POST' \
  'http://localhost:5228/api/projects/add' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '"Test"'

# Assume we get an ID of 8b90f42c-af9f-4c61-9b18-52e764f0ca8c

# Add a user:
curl -X 'POST' \
  'http://localhost:5228/api/users/add' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "User 1",
  "createdUtc": "2024-01-14T23:45:38.591Z",
  "email": "user1@example.com"
}'

# Now add a work item using the project ID and user ID:
curl -X 'POST' \
  'http://localhost:5228/api/tasks/add' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '{
  "projectId": "8b90f42c-af9f-4c61-9b18-52e764f0ca8c",
  "name": "Task 1",
  "collaborators": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6"
  ]
}'

# Assume we get an ID of: faba6123-183d-405d-8751-9736ed4a243b

# This will log:
# [NEW] Found new task Task 1 on project Test
#   ⮑  Notifying user User 1

# Finally, let's update the status with our work item ID:
curl -X 'POST' \
  'http://localhost:5228/api/tasks/faba6123-183d-405d-8751-9736ed4a243b/status' \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d '"Completed"'
```

Console output when a new task is detected:

![Console output when a new task is detected](./images/new-workitem.png)

Console output when the task status is updated to `Completed`:

![Console output when the task status is updated to Completed](./images/update-project-status.png)