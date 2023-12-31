# About
This repository demonstrates a bug with the MediatR NuGet package (version 12.1.1, the current latest version as of writing) when using notification handlers with generic type arguments and the IoC service provider's `ValidateOnBuild` config option set to true.
The bug may not directly be a MediatR bug, but it does mean that MediatR is incompatible with the IoC service provider's validation build step.

When adding MediatR to ASP.Net Core's default IoC container, it might be possible to tweak how INotificationHandlers are registered to fix this issue.

Link to the submitted issue on the MediatR repository: https://github.com/jbogard/MediatR/issues/948

# Bug Description
The following test demonstrates the failure caused by the bug:
https://github.com/Mayron/Demo-NotificationService/blob/master/NotificationService.IntegrationTests/UsersApiIntegrationTests.cs#L17

![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/a14b59b0-9d09-4ab1-a8a8-5a412c784d05)

The test hits this API endpoint:
https://github.com/Mayron/Demo-NotificationService/blob/master/NotificationService.Api/Controllers/UsersApiController.cs#L22

The action for this endpoint publishes 4 MediatR notifications:
![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/d04da39e-147c-4d0b-949d-6c68ba535977)

A `NewUserNotification` is published only once but triggers the same `NewUserNotificationHandler` twice, resulting in 2 User objects being added to a dummy User repository class.
If the functionality worked as expected, there should only be 1 new User object created and added to this repository.

However, because the `AuditLogNotificationHandler<T> where T: AuditLogNotification` accepts a generic type argument, this causes the `NewUserNotificationHandler` to be executed twice.
The `NewUserNotificationHandler` type has no generic type arguments assigned to it and should not be influenced by the `AuditLogNotificationHandler` in any way.

**NOTE:** This only occurs when the `ValidateOnBuild` service provider option is set to `true`. Please refer to the [Theories for why this bug is happening](#theories-for-why-this-bug-is-happening) section for a detailed explanation of how this option is interfering with MediatR.

![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/e2c1f344-30c7-4d04-93aa-5e8828fd8b38)

# Temporary Fixes and Why it is Still a Problem
There are 2 ways to make the above test pass, but neither is ideal:

1️⃣ Remove the generic type argument from the `AuditLogNotificationHandler`.

```diff
- public class AuditLogNotificationHandler<T> : INotificationHandler<T> where T : AuditLogNotification
+ public class AuditLogNotificationHandler : INotificationHandler<AuditLogNotification>
```

The problem with this is that the `AuditLogNotificationHandler` class currently works. Removing this means that it no longer handles subclass types of `AuditLogNotification` and the other test included in this repository then fails.

```csharp
// ERROR: Does not hit the handler!
await _mediator.Publish(new UserAuditLogNotification("New User Created", newUser.Id));
// ERROR: Does not hit the handler!
await _mediator.Publish(new AccountAuditLogNotification("User Joined Account", accountId));
// SUCCESS!
await _mediator.Publish(new AuditLogNotification("User Created", true));
```

You could argue splitting the `AuditLogNotificationHandler` handler into 3 handlers to handle each `AuditLogNotification` subtype will solve this but this does not scale well.
The use of the `AuditLogNotificationHandler` handler in this repository is simple and a more complex real-world example may require a lot of shared logic that can mostly act on the base `AuditLogNotification` type.
If a new audit log notification subtype was added, it should be able to work with the existing logic and require minimal changes; Requiring a new handler for each type would be bad practice.

More importantly, being able to use generic type arguments for notification handlers is a good feature that works and we should not be discouraged from using it.

2️⃣ Set `ValidateOnBuild` to false.

This is the best temporary solution because it does not break the application's functionality.
However, this also means that the IoC service dependency validation step is ignored during the build time. 

Disabling this validation step has caused errors to slip into production (speaking from experience, which led to the discovery of this bug).
Instead of ensuring services can be constructed during the build process, it resolves them at runtime when needed, meaning production failures may only trigger when some arbitrary application feature is accessed by a user.

I have found this validation step to be a vital metric for ensuring a pull request is ready for production.

# Theories for Why This Bug is Happening

During the IoC service validation build step, the framework checks if each registered service can be resolved.
The process then seems to create 2 service descriptors, one for the service and its interface (as expected) but then a 2nd one for an array containing that service and its interface:

- Service Type: `INotificationHandler<NewUserNotification>`, Implementation Type: `NewUserNotificationHandler`
- Service Type: `INotificationHandler<NewUserNotification>[]` Implementation Type: `NewUserNotificationHandler[]`

I think this validation process, or how MediatR is registering the services, may be ignoring or not correctly catering to the generic type constraint on the `AuditLogNotificationHandler<T> : INotificationHandler<T> where T : AuditLogNotification` class declaration,
resulting in any service that implements INotificationHandler<> being registered twice as an array and as a standalone service. 

Then, MediatR appears to be calling `GetInstances` to get the registered instances of handlers that can handle the `NewUserNotification` and incorrectly receiving 2 handlers where both handlers are instances of `NewUserNotificationHandler`:

![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/799f7312-6ed8-46a3-9865-8992e3959f5e)
![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/2185b894-169f-42f1-b380-39aa7b7b69a8)
