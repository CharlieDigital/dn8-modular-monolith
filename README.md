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

These are all the same reasons why I have advocated for "modular monoliths" ("**MoMo**") as I've worked with startups over the last few years.

## A Practical Guide in .NET

In the paper, the authors propose a solution paradigm:

> The two main parts of our proposal are (1) a programming model with abstractions that allow developers to write single-binary modular applications focused solely on business logic, and (2) a runtime for building, deploying, and optimizing these applications.

While the proposed solution design in the paper introduces appropriate complexity for Google's concerns, we can build a simpler version of this paradigm that can be scaled accordingly in complexity as needed (e.g. replace the communication mechanism with an event bus like SQS or Google Pub/Sub).

.NET feels particularly suited for this because of [.NET's generic host model](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host?tabs=appbuilder).  In summary, whether you're running an API or a console app, in .NET, it can be run as the workload of a "host".

In this model, the reponsibility of the host is to configure the runtime by loading the required services and configuration and then executing those services.  In other words, *it separates the application and business logic from the runtime and how the application is deployed*.

### Our Sample Domain

In this example, our domain is a simple project management application with only 3 entities:

1. `Project`.  Our tasks or `WorkItem`s are associated with a `Project`.
2. `WorkItem`.  Each `WorkItem` is linked to a single `Project` and has one or more `User` collaborators.
3. `User`.  A simple entity representig the assignees of the `WorkItem`s

> 💡 I didn't use the nomenclature `Task` as this is a system type name.

We would like our system to do two things when a `WorkItem` has its status set to `Completed`:

1. Send a notification to each of the `User` collaborators
2. Check to see if all of the `WorkItem`s for the `Project` are completed and, if so, mark the `Project` as completed.

In our MoMo, we still want to decouple these two actions from the main action of updating the the status of the task.  But the question is how we can build the code in such a way that we can minimize the DX friction of running multiple services locally in dev while still retaining the option to scale these services independently from an operational standpoint.

We can achieve this in .NET by simply using multiple hosts to run services that reside in a single monolithic binary.

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