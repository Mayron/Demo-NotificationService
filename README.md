# Description
This repository demonstrates a bug with the MediatR NuGet package. The following test demonstrates the failure caused by the bug:
https://github.com/Mayron/Demo-NotificationService/blob/master/NotificationService.IntegrationTests/UsersApiIntegrationTests.cs#L43

![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/a14b59b0-9d09-4ab1-a8a8-5a412c784d05)

The test hits this API endpoint:
https://github.com/Mayron/Demo-NotificationService/blob/master/NotificationService.Api/Controllers/UsersApiController.cs#L22

The action for this endpoint publishes 4 MediatR notifications:
![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/d04da39e-147c-4d0b-949d-6c68ba535977)

However, because the `AuditLogNotificationHandler<T> where T: AuditLogNotification` accepts a generic type argument, this causes the `NewUserNotificationHandler` to be executed twice.
The `NewUserNotificationHandler` type has no generic type arguments assigned to it and should not be influenced by the `AuditLogNotificationHandler` in any way.

**NOTE:** This only occurs when the `ValidateOnBuild` service provider option is set to `true`:
Please refer to the [Theories for why this bug is happening](#theories-for-why-this-bug-is-happening) section for a detailed explanation of how this option is interfering with MediatR.

![image](https://github.com/Mayron/Demo-NotificationService/assets/5854995/e2c1f344-30c7-4d04-93aa-5e8828fd8b38)

# Important
There are 2 ways to make the above test pass, but neither is ideal:

1️⃣ Remove the generic type argument from the `AuditLogNotificationHandler`.

```diff
- public class AuditLogNotificationHandler<T> : INotificationHandler<T> where T : AuditLogNotification
+ public class AuditLogNotificationHandler : INotificationHandler<AuditLogNotification>
```

The problem with this is that the `AuditLogNotificationHandler` class currently works. Removing this means that it no longer handles subclasses of `AuditLogNotification` and the other test included in this repository then fails.

```csharp
// ERROR: Does not hit the handler!
await _mediator.Publish(new UserAuditLogNotification("New User Created", newUser.Id));
// ERROR: Does not hit the handler!
await _mediator.Publish(new AccountAuditLogNotification("User Joined Account", accountId));
// SUCCESS!
await _mediator.Publish(new AuditLogNotification("User Created", true));
```

You could argue splitting the `AuditLogNotificationHandler` handler into 3 handlers to handle each `AuditLogNotification` subtype but this does not scale well.
The use of the `AuditLogNotificationHandler` handler in this repository is simple and a real working example may have a lot of shared logic that can mostly act on the base `AuditLogNotification` type.
If a new audit log notification subtype was implemented, it should be able to work with the existing logic; Requiring a new handler for each type would be bad practice.

Finally, being able to use generic type arguments for notification handlers is a good feature that works and we should not be discouraged from using it.

2️⃣ Set `ValidateOnBuild` to false.

This is the best temporary solution because it does not break the application's functionality.
However, this also means that the IoC service dependency validation step is ignored during the build time. 

Disabling this validation step has caused errors to slip into production (speaking from experience, which led to the discovery of this MediatR bug).
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
